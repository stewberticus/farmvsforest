using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>Plays one turn for whoever is the current player. Used by the balance simulator and, later, AI opponents.</summary>
    public interface IBot
    {
        string Name { get; }
        /// <summary>Plays cards for <see cref="Match.CurrentPlayer"/>. Does not end the turn.</summary>
        void PlayTurn(Match match);
    }

    /// <summary>Never plays a card. Use as the opponent to "goldfish" a team: how fast does it win unbothered?</summary>
    public class PassBot : IBot
    {
        public string Name => "pass";
        public void PlayTurn(Match match) { }
    }

    /// <summary>
    /// Plays a random affordable card on a random legal square, and keeps going while extra plays are possible
    /// (Forest's Acorn trees), like <see cref="GreedyBot"/>. A floor: any real strategy should beat it.
    /// </summary>
    public class RandomBot : IBot
    {
        private readonly GameRandom _rng;
        public RandomBot(int seed) => _rng = new GameRandom(seed);
        public string Name => "random";

        public void PlayTurn(Match match)
        {
            for (int i = 0; i < 10 && !match.IsOver; i++)
            {
                var p = match.CurrentPlayer;
                var options = Bots.Options(match, p).ToList();
                if (options.Count == 0) return;
                var (card, square) = options[_rng.Range(0, options.Count)];
                if (match.PlayCard(p.Id, card, square) != PlayResult.Ok) return;
            }
            // Compost a random card now and then (Forest only; does nothing for other teams).
            var me = match.CurrentPlayer;
            if (!match.IsOver && me.Hand.Count > 0 && _rng.Range(0, 3) == 0)
                match.Compost(me.Id, me.Hand[_rng.Range(0, me.Hand.Count)]);
            // Pollinate random growing trees with some of its Honey.
            for (int i = 0; i < 10 && !match.IsOver && _rng.Range(0, 2) == 0; i++)
            {
                var trees = match.Board.AllCards().Where(c => match.CanPollinate(me.Id, c)).ToList();
                if (trees.Count == 0) break;
                match.Pollinate(me.Id, trees[_rng.Range(0, trees.Count)]);
            }
        }
    }

    /// <summary>
    /// A simple, decent player. Scores each affordable card with <see cref="CardValuation"/>, weighted towards what its
    /// team's goal needs, and each square by how much room it opens up and how many matching neighbours it has.
    /// Good enough to compare cards and teams against each other; not meant to be clever.
    /// </summary>
    public class GreedyBot : IBot
    {
        private readonly GameRandom _rng;
        private readonly BalanceSettings _settings;
        private readonly Dictionary<string, CardScore> _scores = new Dictionary<string, CardScore>();

        /// <summary>How much spending resources hurts, as a fraction of their value. Higher for teams that win by holding resources.</summary>
        public double SpendWeight = 0.5;
        /// <summary>Random noise added to each option, so games vary.</summary>
        public double Noise = 1.0;
        /// <summary>
        /// Plays the worst option it can find instead of the best (it still plays a card every turn if it can).
        /// Stands in for the weakest sensible player, to measure how much decisions matter.
        /// </summary>
        public bool Worst;
        /// <summary>
        /// Plays as if Sunny Glades weren't there (it still gets their bonus by luck). Mirror it against the normal bot to
        /// measure whether going for the glades matters.
        /// </summary>
        public bool IgnoreGlades;

        public GreedyBot(int seed, BalanceSettings settings = null, bool worst = false)
        {
            _rng = new GameRandom(seed);
            _settings = settings ?? new BalanceSettings();
            Worst = worst;
        }

        /// <summary>Name shown in reports. Defaults to "greedy" or "worst".</summary>
        public string Label;

        public string Name => Label ?? (Worst ? "worst" : "greedy");

        /// <summary>Plays the best card, then keeps going while extra plays are possible (Forest's Acorn trees).</summary>
        public void PlayTurn(Match match)
        {
            for (int i = 0; i < 10 && !match.IsOver; i++)
                if (!PlayBest(match)) break;
            if (!match.IsOver) MaybeCompost(match);
            if (!match.IsOver) Pollinate(match);
        }

        /// <summary>
        /// Forest's Pollinate: spend Honey on the growing tree closest to becoming an Old Tree, keeping enough Honey
        /// for a Honey card in hand. The worst bot picks the tree furthest from it.
        /// </summary>
        private void Pollinate(Match match)
        {
            var p = match.CurrentPlayer;
            int reserve = p.Hand.Max(c => (int?)(c.Definition.cost ?? new List<ResourceAmount>())
                .Where(r => r.type == ResourceType.Honey).Sum(r => r.amount)) ?? 0;
            for (int i = 0; i < 20 && !match.IsOver; i++)
            {
                if (p.Resources.Get(ResourceType.Honey) - match.Config.forestPollinateHoney < (Worst ? 0 : reserve)) break;
                var trees = match.Board.AllCards().Where(c => match.CanPollinate(p.Id, c))
                    .OrderBy(c => ForestCards.StepsToOld(c)).ToList();
                if (trees.Count == 0) return;
                if (!match.Pollinate(p.Id, Worst ? trees.Last() : trees.First())) return;
            }
        }

        /// <summary>
        /// Forest's Compost: discard the least useful card for Acorns when the hand is getting full, or when it's
        /// stuck with nothing affordable. The worst bot composts its best card instead.
        /// </summary>
        private void MaybeCompost(Match match)
        {
            var p = match.CurrentPlayer;
            if (p.Hand.Count == 0 || !match.CanCompost(p.Id, p.Hand[0])) return;
            bool stuck = Bots.Playable(match, p).Count == 0;
            if (!stuck && p.Hand.Count < match.Config.maxHandSize - 1) return;
            // Keep goal pieces (trees): they're worth more than the Acorn. Compost the weakest of the rest.
            var goal = match.GoalOf(p) as ConnectedTaggedCardsGoal;
            var ranked = p.Hand.Where(c => goal == null || !c.Definition.HasTag(goal.Tag))
                .OrderBy(c => ScoreCard(match, p, c.Definition)).ToList();
            if (ranked.Count == 0) return;
            match.Compost(p.Id, Worst ? ranked.Last() : ranked.First());
        }

        private bool PlayBest(Match match)
        {
            var p = match.CurrentPlayer;
            double bestScore = double.NegativeInfinity;
            CardInstance bestCard = null;
            Coord bestSquare = default;

            foreach (var card in Bots.Playable(match, p))
            {
                double cardScore = ScoreCard(match, p, card.Definition);
                foreach (var square in match.LegalSquares(p, card.Definition))
                {
                    double total = cardScore + ScoreSquare(match, p, card.Definition, square) +
                                   TouchingValue(match, p, card.Definition, square);
                    if (Worst) total = -total;
                    total += _rng.Range(0, 1000) / 1000.0 * Noise;
                    if (total > bestScore)
                    {
                        bestScore = total;
                        bestCard = card;
                        bestSquare = square;
                    }
                }
            }

            return bestCard != null && match.PlayCard(p.Id, bestCard, bestSquare) == PlayResult.Ok;
        }

        private CardScore Score(CardDefinition def)
        {
            if (!_scores.TryGetValue(def.id ?? def.displayName, out var s))
                _scores[def.id ?? def.displayName] = s = CardValuation.Evaluate(def, _settings);
            return s;
        }

        private double ScoreCard(Match match, PlayerState p, CardDefinition def)
        {
            var s = Score(def);
            double value = s.Value;

            // Lean towards what the goal still needs: resources we're short of are worth more.
            var goal = match.GoalOf(p);
            if (goal is HaveResourcesGoal have)
            {
                foreach (var e in def.effects ?? new List<EffectDefinition>())
                {
                    if (!have.Types.Contains(e.resource)) continue;
                    double shortfall = 1 - Math.Min(1, p.Resources.Get(e.resource) / (double)have.Amount);
                    value += 3 * shortfall;
                }
            }
            else if (goal is GainInOneRoundGoal burst &&
                     (def.effects ?? new List<EffectDefinition>()).Any(e => e.resource == burst.Type))
            {
                value += 6;
            }
            else if (goal is ConnectedTaggedCardsGoal group && def.HasTag(group.Tag))
            {
                value += 4;
            }

            double spend = s.Cost * SpendWeight;
            if (goal is HaveResourcesGoal held)
                spend += (def.cost ?? new List<ResourceAmount>()).Where(c => held.Types.Contains(c.type))
                    .Sum(c => c.amount * _settings.ValueOf(c.type));
            return value - spend;
        }

        /// <summary>
        /// What touching a Sunny Glade is worth to this card: its bonus every round for the rest of the game, valued at
        /// the card's main resource. Trees make nothing, but a glade keeps them in the sun for good.
        /// </summary>
        private double GladeValue(Match match, PlayerState p, CardDefinition def, Coord square)
        {
            if (IgnoreGlades || match.Config.sunnyGladeBonus <= 0 || !match.Board.TouchesGlade(square)) return 0;
            if (ForestCards.Grows(def)) return 2;
            var makes = (def.effects ?? new List<EffectDefinition>()).Where(e => e.trigger != EffectTrigger.OnPlay &&
                (e.kind == EffectKind.GainResource || e.kind == EffectKind.GainPerTouching ||
                 e.kind == EffectKind.ConvertResource || e.kind == EffectKind.GainPerEmptyTouching ||
                 e.customEffectId == StartingIncomeEffect.Id)).ToList();
            if (makes.Count == 0) return 0;
            double perRound = makes.Max(e => _settings.ValueOf(e.resource)) * match.Config.sunnyGladeBonus;
            double roundsLeft = Math.Max(2, _settings.targetRounds - match.Round);
            // Discounted like any future income: worth about half its face value over the rest of the game.
            return 0.5 * perRound * roundsLeft;
        }

        private double ScoreSquare(Match match, PlayerState p, CardDefinition def, Coord square)
        {
            var board = match.Board;
            double score = GladeValue(match, p, def, square);
            foreach (var n in board.Neighbours(square))
            {
                var other = board.Get(n);
                if (other == null) score += 0.3; // room to grow
                else if (other.Owner == p && TouchCares(def, other.Definition)) score += 1.5;
            }

            // Forest growth: trees grow only while they touch an empty square (sunlight). Give growing cards room,
            // and don't take the last open square from one of our growing trees.
            if (ForestCards.Grows(def))
                score += 1.0 * Math.Min(2, board.Neighbours(square).Count(board.IsOpen));
            foreach (var n in board.Neighbours(square))
            {
                var other = board.Get(n);
                if (other == null || other.Owner != p || !ForestCards.Grows(other.Definition)) continue;
                int open = board.Neighbours(n).Count(board.IsOpen);
                score -= open <= 1 ? 6 : 0.5;
            }

            // Push towards the middle of the board to claim contested space first.
            double cx = (board.Width - 1) / 2.0, cy = (board.Height - 1) / 2.0;
            double dist = Math.Abs(square.x - cx) + Math.Abs(square.y - cy);
            score += 0.1 * (board.Width - dist);

            // Forest-style goals want one connected group: stay next to cards with the same tag.
            if (match.GoalOf(p) is ConnectedTaggedCardsGoal group && def.HasTag(group.Tag) &&
                board.Neighbours(square).Select(board.Get).Any(c => c != null && c.Owner == p && c.Definition.HasTag(group.Tag)))
                score += 2;
            return score;
        }

        /// <summary>
        /// The card's price assumes an average number of touching cards for "per touching" effects. Correct that for
        /// this square: a Chicken Coop touching no crops is worth nothing, one touching three is worth a lot. Also counts
        /// what this card adds to touching cards whose own effects care about it.
        /// </summary>
        private double TouchingValue(Match match, PlayerState p, CardDefinition def, Coord square)
        {
            var board = match.Board;
            var mine = board.Neighbours(square).Select(board.Get).Where(c => c != null && c.Owner == p).ToList();
            double v = 0;
            foreach (var e in def.effects ?? new List<EffectDefinition>())
            {
                if (e.kind != EffectKind.GainPerTouching) continue;
                double expected = string.IsNullOrEmpty(e.tag) ? _settings.expectedTouchingAny : _settings.expectedTouchingTagged;
                int actual = mine.Count(c => string.IsNullOrEmpty(e.tag) || c.Definition.HasTag(e.tag));
                v += (actual - expected) * e.amount * _settings.ValueOf(e.resource) * CardValuation.Fires(e, _settings);
            }
            foreach (var e in def.effects ?? new List<EffectDefinition>())
            {
                if (e.kind != EffectKind.GainPerEmptyTouching) continue;
                int empty = board.Neighbours(square).Count(board.IsEmpty);
                v += (empty - _settings.expectedTouchingEmpty) * e.amount * _settings.ValueOf(e.resource) * CardValuation.Fires(e, _settings);
            }
            foreach (var n in mine)
            foreach (var e in n.Definition.effects ?? new List<EffectDefinition>())
            {
                double each = e.amount * _settings.ValueOf(e.resource) * CardValuation.Fires(e, _settings);
                if (e.kind == EffectKind.GainPerTouching && (string.IsNullOrEmpty(e.tag) || def.HasTag(e.tag))) v += each;
                // Filling a square next to a card that wants empty space costs it that square.
                else if (e.kind == EffectKind.GainPerEmptyTouching) v -= each;
            }
            return v;
        }

        /// <summary>Does placing <paramref name="def"/> next to <paramref name="neighbour"/> help either card's per-touching effects?</summary>
        private static bool TouchCares(CardDefinition def, CardDefinition neighbour)
        {
            bool Cares(CardDefinition a, CardDefinition b) => (a.effects ?? new List<EffectDefinition>()).Any(e =>
                e.kind == EffectKind.GainPerTouching && (string.IsNullOrEmpty(e.tag) || b.HasTag(e.tag)));
            return Cares(def, neighbour) || Cares(neighbour, def);
        }
    }

    public static class Bots
    {
        /// <summary>Noise of the "sloppy" bot (greedy uses 1). At 6 it often picks the second or third best play.</summary>
        public static double SloppyNoise = 6;

        /// <summary>Every (card, square) the player could play right now.</summary>
        public static IEnumerable<(CardInstance card, Coord square)> Options(Match match, PlayerState p) =>
            Playable(match, p)
                .SelectMany(c => match.LegalSquares(p, c.Definition).Select(sq => (c, sq)));

        /// <summary>Cards in hand the player can pay for right now, counting any extra-play cost.</summary>
        public static List<CardInstance> Playable(Match match, PlayerState p) =>
            p.Hand.Where(c =>
            {
                var cost = match.TotalCost(p, c.Definition);
                return cost != null && p.Resources.CanAfford(cost);
            }).ToList();

        public static IBot Create(string name, int seed, BalanceSettings settings = null)
        {
            switch ((name ?? "greedy").ToLowerInvariant())
            {
                case "random": return new RandomBot(seed);
                case "pass": return new PassBot();
                case "worst": return new GreedyBot(seed, settings, worst: true);
                // A careless player: same judgement as greedy, but noise swamps the smaller differences between options.
                case "sloppy": return new GreedyBot(seed, settings) { Noise = SloppyNoise, Label = "sloppy" };
                case "noglades": return new GreedyBot(seed, settings) { IgnoreGlades = true, Label = "noglades" };
                default: return new GreedyBot(seed, settings);
            }
        }
    }
}
