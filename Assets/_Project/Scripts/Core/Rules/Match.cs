using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The rules engine for one match. Holds all game state and is the only thing that changes it.
    /// Has no Unity dependencies, so it can be unit tested, run on a server, or driven by an AI.
    ///
    /// Each round is a day then a night:
    /// 1. Day starts: every card's Day effects fire, then Dark Forest's Ghouls eat (<see cref="GhoulFeeding"/>). Then each day player (Farm, Forest, Factory) takes a turn.
    /// 2. Night starts: every card's Night effects fire. Then each night player (Dark Forest) takes a turn.
    /// A turn is: draw, then play up to <see cref="MatchConfig.cardsPerTurn"/> cards, then <see cref="EndTurn"/>.
    /// A phase with nobody to play (night in Farm vs Forest) just fires its effects.
    ///
    /// The match ends when a player reaches their team's goal, or when the board is full
    /// (most squares wins; a tie is a draw).
    /// </summary>
    public class Match
    {
        private readonly EffectRegistry _effects;
        private readonly IPlacementRule _placement;
        private readonly Func<PlayerState, ITeamGoal> _goalFor;
        private readonly Dictionary<PlayerState, ITeamGoal> _goals = new Dictionary<PlayerState, ITeamGoal>();
        private readonly List<PlayerState> _players = new List<PlayerState>();
        private readonly List<GameEvent> _log = new List<GameEvent>();
        private readonly List<PlayerState> _phaseQueue = new List<PlayerState>();
        private int _queuePos;
        private int _nextInstanceId = 1;
        private readonly Dictionary<string, CardDefinition> _cardsById = new Dictionary<string, CardDefinition>(StringComparer.Ordinal);

        public MatchConfig Config { get; }
        public Board Board { get; private set; }
        public GameRandom Random { get; }
        public IReadOnlyList<PlayerState> Players => _players;
        public IReadOnlyList<GameEvent> Log => _log;

        /// <summary>Whose turn it is. Null between phases and once the match is over.</summary>
        public PlayerState CurrentPlayer { get; private set; }

        /// <summary>Counts every player-turn taken, starting at 1.</summary>
        public int TurnNumber { get; private set; }
        /// <summary>One round is a day and the night after it. Starts at 1.</summary>
        public int Round { get; private set; }
        public TimeOfDay TimeOfDay { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsOver { get; private set; }
        /// <summary>The winning alliance, or null for a draw / match not over.</summary>
        public int? WinningAllianceId { get; private set; }
        /// <summary>Why the match ended, e.g. "Farm reached its goal" or "The board is full".</summary>
        public string EndReason { get; private set; }

        /// <summary>Raised for everything that happens. Subscribe from the presentation layer.</summary>
        public event Action<GameEvent> EventRaised;

        /// <param name="goalFor">Override a player's win condition. Defaults to <see cref="TeamRules.DefaultGoal"/>.</param>
        public Match(MatchConfig config, EffectRegistry effects = null, IPlacementRule placement = null,
            Func<PlayerState, ITeamGoal> goalFor = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _effects = effects ?? EffectRegistry.CreateDefault();
            _placement = placement ?? new TouchOwnCardPlacement();
            _goalFor = goalFor ?? (p => TeamRules.DefaultGoal(p.Team, Config));
            Random = new GameRandom(config.seed);
        }

        public ITeamGoal GoalOf(PlayerState player) => _goals.TryGetValue(player, out var g) ? g : null;

        // ------------------------------------------------------------------ setup

        public void Start()
        {
            if (IsStarted) throw new InvalidOperationException("Match already started.");
            int expected = GameModes.PlayerCount(Config.mode);
            if (Config.players == null || Config.players.Count != expected)
                throw new InvalidOperationException(Config.mode + " needs " + expected + " players, got " +
                                                    (Config.players?.Count ?? 0) + ".");

            int size = Config.BoardSizeFor(expected);
            Board = new Board(size, size, Config.adjacency);
            var seats = GameModes.Seats(Config.mode);

            for (int i = 0; i < expected; i++)
            {
                var setup = Config.players[i];
                var player = new PlayerState(i, string.IsNullOrEmpty(setup.name) ? "Player " + (i + 1) : setup.name,
                    setup.team, seats[i].Corner, seats[i].AllianceId);
                _players.Add(player);
                _goals[player] = _goalFor(player);

                foreach (var def in setup.deck ?? new List<CardDefinition>())
                    player.Deck.Add(CreateInstance(def, player));
                Random.Shuffle(player.Deck);

                player.Resources.Add(Config.startingResources);

                if (setup.startingCard != null)
                {
                    var start = CreateInstance(setup.startingCard, player);
                    PutOnBoard(start, player.Corner.ToCoord(Board.Width, Board.Height));
                    player.StartingCard = start;
                }
            }

            IndexCards();
            IsStarted = true;
            Raise(new GameEvent { Type = GameEventType.MatchStarted, Message = "Match started: " + Config.mode });

            foreach (var p in _players) DrawCards(p, Config.startingHandSize);

            Round = 1;
            BeginPhase(TimeOfDay.Day);
        }

        /// <summary>Looks up a card definition by id among the catalog, every deck and every starting card.</summary>
        public CardDefinition FindCard(string id) =>
            id != null && _cardsById.TryGetValue(id, out var def) ? def : null;

        private void IndexCards()
        {
            void Index(CardDefinition def)
            {
                if (def != null && !string.IsNullOrEmpty(def.id) && !_cardsById.ContainsKey(def.id)) _cardsById[def.id] = def;
            }
            foreach (var def in Config.cardCatalog ?? new List<CardDefinition>()) Index(def);
            foreach (var setup in Config.players)
            {
                Index(setup.startingCard);
                foreach (var def in setup.deck ?? new List<CardDefinition>()) Index(def);
            }
        }

        public CardInstance CreateInstance(CardDefinition def, PlayerState owner) =>
            new CardInstance(_nextInstanceId++, def, owner);

        // ------------------------------------------------------------------ days, nights and turns

        private void BeginPhase(TimeOfDay time)
        {
            while (!IsOver)
            {
                TimeOfDay = time;
                CurrentPlayer = null;

                if (time == TimeOfDay.Day)
                {
                    foreach (var p in _players) p.GainedThisRound.Clear();
                    Raise(new GameEvent { Type = GameEventType.DayStarted, Message = "Day " + Round + " begins" });
                }
                else
                {
                    Raise(new GameEvent { Type = GameEventType.NightStarted, Message = "Night " + Round + " falls" });
                }

                FirePhaseEffects(time);
                if (time == TimeOfDay.Day && !IsOver) GhoulFeeding.FeedAll(this);
                CheckGoals();
                if (IsOver) return;

                _phaseQueue.Clear();
                _phaseQueue.AddRange(_players.Where(p => p.PlaysDuring == time));
                _queuePos = 0;
                if (_phaseQueue.Count > 0)
                {
                    BeginTurn(_phaseQueue[0]);
                    return;
                }

                // Nobody plays in this phase: move straight on to the next one.
                if (!AdvanceTime(ref time)) return;
            }
        }

        /// <summary>Moves day to night, or night to the next day. Returns false if that ended the match.</summary>
        private bool AdvanceTime(ref TimeOfDay time)
        {
            if (time == TimeOfDay.Day)
            {
                time = TimeOfDay.Night;
                return true;
            }

            Round++;
            if (Config.roundLimit > 0 && Round > Config.roundLimit)
            {
                EndByMostSquares("Round limit reached");
                return false;
            }
            time = TimeOfDay.Day;
            return true;
        }

        private void BeginTurn(PlayerState player)
        {
            CurrentPlayer = player;
            TurnNumber++;
            player.CardsPlayedThisTurn = 0;
            Raise(new GameEvent
            {
                Type = GameEventType.TurnStarted, Player = player,
                Message = "Turn " + TurnNumber + " (" + TimeOfDay + " " + Round + "): " + player
            });

            bool skipDraw = TurnNumber == 1 && Config.firstPlayerSkipsDraw;
            if (!skipDraw) DrawCards(player, Config.drawsPerTurn);
        }

        /// <summary>Ends the current player's turn. Returns false if it isn't their turn or the match is over.</summary>
        public bool EndTurn(int playerId)
        {
            if (!IsStarted || IsOver || CurrentPlayer == null || CurrentPlayer.Id != playerId) return false;

            var player = CurrentPlayer;
            Raise(new GameEvent { Type = GameEventType.TurnEnded, Player = player, Message = player + " ended their turn" });

            _queuePos++;
            if (_queuePos < _phaseQueue.Count)
            {
                BeginTurn(_phaseQueue[_queuePos]);
                return true;
            }

            var time = TimeOfDay;
            if (AdvanceTime(ref time)) BeginPhase(time);
            return true;
        }

        // ------------------------------------------------------------------ playing cards

        public bool CanPlaceOn(PlayerState player, CardDefinition card, Coord square) =>
            Board.InBounds(square) && _placement.CanPlace(this, player, card, square);

        /// <summary>All squares this card could go on right now, ignoring cost and turn.</summary>
        public IEnumerable<Coord> LegalSquares(PlayerState player, CardDefinition card)
        {
            for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
            {
                var c = new Coord(x, y);
                if (CanPlaceOn(player, card, c)) yield return c;
            }
        }

        /// <summary>Checks whether a play is legal without changing anything.</summary>
        public PlayResult CheckPlay(int playerId, CardInstance card, Coord square)
        {
            if (!IsStarted || IsOver) return PlayResult.GameOver;
            var player = CurrentPlayer;
            if (player == null || player.Id != playerId) return PlayResult.NotYourTurn;
            if (card == null || !player.Hand.Contains(card)) return PlayResult.CardNotInHand;
            if (player.CardsPlayedThisTurn >= Config.cardsPerTurn) return PlayResult.NoPlaysLeft;
            if (!player.Resources.CanAfford(card.Definition.cost)) return PlayResult.CannotAfford;
            if (!CanPlaceOn(player, card.Definition, square)) return PlayResult.InvalidSquare;
            return PlayResult.Ok;
        }

        /// <summary>Pays for and places a card from the current player's hand, then fires its OnPlay effects.</summary>
        public PlayResult PlayCard(int playerId, CardInstance card, Coord square)
        {
            var result = CheckPlay(playerId, card, square);
            if (result != PlayResult.Ok) return result;

            var player = CurrentPlayer;
            player.Resources.TrySpend(card.Definition.cost);
            if (!card.Definition.IsFree)
                Raise(new GameEvent { Type = GameEventType.ResourcesSpent, Player = player, Card = card,
                    Message = player + " paid " + string.Join(", ", card.Definition.cost) });

            player.Hand.Remove(card);
            PutOnBoard(card, square);
            player.CardsPlayedThisTurn++;
            Raise(new GameEvent { Type = GameEventType.CardPlayed, Player = player, Card = card,
                Message = player + " played " + card.Definition + " at " + square });

            FireEffects(card, EffectTrigger.OnPlay);
            CheckGoals();
            if (!IsOver && Board.IsFull) EndByMostSquares("The board is full");
            return PlayResult.Ok;
        }

        /// <summary>Convenience overload: play by hand index.</summary>
        public PlayResult PlayCard(int playerId, int handIndex, Coord square)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player == null || handIndex < 0 || handIndex >= player.Hand.Count) return PlayResult.CardNotInHand;
            return PlayCard(playerId, player.Hand[handIndex], square);
        }

        /// <summary>
        /// Puts a new card on the board for a player with no cost, turn or placement checks, and no OnPlay effects.
        /// For token-spawning effects, scenarios and tests. Returns null if the square is taken.
        /// </summary>
        public CardInstance Summon(CardDefinition def, PlayerState owner, Coord square)
        {
            if (!Board.IsEmpty(square)) return null;
            var card = CreateInstance(def, owner);
            PutOnBoard(card, square);
            return card;
        }

        private void PutOnBoard(CardInstance card, Coord square)
        {
            Board.Place(card, square);
            card.IsOnBoard = true;
            card.PlayedOnTurn = TurnNumber;
        }

        // ------------------------------------------------------------------ state changes used by effects

        public void DrawCards(PlayerState player, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (player.Deck.Count == 0) return;
                var card = player.Deck[player.Deck.Count - 1];
                player.Deck.RemoveAt(player.Deck.Count - 1);

                if (player.Hand.Count >= Config.maxHandSize)
                {
                    player.Discard.Add(card);
                    Raise(new GameEvent { Type = GameEventType.CardBurned, Player = player, Card = card,
                        Message = player + " had a full hand and discarded " + card.Definition });
                    continue;
                }

                player.Hand.Add(card);
                Raise(new GameEvent { Type = GameEventType.CardDrawn, Player = player, Card = card,
                    Message = player + " drew a card" });
            }
        }

        /// <summary>
        /// Creates a new copy of <paramref name="def"/> in the player's hand (it doesn't come from the deck).
        /// With a full hand the card is lost, the same as an over-draw.
        /// </summary>
        public CardInstance AddToHand(PlayerState player, CardDefinition def, CardInstance source = null)
        {
            if (player.Hand.Count >= Config.maxHandSize)
            {
                Raise(new GameEvent { Type = GameEventType.CardBurned, Player = player, Source = source,
                    Message = player + " had a full hand, so the " + def + (source != null ? " from " + source.Definition : "") +
                              " was lost" });
                return null;
            }
            var card = CreateInstance(def, player);
            player.Hand.Add(card);
            Raise(new GameEvent { Type = GameEventType.CardAddedToHand, Player = player, Card = card, Source = source,
                Message = player + " got a " + def + (source != null ? " from " + source.Definition : "") });
            return card;
        }

        public void GainResource(PlayerState player, ResourceType type, int amount, CardInstance source = null)
        {
            if (amount <= 0) return;
            player.Resources.Add(type, amount);
            player.GainedThisRound.Add(type, amount);
            Raise(new GameEvent { Type = GameEventType.ResourcesGained, Player = player, Source = source,
                Resource = type, Amount = amount,
                Message = player + " gained " + amount + " " + type + (source != null ? " from " + source.Definition : "") });
        }

        public void SpendResource(PlayerState player, ResourceType type, int amount, CardInstance source = null)
        {
            if (amount <= 0) return;
            player.Resources.Add(type, -amount);
            Raise(new GameEvent { Type = GameEventType.ResourcesSpent, Player = player, Source = source,
                Resource = type, Amount = amount,
                Message = player + " spent " + amount + " " + type + (source != null ? " on " + source.Definition : "") });
        }

        // ------------------------------------------------------------------ effects

        private void FirePhaseEffects(TimeOfDay time)
        {
            var trigger = time == TimeOfDay.Day ? EffectTrigger.Day : EffectTrigger.Night;
            // Stable order: oldest card first.
            var cards = Board.AllCards().OrderBy(c => c.InstanceId).ToList();
            foreach (var card in cards)
            {
                if (time == TimeOfDay.Day) card.DaysSeen++;
                else card.NightsSeen++;
            }
            foreach (var card in cards)
            {
                if (IsOver) return;
                FireEffects(card, trigger);
            }
        }

        private void FireEffects(CardInstance card, EffectTrigger trigger)
        {
            var effects = card.Definition.effects;
            if (effects == null) return;
            foreach (var def in effects)
            {
                if (def.trigger != trigger) continue;
                if (trigger == EffectTrigger.Day && !def.FiresOn(card.DaysSeen)) continue;
                if (trigger == EffectTrigger.Night && !def.FiresOn(card.NightsSeen)) continue;

                var ctx = new EffectContext { Match = this, Source = card, Effect = def };
                if (def.target == EffectTarget.OwnerAlliance)
                    ctx.TargetPlayers.AddRange(_players.Where(p => p.IsAlliedWith(card.Owner)));
                else
                    ctx.TargetPlayers.Add(card.Owner);
                _effects.Resolve(def).Apply(ctx);
            }
        }

        // ------------------------------------------------------------------ winning

        /// <summary>How many squares each alliance owns (a card on a square means you own it).</summary>
        public Dictionary<int, int> SquaresByAlliance()
        {
            var result = _players.Select(p => p.AllianceId).Distinct().ToDictionary(a => a, a => 0);
            foreach (var card in Board.AllCards()) result[card.Owner.AllianceId]++;
            return result;
        }

        public int SquaresOwnedBy(PlayerState player) => Board.AllCards().Count(c => c.Owner == player);

        /// <summary>Checks every player's goal, starting with whoever is playing, and ends the match if one is met.</summary>
        private void CheckGoals()
        {
            if (IsOver) return;
            int start = CurrentPlayer != null ? CurrentPlayer.Id : 0;
            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[(start + i) % _players.Count];
                var goal = GoalOf(p);
                if (goal == null || !goal.IsMet(this, p)) continue;
                Raise(new GameEvent { Type = GameEventType.GoalReached, Player = p, Message = p + " reached their goal!" });
                EndMatch(p.AllianceId, p.Team + " reached its goal");
                return;
            }
        }

        private void EndByMostSquares(string reason)
        {
            var squares = SquaresByAlliance();
            int best = squares.Values.Max();
            var leaders = squares.Where(kv => kv.Value == best).Select(kv => kv.Key).ToList();
            EndMatch(leaders.Count == 1 ? leaders[0] : (int?)null, reason + ", most squares wins");
        }

        private void EndMatch(int? winningAlliance, string reason)
        {
            if (IsOver) return;
            IsOver = true;
            CurrentPlayer = null;
            WinningAllianceId = winningAlliance;
            EndReason = reason;
            string who = winningAlliance.HasValue
                ? string.Join(" & ", _players.Where(p => p.AllianceId == winningAlliance.Value).Select(p => p.ToString()))
                : "nobody (draw)";
            Raise(new GameEvent { Type = GameEventType.MatchEnded, Message = reason + ". Winner: " + who });
        }

        private void Raise(GameEvent e)
        {
            _log.Add(e);
            EventRaised?.Invoke(e);
        }
    }
}
