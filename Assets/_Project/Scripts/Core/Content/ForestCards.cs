using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The Forest team's designed cards. See docs/teams/FOREST.md for the why behind each one.
    /// Forest grows: trees grow Sapling, Young Tree, Old Tree on sunny mornings (touching an empty square), and the goal
    /// is Old Trees in one forest. Generator cards add Saplings to your hand, and the Forest rule lets you spend Acorns
    /// to play extra trees each turn (<see cref="MatchConfig.forestExtraTreeAcorns"/>). Numbers are tuned by sim.
    /// </summary>
    public static class ForestCards
    {
        private const Team T = Team.Forest;

        public const string HugeTreeId = "forest.huge_tree";
        public const string SaplingId = "forest.sapling";
        /// <summary>Squirrel Dreys make more next to these.</summary>
        public const string OakTag = "oak";
        /// <summary>Custom effect id: "Grows into X after N sunny mornings".</summary>
        public const string GrowEffect = "forest.grow";
        /// <summary>Sunny mornings a Sapling needs to become a Young Tree.</summary>
        public const int SaplingGrowDays = 2;
        /// <summary>Sunny mornings a young tree (Young Tree, Oak, Pine, Willow) needs to become old.</summary>
        public const int YoungGrowDays = 4;

        // ---------------------------------------------------------------- trees and their grown-up forms

        /// <summary>Starting corner card. Already an Old Tree; does nothing else.</summary>
        public static readonly CardDefinition HugeTree = Card(HugeTreeId, "Huge Tree",
            "Your forest's first Old Tree. Does nothing else.", Free(), new[] { Tags.Start, Tags.Tree, Tags.Old });

        public static readonly CardDefinition OldTree = Card("forest.old_tree", "Old Tree",
            "A fully grown tree.", Free(), new[] { Tags.Tree, Tags.Old });

        public static readonly CardDefinition YoungTree = Card("forest.young_tree", "Young Tree",
            "Grows into an Old Tree after " + YoungGrowDays + " sunny mornings.", Free(), new[] { Tags.Tree },
            Grow(OldTree, YoungGrowDays));

        public static readonly CardDefinition Sapling = Card(SaplingId, "Sapling",
            "Grows into a Young Tree after " + SaplingGrowDays + " sunny mornings.", Free(), new[] { Tags.Tree },
            Grow(YoungTree, SaplingGrowDays));

        public static readonly CardDefinition GreatOak = Card("forest.great_oak", "Great Oak",
            "Every day: gain 1 Acorn.", Free(), new[] { Tags.Tree, Tags.Old, OakTag },
            Gain(EffectTrigger.Day, ResourceType.Acorns, 1));

        public static readonly CardDefinition Oak = Card("forest.oak", "Oak",
            "Every day: gain 1 Acorn. Grows into a Great Oak after " + YoungGrowDays + " sunny mornings.",
            Cost(ResourceType.Acorns, 2), new[] { Tags.Tree, OakTag },
            Gain(EffectTrigger.Day, ResourceType.Acorns, 1), Grow(GreatOak, YoungGrowDays));

        public static readonly CardDefinition TallPine = Card("forest.tall_pine", "Tall Pine",
            "Every 2 nights: add a Sapling to your hand.", Free(), new[] { Tags.Tree, Tags.Old },
            AddSapling(EffectTrigger.Night, 2));

        public static readonly CardDefinition Pine = Card("forest.pine", "Pine",
            "Every 2 nights: add a Sapling to your hand. Grows into a Tall Pine after " + YoungGrowDays + " sunny mornings.",
            Cost(ResourceType.Acorns, 2), new[] { Tags.Tree },
            AddSapling(EffectTrigger.Night, 2), Grow(TallPine, YoungGrowDays));

        public static readonly CardDefinition GrandWillow = Card("forest.grand_willow", "Grand Willow",
            "Every day: add a Sapling to your hand.", Free(), new[] { Tags.Tree, Tags.Old },
            AddSapling(EffectTrigger.Day, 1));

        public static readonly CardDefinition WeepingWillow = Card("forest.weeping_willow", "Weeping Willow",
            "Every day: add a Sapling to your hand. Grows into a Grand Willow after " + YoungGrowDays + " sunny mornings.",
            Cost(ResourceType.Acorns, 4, ResourceType.Honey, 2), new[] { Tags.Tree },
            AddSapling(EffectTrigger.Day, 1), Grow(GrandWillow, YoungGrowDays));

        // ---------------------------------------------------------------- animals and flowers

        public static readonly CardDefinition SquirrelDrey = Card("forest.squirrel_drey", "Squirrel Drey",
            "Every day: gain 1 Acorn, plus 1 for each Oak touching this.", Free(), new[] { "animal" },
            Gain(EffectTrigger.Day, ResourceType.Acorns, 1),
            new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1, ResourceType.Acorns) { tag = OakTag });

        public static readonly CardDefinition WildflowerMeadow = Card("forest.wildflower_meadow", "Wildflower Meadow",
            "Every day: gain 1 Honey for each empty square touching this.", Free(), new[] { "flower" },
            new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerEmptyTouching, 1, ResourceType.Honey));

        public static readonly CardDefinition BlossomHive = Card("forest.blossom_hive", "Blossom Hive",
            "Every day: gain 1 Honey for each tree touching this.", Cost(ResourceType.Acorns, 3), new[] { "animal" },
            new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1, ResourceType.Honey) { tag = Tags.Tree });

        public static readonly CardDefinition DormouseDen = Card("forest.dormouse_den", "Dormouse Den",
            "Every night: gain 2 Acorns.", Cost(ResourceType.Honey, 3), new[] { "animal" },
            Gain(EffectTrigger.Night, ResourceType.Acorns, 2));

        public static readonly CardDefinition BlueJaysNest = Card("forest.blue_jays_nest", "Blue Jay's Nest",
            "Every 2 days: add a Sapling to your hand.", Cost(ResourceType.Honey, 2), new[] { "animal" },
            AddSapling(EffectTrigger.Day, 2));

        /// <summary>Every Forest card by id, including grown forms that only appear on the board.</summary>
        public static readonly Dictionary<string, CardDefinition> ById = new[]
        {
            HugeTree, OldTree, YoungTree, Sapling, GreatOak, Oak, TallPine, Pine, GrandWillow, WeepingWillow,
            SquirrelDrey, WildflowerMeadow, BlossomHive, DormouseDen, BlueJaysNest,
        }.ToDictionary(c => c.id);

        /// <summary>Registers the growth effect.</summary>
        public static void RegisterEffects(EffectRegistry registry) => registry.Register(GrowEffect, new GrowthEffect());

        /// <summary>
        /// Every morning the card touches an empty square (sunlight), it saves up one growth step. After
        /// <c>amount</c> steps it turns into the card <c>cardId</c>, in place.
        /// </summary>
        public class GrowthEffect : IEffect
        {
            public void Apply(EffectContext ctx)
            {
                var card = ctx.Source;
                var board = ctx.Match.Board;
                if (!board.Neighbours(card.Position).Any(board.IsEmpty)) return;
                card.GrowthProgress++;
                if (card.GrowthProgress < ctx.Effect.amount) return;
                var into = ctx.Match.FindCard(ctx.Effect.cardId) ??
                           (ById.TryGetValue(ctx.Effect.cardId ?? "", out var d) ? d : null);
                ctx.Match.Transform(card, into);
            }
        }

        /// <summary>True if this card grows (has a growth effect).</summary>
        public static bool Grows(CardDefinition def) =>
            def.effects != null && def.effects.Any(e => e.kind == EffectKind.Custom && e.customEffectId == GrowEffect);

        /// <summary>Every distinct Forest deck card (not the starting card).</summary>
        public static List<CardDefinition> All() => DeckList.Select(e => e.card).ToList();

        /// <summary>The starter deck list: card and number of copies.</summary>
        public static readonly (CardDefinition card, int copies)[] DeckList =
        {
            (Sapling, 10),
            (SquirrelDrey, 4),
            (WildflowerMeadow, 3),
            (Oak, 3),
            (BlossomHive, 2),
            (DormouseDen, 2),
            (Pine, 3),
            (BlueJaysNest, 2),
            (WeepingWillow, 1),
        };

        /// <summary>The 30-card starter deck, one entry per copy.</summary>
        public static List<CardDefinition> Deck() =>
            DeckList.SelectMany(e => Enumerable.Repeat(e.card, e.copies)).ToList();

        // ---------------------------------------------------------------- helpers

        private static CardDefinition Card(string id, string name, string text, List<ResourceAmount> cost, string[] tags,
            params EffectDefinition[] effects) =>
            new CardDefinition
            {
                id = id, displayName = name, team = T, rulesText = text, cost = cost,
                tags = tags.ToList(), effects = effects.ToList(),
            };

        private static List<ResourceAmount> Free() => new List<ResourceAmount>();

        private static List<ResourceAmount> Cost(ResourceType type, int amount) =>
            new List<ResourceAmount> { new ResourceAmount(type, amount) };

        private static List<ResourceAmount> Cost(ResourceType a, int amountA, ResourceType b, int amountB) =>
            new List<ResourceAmount> { new ResourceAmount(a, amountA), new ResourceAmount(b, amountB) };

        private static EffectDefinition Gain(EffectTrigger trigger, ResourceType resource, int amount) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource);

        private static EffectDefinition Grow(CardDefinition into, int sunnyDays) =>
            new EffectDefinition(EffectTrigger.Day, EffectKind.Custom, sunnyDays)
                { customEffectId = GrowEffect, cardId = into.id };

        private static EffectDefinition AddSapling(EffectTrigger trigger, int interval) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, 1, interval: interval) { cardId = SaplingId };
    }
}
