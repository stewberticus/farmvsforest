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

    /// <summary>Plays a random affordable card on a random legal square. A floor: any real strategy should beat it.</summary>
    public class RandomBot : IBot
    {
        private readonly GameRandom _rng;
        public RandomBot(int seed) => _rng = new GameRandom(seed);
        public string Name => "random";

        public void PlayTurn(Match match)
        {
            var p = match.CurrentPlayer;
            var options = Bots.Options(match, p).ToList();
            if (options.Count == 0) return;
            var (card, square) = options[_rng.Range(0, options.Count)];
            match.PlayCard(p.Id, card, square);
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

        public GreedyBot(int seed, BalanceSettings settings = null)
        {
            _rng = new GameRandom(seed);
            _settings = settings ?? new BalanceSettings();
        }

        public string Name => "greedy";

        /// <summary>Plays the best card, then keeps going while extra plays are possible (Forest's Acorn trees).</summary>
        public void PlayTurn(Match match)
        {
            for (int i = 0; i < 10 && !match.IsOver; i++)
                if (!PlayBest(match)) break;
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
                    double total = cardScore + ScoreSquare(match, p, card.Definition, square) + _rng.Range(0, 1000) / 1000.0 * Noise;
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

        private static double ScoreSquare(Match match, PlayerState p, CardDefinition def, Coord square)
        {
            var board = match.Board;
            double score = 0;
            foreach (var n in board.Neighbours(square))
            {
                var other = board.Get(n);
                if (other == null) score += 0.3; // room to grow
                else if (other.Owner == p && TouchCares(def, other.Definition)) score += 1.5;
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
                default: return new GreedyBot(seed, settings);
            }
        }
    }
}
