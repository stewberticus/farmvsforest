using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The numbers behind the card value model in docs/BALANCE.md. Every card is scored in "value points" (V):
    /// 1 V is one tier-1 resource. These are starting numbers; the balance simulator is how we tune them.
    /// </summary>
    [Serializable]
    public class BalanceSettings
    {
        /// <summary>Target length of a 1v1 game in rounds. Everything else is derived from this.</summary>
        public int targetRounds = 20;

        /// <summary>
        /// How many rounds a typical card is on the board before the game ends. A card played in the middle of a
        /// <see cref="targetRounds"/> game sees about half of it, so the default is targetRounds / 2.
        /// </summary>
        public double horizon = 10;

        /// <summary>A free card is expected to be worth this much (the vanilla "1 tier-1 resource every day" card).</summary>
        public double parBase = 10;
        /// <summary>Each 1 V of cost should buy this much extra value. Above 1 because resources are easier to get than plays.</summary>
        public double parPerCost = 1.5;
        /// <summary>A card within this fraction of par is "on curve".</summary>
        public double tolerance = 0.15;

        /// <summary>One extra random card in hand. Low, because you can only play one card a turn anyway.</summary>
        public double drawValue = 2;
        /// <summary>
        /// A specific card made straight into your hand ("every 2 days, add a Cow card") is worth this, plus however
        /// far that card is above its own par. More than a random draw because you know what you're getting.
        /// </summary>
        public double madeCardValue = 3;
        /// <summary>Finds a card by id, for pricing AddCardToHand. Set by EvaluateAll from the cards it's given.</summary>
        [NonSerialized] public Func<string, CardDefinition> cardLookup;
        /// <summary>A once-only delayed effect loses this fraction of its value for every round you wait.</summary>
        public double delayDiscountPerRound = 0.05;
        /// <summary>Delayed effects never drop below this fraction of their face value.</summary>
        public double delayFloor = 0.5;
        /// <summary>Converters only fire when you can pay, so they're worth this fraction of their full rate.</summary>
        public double converterEfficiency = 0.8;
        /// <summary>Expected number of your own cards touching a "per touching" card over its life (any card).</summary>
        public double expectedTouchingAny = 2.0;
        /// <summary>Expected number of touching cards that have a specific tag.</summary>
        public double expectedTouchingTagged = 1.2;

        /// <summary>V per resource. Tier 1 = 1, tier 2 = 2, the goal resource is tier 3. Teams retune these.</summary>
        public Dictionary<ResourceType, double> resourceValue = new Dictionary<ResourceType, double>
        {
            { ResourceType.Wheat, 1 }, { ResourceType.Milk, 2 }, { ResourceType.Eggs, 2 },
            { ResourceType.Acorns, 1 }, { ResourceType.Honey, 2 },
            { ResourceType.Parts, 1 }, { ResourceType.Toys, 3 },
            { ResourceType.Bats, 1 }, { ResourceType.Spiders, 1 }, { ResourceType.Ghouls, 3 },
        };

        /// <summary>
        /// Extra value for tags that count towards that team's goal. A Forest "tree" card is a step towards 30 connected
        /// trees; a Dark Forest card tagged "tree" is not.
        /// </summary>
        public Dictionary<Team, Dictionary<string, double>> goalTagValue = new Dictionary<Team, Dictionary<string, double>>
        {
            { Team.Forest, new Dictionary<string, double> { { Tags.Tree, 3 } } },
        };

        /// <summary>Hand-set values for Custom effects, by customEffectId. Unknown ones are flagged.</summary>
        public Dictionary<string, double> customEffectValue = new Dictionary<string, double>
        {
            // Factory's Big Whistle: about 4 Assemblers each making 1 Toy (3 V) when it's played.
            { RunTaggedCardsEffect.Id, 12 },
        };

        public double ValueOf(ResourceType type) => resourceValue.TryGetValue(type, out var v) ? v : 1;

        public double Par(double costValue) => parBase + parPerCost * costValue;
    }

    /// <summary>How a card scores against the curve.</summary>
    public class CardScore
    {
        public CardDefinition Card;
        /// <summary>What the card gives you over its life, in V.</summary>
        public double Value;
        /// <summary>What the card costs, in V.</summary>
        public double Cost;
        /// <summary>What a card with this cost should give: parBase + parPerCost * Cost.</summary>
        public double Par;
        /// <summary>(Value - Par) / Par. 0 is exactly on curve, +0.3 is 30% too strong.</summary>
        public double Delta;
        public string Verdict;
        /// <summary>One line per effect or tag explaining the number.</summary>
        public List<string> Breakdown = new List<string>();
        /// <summary>Effects the model can't price (Custom with no hand-set value).</summary>
        public List<string> Warnings = new List<string>();
    }

    /// <summary>
    /// Prices a card with the value model in docs/BALANCE.md. It's a first guess to spot outliers before
    /// simulating, not the final word: the simulator and playtests decide.
    /// </summary>
    public static class CardValuation
    {
        public static CardScore Evaluate(CardDefinition card, BalanceSettings s = null) => Evaluate(card, s, 0);

        private static CardScore Evaluate(CardDefinition card, BalanceSettings s, int depth)
        {
            s = s ?? new BalanceSettings();
            var score = new CardScore { Card = card };

            foreach (var cost in card.cost ?? new List<ResourceAmount>())
                score.Cost += cost.amount * s.ValueOf(cost.type);

            foreach (var e in card.effects ?? new List<EffectDefinition>())
            {
                double v = EffectValue(e, s, depth, out string why, out string warning);
                score.Value += v;
                score.Breakdown.Add(why + " = " + v.ToString("0.#"));
                if (warning != null) score.Warnings.Add(warning);
            }

            foreach (var tag in card.tags ?? new List<string>())
            {
                if (!s.goalTagValue.TryGetValue(card.team, out var tags) || !tags.TryGetValue(tag, out var tv) || tv == 0) continue;
                score.Value += tv;
                score.Breakdown.Add("tag " + tag + " = " + tv.ToString("0.#"));
            }

            score.Par = s.Par(score.Cost);
            score.Delta = score.Par > 0 ? (score.Value - score.Par) / score.Par : 0;
            score.Verdict = card.HasTag(Tags.Start) ? "starting card"
                : score.Warnings.Count > 0 ? "price by hand"
                : score.Delta > s.tolerance ? "strong"
                : score.Delta < -s.tolerance ? "weak"
                : "on curve";
            return score;
        }

        /// <summary>How many times a Day/Night effect fires within the horizon.</summary>
        public static double Fires(EffectDefinition e, BalanceSettings s)
        {
            if (e.trigger == EffectTrigger.OnPlay) return 1;
            int every = Math.Max(1, e.interval);
            if (e.repeat) return s.horizon / every;
            return every <= s.horizon ? Math.Max(s.delayFloor, 1 - s.delayDiscountPerRound * every) : 0;
        }

        private static double EffectValue(EffectDefinition e, BalanceSettings s, int depth, out string why, out string warning)
        {
            warning = null;
            double fires = Fires(e, s);
            string when = When(e);
            double each;
            switch (e.kind)
            {
                case EffectKind.GainResource:
                    each = e.amount * s.ValueOf(e.resource);
                    why = when + ": " + e.amount + " " + e.resource;
                    break;
                case EffectKind.DrawCard:
                    each = e.amount * s.drawValue;
                    why = when + ": draw " + e.amount;
                    break;
                case EffectKind.AddCardToHand:
                    var made = s.cardLookup?.Invoke(e.cardId);
                    double extra = 0;
                    if (made == null) warning = "Can't find card '" + e.cardId + "' to price AddCardToHand.";
                    else if (depth < 2) extra = Math.Max(0, Evaluate(made, s, depth + 1).Value - s.Par(Evaluate(made, s, depth + 1).Cost));
                    each = e.amount * (s.madeCardValue + extra);
                    why = when + ": add " + e.amount + " " + (made?.displayName ?? e.cardId) + " card to hand";
                    break;
                case EffectKind.ConvertResource:
                    each = (e.amount * s.ValueOf(e.resource) - e.fromAmount * s.ValueOf(e.fromResource)) * s.converterEfficiency;
                    why = when + ": " + e.fromAmount + " " + e.fromResource + " -> " + e.amount + " " + e.resource;
                    break;
                case EffectKind.GainPerTouching:
                    double touching = string.IsNullOrEmpty(e.tag) ? s.expectedTouchingAny : s.expectedTouchingTagged;
                    each = e.amount * s.ValueOf(e.resource) * touching;
                    why = when + ": " + e.amount + " " + e.resource + " per touching " + (string.IsNullOrEmpty(e.tag) ? "card" : e.tag) +
                          " (x" + touching.ToString("0.#") + ")";
                    break;
                default:
                    if (e.customEffectId != null && s.customEffectValue.TryGetValue(e.customEffectId, out var cv))
                    {
                        each = cv;
                        why = when + ": " + e.customEffectId + " (hand-set)";
                    }
                    else
                    {
                        each = 0;
                        why = when + ": " + (e.customEffectId ?? e.kind.ToString());
                        warning = "No value for custom effect '" + e.customEffectId + "'. Add it to customEffectValue.";
                    }
                    break;
            }
            return each * fires;
        }

        private static string When(EffectDefinition e)
        {
            if (e.trigger == EffectTrigger.OnPlay) return "on play";
            string unit = e.trigger == EffectTrigger.Day ? "day" : "night";
            int n = Math.Max(1, e.interval);
            if (!e.repeat) return "once after " + n + " " + unit + (n == 1 ? "" : "s");
            return n == 1 ? "every " + unit : "every " + n + " " + unit + "s";
        }

        /// <summary>Scores every card and sorts them, most out-of-line first.</summary>
        public static List<CardScore> EvaluateAll(IEnumerable<CardDefinition> cards, BalanceSettings s = null)
        {
            s = s ?? new BalanceSettings();
            var unique = cards.Where(c => c != null).GroupBy(c => c.id).Select(g => g.First()).ToList();
            if (s.cardLookup == null)
            {
                var byId = unique.Where(c => c.id != null).ToDictionary(c => c.id);
                s.cardLookup = id => id != null && byId.TryGetValue(id, out var c) ? c : null;
            }
            return unique.Select(c => Evaluate(c, s)).OrderByDescending(c => Math.Abs(c.Delta)).ToList();
        }
    }
}
