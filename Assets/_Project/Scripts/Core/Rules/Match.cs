using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The rules engine for one match. Holds all game state and is the only thing that changes it.
    /// Has no Unity dependencies, so it can be unit tested, run on a server, or driven by an AI.
    ///
    /// Turn structure: start-of-turn effects fire, then the player draws, then they may play up to
    /// <see cref="MatchConfig.cardsPerTurn"/> cards, then end-of-turn effects fire and play passes on.
    /// </summary>
    public class Match
    {
        private readonly EffectRegistry _effects;
        private readonly IPlacementRule _placement;
        private readonly IWinCondition _winCondition;
        private readonly List<PlayerState> _players = new List<PlayerState>();
        private readonly List<GameEvent> _log = new List<GameEvent>();
        private int _nextInstanceId = 1;
        private int _currentIndex;

        public MatchConfig Config { get; }
        public Board Board { get; private set; }
        public GameRandom Random { get; }
        public IReadOnlyList<PlayerState> Players => _players;
        public IReadOnlyList<GameEvent> Log => _log;
        public PlayerState CurrentPlayer => _players.Count == 0 ? null : _players[_currentIndex];

        /// <summary>Counts every player-turn taken, starting at 1.</summary>
        public int TurnNumber { get; private set; }
        /// <summary>A round is complete when every remaining player has had a turn. Starts at 1.</summary>
        public int Round { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsOver { get; private set; }
        /// <summary>The winning alliance, or null for a draw / match not over.</summary>
        public int? WinningAllianceId { get; private set; }

        /// <summary>Raised for everything that happens. Subscribe from the presentation layer.</summary>
        public event Action<GameEvent> EventRaised;

        public Match(MatchConfig config, EffectRegistry effects = null, IPlacementRule placement = null,
            IWinCondition winCondition = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _effects = effects ?? EffectRegistry.CreateDefault();
            _placement = placement ?? new HomeZoneOrAdjacentPlacement();
            _winCondition = winCondition ?? new HomeCardWinCondition();
            Random = new GameRandom(config.seed);
        }

        // ------------------------------------------------------------------ setup

        public void Start()
        {
            if (IsStarted) throw new InvalidOperationException("Match already started.");
            int expected = GameModes.PlayerCount(Config.mode);
            if (Config.players == null || Config.players.Count != expected)
                throw new InvalidOperationException(Config.mode + " needs " + expected + " players, got " +
                                                    (Config.players?.Count ?? 0) + ".");

            Board = new Board(Config.boardWidth, Config.boardHeight, Config.adjacency);
            var seats = GameModes.Seats(Config.mode);

            for (int i = 0; i < expected; i++)
            {
                var setup = Config.players[i];
                var player = new PlayerState(i, string.IsNullOrEmpty(setup.name) ? "Player " + (i + 1) : setup.name,
                    setup.team, seats[i].Corner, seats[i].AllianceId);
                _players.Add(player);

                foreach (var def in setup.deck ?? new List<CardDefinition>())
                    player.Deck.Add(CreateInstance(def, player));
                Random.Shuffle(player.Deck);

                player.Resources.Add(Config.startingResources);

                if (Config.homeCard != null)
                {
                    var home = CreateInstance(Config.homeCard, player);
                    PutOnBoard(home, player.Corner.ToCoord(Board.Width, Board.Height));
                    player.HomeCard = home;
                }
            }

            IsStarted = true;
            Raise(new GameEvent { Type = GameEventType.MatchStarted, Message = "Match started: " + Config.mode });

            foreach (var p in _players) DrawCards(p, Config.startingHandSize);

            _currentIndex = 0;
            Round = 1;
            BeginTurn(skipDraw: Config.firstPlayerSkipsDraw);
        }

        public CardInstance CreateInstance(CardDefinition def, PlayerState owner) =>
            new CardInstance(_nextInstanceId++, def, owner);

        // ------------------------------------------------------------------ turns

        private void BeginTurn(bool skipDraw)
        {
            var player = CurrentPlayer;
            TurnNumber++;
            player.CardsPlayedThisTurn = 0;
            Raise(new GameEvent
            {
                Type = GameEventType.TurnStarted, Player = player,
                Message = "Turn " + TurnNumber + " (round " + Round + "): " + player
            });

            FireForPlayerCards(player, EffectTrigger.OnOwnerTurnStart);
            if (IsOver) return;

            if (!skipDraw) DrawCards(player, Config.drawsPerTurn);
        }

        /// <summary>Ends the current player's turn. Returns false if it isn't their turn or the match is over.</summary>
        public bool EndTurn(int playerId)
        {
            if (!IsStarted || IsOver || CurrentPlayer.Id != playerId) return false;

            var player = CurrentPlayer;
            FireForPlayerCards(player, EffectTrigger.OnOwnerTurnEnd);
            Raise(new GameEvent { Type = GameEventType.TurnEnded, Player = player, Message = player + " ended their turn" });
            if (IsOver) return true;

            int next = NextActiveIndex(_currentIndex);
            if (next <= _currentIndex) Round++;
            _currentIndex = next;

            if (Config.roundLimit > 0 && Round > Config.roundLimit)
            {
                EndMatch(null, "Round limit reached");
                return true;
            }

            BeginTurn(skipDraw: false);
            return true;
        }

        private int NextActiveIndex(int from)
        {
            for (int step = 1; step <= _players.Count; step++)
            {
                int i = (from + step) % _players.Count;
                if (!_players[i].IsEliminated) return i;
            }
            return from;
        }

        // ------------------------------------------------------------------ playing cards

        public bool IsInHomeZone(PlayerState player, Coord square) =>
            Board.InBounds(square) &&
            player.Corner.ToCoord(Board.Width, Board.Height).ChebyshevDistance(square) < Config.homeZoneSize;

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
            if (player.Id != playerId || player.IsEliminated) return PlayResult.NotYourTurn;
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

            FireTrigger(card, EffectTrigger.OnPlay);
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
        /// Puts a new card on the board for a player with no cost, turn or placement checks.
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

        public void GainResource(PlayerState player, ResourceType type, int amount, CardInstance source = null)
        {
            if (amount == 0) return;
            player.Resources.Add(type, amount);
            Raise(new GameEvent { Type = GameEventType.ResourcesGained, Player = player, Source = source,
                Resource = type, Amount = amount,
                Message = player + (amount > 0 ? " gained " : " lost ") + Math.Abs(amount) + " " + type +
                          (source != null ? " from " + source.Definition : "") });
        }

        public void DealDamage(CardInstance target, int amount, CardInstance source = null)
        {
            if (target == null || !target.IsOnBoard || target.IsIndestructible || amount <= 0) return;
            target.Health -= amount;
            Raise(new GameEvent { Type = GameEventType.CardDamaged, Card = target, Source = source, Amount = amount,
                Player = target.Owner,
                Message = target.Definition + " took " + amount + " damage" +
                          (source != null ? " from " + source.Definition : "") });
            if (target.Health <= 0) Destroy(target, source);
        }

        public void Heal(CardInstance target, int amount, CardInstance source = null)
        {
            if (target == null || !target.IsOnBoard || target.IsIndestructible || amount <= 0) return;
            int healed = Math.Min(amount, target.MaxHealth - target.Health);
            if (healed <= 0) return;
            target.Health += healed;
            Raise(new GameEvent { Type = GameEventType.CardHealed, Card = target, Source = source, Amount = healed,
                Player = target.Owner, Message = target.Definition + " healed " + healed });
        }

        public void Destroy(CardInstance card, CardInstance source = null)
        {
            if (card == null || !card.IsOnBoard) return;
            Board.Remove(card);
            card.IsOnBoard = false;
            card.Owner.Discard.Add(card);
            Raise(new GameEvent { Type = GameEventType.CardDestroyed, Card = card, Source = source, Player = card.Owner,
                Message = card.Definition + " was destroyed" });

            FireTrigger(card, EffectTrigger.OnDestroyed);
            CheckEliminations();
        }

        // ------------------------------------------------------------------ effects

        private void FireForPlayerCards(PlayerState player, EffectTrigger trigger)
        {
            // Snapshot in a stable order so effects that destroy cards don't break iteration.
            var cards = Board.AllCards().Where(c => c.Owner == player).OrderBy(c => c.InstanceId).ToList();
            foreach (var card in cards)
            {
                if (IsOver) return;
                if (card.IsOnBoard) FireTrigger(card, trigger);
            }
        }

        private void FireTrigger(CardInstance card, EffectTrigger trigger)
        {
            var effects = card.Definition.effects;
            if (effects == null) return;
            foreach (var def in effects)
            {
                if (IsOver) return;
                if (def.trigger != trigger) continue;
                // A card destroyed partway through its own effects stops, unless these are its death effects.
                if (trigger != EffectTrigger.OnDestroyed && !card.IsOnBoard) return;

                var ctx = new EffectContext { Match = this, Source = card, Effect = def };
                TargetResolver.Resolve(this, card, def, ctx.TargetCards, ctx.TargetPlayers);
                _effects.Resolve(def).Apply(ctx);
            }
        }

        // ------------------------------------------------------------------ winning

        private void CheckEliminations()
        {
            if (IsOver) return;
            foreach (var p in _players)
            {
                if (p.IsEliminated || !_winCondition.IsEliminated(this, p)) continue;
                p.IsEliminated = true;
                Raise(new GameEvent { Type = GameEventType.PlayerEliminated, Player = p, Message = p + " was eliminated" });
            }

            var alliances = WinConditionHelpers.RemainingAlliances(this);
            if (alliances.Length == 1) EndMatch(alliances[0], "Last side standing");
            else if (alliances.Length == 0) EndMatch(null, "Everyone was eliminated");
        }

        private void EndMatch(int? winningAlliance, string reason)
        {
            if (IsOver) return;
            IsOver = true;
            WinningAllianceId = winningAlliance;
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
