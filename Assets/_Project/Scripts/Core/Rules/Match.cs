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
            foreach (var glade in Config.GladesFor(expected, size)) Board.AddGlade(glade);
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
                if (Config.openingHandHasFreeCard) PutFreeCardInOpeningHand(player);

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

        /// <summary>The top of the deck is the end of the list; make sure a free card is among the cards drawn first.</summary>
        private void PutFreeCardInOpeningHand(PlayerState player)
        {
            var deck = player.Deck;
            int handSize = Math.Min(Config.startingHandSize, deck.Count);
            if (handSize == 0) return;
            for (int i = deck.Count - handSize; i < deck.Count; i++)
                if (deck[i].Definition.IsFree) return;
            int free = deck.FindIndex(c => c.Definition.IsFree);
            if (free < 0) return;
            int top = deck.Count - 1;
            (deck[free], deck[top]) = (deck[top], deck[free]);
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

        private readonly HashSet<PlayerState> _hadTurn = new HashSet<PlayerState>();

        private void BeginTurn(PlayerState player)
        {
            CurrentPlayer = player;
            TurnNumber++;
            player.CardsPlayedThisTurn = 0;
            player.CompostedThisTurn = false;
            player.GhoulsSummonedThisTurn = 0;
            Raise(new GameEvent
            {
                Type = GameEventType.TurnStarted, Player = player,
                Message = "Turn " + TurnNumber + " (" + TimeOfDay + " " + Round + "): " + player
            });

            bool skipDraw = Config.everyoneSkipsFirstDraw ? _hadTurn.Add(player) : TurnNumber == 1 && Config.firstPlayerSkipsDraw;
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
            var cost = TotalCost(player, card.Definition);
            if (cost == null) return PlayResult.NoPlaysLeft;
            if (!player.Resources.CanAfford(cost)) return PlayResult.CannotAfford;
            if (!CanPlaceOn(player, card.Definition, square)) return PlayResult.InvalidSquare;
            return PlayResult.Ok;
        }

        /// <summary>Pays for and places a card from the current player's hand, then fires its OnPlay effects.</summary>
        public PlayResult PlayCard(int playerId, CardInstance card, Coord square)
        {
            var result = CheckPlay(playerId, card, square);
            if (result != PlayResult.Ok) return result;

            var player = CurrentPlayer;
            var cost = TotalCost(player, card.Definition);
            player.Resources.TrySpend(cost);
            if (cost.Exists(c => c.amount > 0))
                Raise(new GameEvent { Type = GameEventType.ResourcesSpent, Player = player, Card = card,
                    Message = player + " paid " + string.Join(", ", cost) });

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

        /// <summary>
        /// What playing <paramref name="def"/> would cost the player right now: the card's cost, plus any extra-play
        /// cost if they've used their normal plays this turn (Forest can pay Acorns to play extra trees).
        /// Null means they have no plays left for this card.
        /// </summary>
        public List<ResourceAmount> TotalCost(PlayerState player, CardDefinition def)
        {
            var cost = new List<ResourceAmount>(def.cost ?? new List<ResourceAmount>());
            if (player.CardsPlayedThisTurn < Config.cardsPerTurn) return cost;
            var extra = TeamRules.ExtraPlayCost(player.Team, def, Config, player.CardsPlayedThisTurn - Config.cardsPerTurn);
            if (extra == null) return null;
            cost.AddRange(extra);
            return cost;
        }

        /// <summary>
        /// Turns a card on the board into another card in place (a Sapling growing into a Young Tree). It keeps its
        /// square and owner; its timers start again.
        /// </summary>
        public void Transform(CardInstance card, CardDefinition into)
        {
            if (card == null || into == null || !card.IsOnBoard) return;
            var from = card.Definition;
            card.Definition = into;
            card.DaysSeen = 0;
            card.NightsSeen = 0;
            card.GrowthProgress = 0;
            Raise(new GameEvent { Type = GameEventType.CardGrew, Player = card.Owner, Card = card,
                Message = from + " grew into " + into + " at " + card.Position });
            CheckGoals();
        }

        /// <summary>Whether the current player can pollinate this card now (Forest: one of their growing trees, for Honey).</summary>
        public bool CanPollinate(int playerId, CardInstance card)
        {
            if (!IsStarted || IsOver || CurrentPlayer == null || CurrentPlayer.Id != playerId) return false;
            var player = CurrentPlayer;
            int cost = Config.forestPollinateHoney;
            return player.Team == Team.Forest && cost > 0 && card != null && card.IsOnBoard && card.Owner == player &&
                   ForestCards.GrowthOf(card.Definition) != null && player.Resources.Get(ResourceType.Honey) >= cost;
        }

        /// <summary>
        /// Forest's Pollinate: spend Honey so one of your growing trees grows one step now, sun or no sun.
        /// Doesn't use up a play. Returns false if not allowed.
        /// </summary>
        public bool Pollinate(int playerId, CardInstance card)
        {
            if (!CanPollinate(playerId, card)) return false;
            var player = CurrentPlayer;
            SpendResource(player, ResourceType.Honey, Config.forestPollinateHoney, card);
            Raise(new GameEvent { Type = GameEventType.CardPollinated, Player = player, Card = card,
                Message = player + " pollinated " + card.Definition + " at " + card.Position });
            ForestCards.GrowOneStep(this, card);
            CheckGoals();
            return true;
        }

        /// <summary>
        /// What the next Ghoul costs the player now (Dark Forest's Summon rule), or null if their team can't summon.
        /// The base recipe, plus <see cref="MatchConfig.darkForestGhoulStep"/> per Ghoul already summoned this turn,
        /// minus 1 Bat and 1 Spider for each of their Haunts that is switched on (see <see cref="DarkForestCards.GhoulDiscount"/>).
        /// </summary>
        public List<ResourceAmount> GhoulCost(PlayerState player)
        {
            if (player == null || player.Team != Team.DarkForest) return null;
            int bats = Config.darkForestGhoulBats, spiders = Config.darkForestGhoulSpiders;
            if (bats <= 0 && spiders <= 0) return null;
            int step = Config.darkForestGhoulStep * player.GhoulsSummonedThisTurn;
            if (Config.darkForestGhoulsPerPriceStep > 0)
                step += player.Resources.Get(ResourceType.Ghouls) / Config.darkForestGhoulsPerPriceStep;
            bats += step;
            spiders += step;
            // Each switched-on Haunt takes 1 Bat and 1 Spider off the price, down to the minimum.
            int min = Math.Max(0, Config.darkForestGhoulMinEach);
            int off = DarkForestCards.GhoulDiscount(this, player);
            bats = Math.Max(Math.Min(bats, min), bats - off);
            spiders = Math.Max(Math.Min(spiders, min), spiders - off);
            return new List<ResourceAmount> { new ResourceAmount(ResourceType.Bats, bats), new ResourceAmount(ResourceType.Spiders, spiders) };
        }

        /// <summary>Whether the current player can summon a Ghoul now (Dark Forest, on their turn, if they can pay).</summary>
        public bool CanSummonGhoul(int playerId)
        {
            if (!IsStarted || IsOver || CurrentPlayer == null || CurrentPlayer.Id != playerId) return false;
            if (Config.darkForestGhoulsPerTurn > 0 && CurrentPlayer.GhoulsSummonedThisTurn >= Config.darkForestGhoulsPerTurn) return false;
            var cost = GhoulCost(CurrentPlayer);
            return cost != null && CurrentPlayer.Resources.CanAfford(cost);
        }

        /// <summary>
        /// Dark Forest's Summon: combine Bats and Spiders into 1 Ghoul. Doesn't use up a play. Returns false if not allowed.
        /// </summary>
        public bool SummonGhoul(int playerId)
        {
            if (!CanSummonGhoul(playerId)) return false;
            var player = CurrentPlayer;
            var cost = GhoulCost(player);
            foreach (var c in cost) SpendResource(player, c.type, c.amount);
            player.GhoulsSummonedThisTurn++;
            Raise(new GameEvent { Type = GameEventType.GhoulSummoned, Player = player, Resource = ResourceType.Ghouls, Amount = 1,
                Message = player + " combined " + string.Join(" and ", cost) + " into a Ghoul" });
            GainResource(player, ResourceType.Ghouls, 1);
            CheckGoals();
            return true;
        }

        /// <summary>Whether the current player can compost a card now (Forest: once a turn, any card in hand).</summary>
        public bool CanCompost(int playerId, CardInstance card)
        {
            if (!IsStarted || IsOver || CurrentPlayer == null || CurrentPlayer.Id != playerId) return false;
            var player = CurrentPlayer;
            return card != null && player.Hand.Contains(card) && !player.CompostedThisTurn &&
                   TeamRules.CompostGain(player.Team, Config) != null;
        }

        /// <summary>
        /// Discards a card from the current player's hand for its team's compost gain (Forest: 1 Acorn).
        /// Doesn't use up a play. Returns false if not allowed.
        /// </summary>
        public bool Compost(int playerId, CardInstance card)
        {
            if (!CanCompost(playerId, card)) return false;
            var player = CurrentPlayer;
            player.Hand.Remove(card);
            player.CompostedThisTurn = true;
            Raise(new GameEvent { Type = GameEventType.CardComposted, Player = player, Card = card,
                Message = player + " composted " + card.Definition });
            foreach (var gain in TeamRules.CompostGain(player.Team, Config))
                GainResource(player, gain.type, gain.amount, null);
            CheckGoals();
            return true;
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
            GladeBonus(player, type, source);
        }

        /// <summary>
        /// Sunny Glades: the first time each round that a card touching a glade makes something for its owner, it makes
        /// <see cref="MatchConfig.sunnyGladeBonus"/> more of it.
        /// </summary>
        private void GladeBonus(PlayerState player, ResourceType type, CardInstance source)
        {
            int bonus = Config.sunnyGladeBonus;
            if (bonus <= 0 || source == null || !source.IsOnBoard || source.Owner != player) return;
            if (source.GladeBonusRound == Round || !Board.TouchesGlade(source.Position)) return;
            source.GladeBonusRound = Round;
            player.Resources.Add(type, bonus);
            player.GainedThisRound.Add(type, bonus);
            Raise(new GameEvent { Type = GameEventType.ResourcesGained, Player = player, Source = source,
                Resource = type, Amount = bonus,
                Message = player + " gained " + bonus + " more " + type + " from the Sunny Glade by " + source.Definition });
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

        /// <summary>
        /// Runs a card's day and night effects once, right now, whatever its timers say (e.g. Big Whistle:
        /// "every Assembler you own runs once right now"). Doesn't change its day or night count.
        /// </summary>
        public void RunCardNow(CardInstance card)
        {
            if (card == null || IsOver) return;
            FireEffects(card, EffectTrigger.Day, ignoreTimers: true);
            if (!IsOver) FireEffects(card, EffectTrigger.Night, ignoreTimers: true);
        }

        private void FireEffects(CardInstance card, EffectTrigger trigger, bool ignoreTimers = false)
        {
            var effects = card.Definition.effects;
            if (effects == null) return;
            foreach (var def in effects)
            {
                if (def.trigger != trigger) continue;
                if (!ignoreTimers && trigger == EffectTrigger.Day && !def.FiresOn(card.DaysSeen)) continue;
                if (!ignoreTimers && trigger == EffectTrigger.Night && !def.FiresOn(card.NightsSeen)) continue;

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

        /// <summary>For team rules outside this class (Ghoul feeding) to report what happened.</summary>
        internal void RaiseEvent(GameEvent e) => Raise(e);

        private void Raise(GameEvent e)
        {
            _log.Add(e);
            EventRaised?.Invoke(e);
        }
    }
}
