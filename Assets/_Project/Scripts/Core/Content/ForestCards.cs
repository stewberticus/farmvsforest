using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The Forest team's designed cards. See docs/teams/FOREST.md for the why behind each one.
    /// Forest spreads: generator cards add Saplings to your hand, and the Forest rule lets you spend Acorns to play
    /// extra trees each turn (<see cref="MatchConfig.forestExtraTreeAcorns"/>). Numbers are starting values, tuned by sim.
    /// </summary>
    public static class ForestCards
    {
        private const Team T = Team.Forest;

        public const string HugeTreeId = "forest.huge_tree";
        public const string SaplingId = "forest.sapling";

        /// <summary>Starting corner card. Does nothing; it's tree 1 of your forest.</summary>
        public static readonly CardDefinition HugeTree = Card(HugeTreeId, "Huge Tree",
            "Your forest's first tree. Does nothing else.", Free(), new[] { Tags.Start, Tags.Tree });

        public static readonly CardDefinition Sapling = Card(SaplingId, "Sapling",
            "A little tree. Does nothing else.", Free(), new[] { Tags.Tree });

        public static readonly CardDefinition SquirrelDrey = Card("forest.squirrel_drey", "Squirrel Drey",
            "Every day: gain 1 Acorn.", Free(), new[] { "animal" },
            Gain(EffectTrigger.Day, ResourceType.Acorns, 1));

        public static readonly CardDefinition WildBeesNest = Card("forest.wild_bees_nest", "Wild Bees' Nest",
            "Every 2 days: gain 1 Honey.", Free(), new[] { "animal" },
            new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Honey, interval: 2));

        public static readonly CardDefinition Oak = Card("forest.oak", "Oak",
            "Every day: gain 1 Acorn.", Cost(ResourceType.Acorns, 2), new[] { Tags.Tree },
            Gain(EffectTrigger.Day, ResourceType.Acorns, 1));

        public static readonly CardDefinition BlossomHive = Card("forest.blossom_hive", "Blossom Hive",
            "Every day: gain 1 Honey for each tree touching this.", Cost(ResourceType.Acorns, 3), new[] { "animal" },
            new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1, ResourceType.Honey) { tag = Tags.Tree });

        public static readonly CardDefinition DormouseDen = Card("forest.dormouse_den", "Dormouse Den",
            "Every night: gain 2 Acorns.", Cost(ResourceType.Honey, 3), new[] { "animal" },
            Gain(EffectTrigger.Night, ResourceType.Acorns, 2));

        public static readonly CardDefinition Pine = Card("forest.pine", "Pine",
            "Every 2 nights: add a Sapling to your hand.", Cost(ResourceType.Acorns, 2), new[] { Tags.Tree },
            AddSapling(EffectTrigger.Night, 2));

        public static readonly CardDefinition BlueJaysNest = Card("forest.blue_jays_nest", "Blue Jay's Nest",
            "Every 2 days: add a Sapling to your hand.", Cost(ResourceType.Honey, 2), new[] { "animal" },
            AddSapling(EffectTrigger.Day, 2));

        public static readonly CardDefinition WeepingWillow = Card("forest.weeping_willow", "Weeping Willow",
            "Every day: add a Sapling to your hand.", Cost(ResourceType.Acorns, 4, ResourceType.Honey, 2), new[] { Tags.Tree },
            AddSapling(EffectTrigger.Day, 1));

        /// <summary>Every distinct Forest deck card (not the starting card).</summary>
        public static List<CardDefinition> All() => DeckList.Select(e => e.card).ToList();

        /// <summary>The starter deck list: card and number of copies.</summary>
        public static readonly (CardDefinition card, int copies)[] DeckList =
        {
            (Sapling, 10),
            (SquirrelDrey, 4),
            (WildBeesNest, 3),
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

        private static EffectDefinition AddSapling(EffectTrigger trigger, int interval) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, 1, interval: interval) { cardId = SaplingId };
    }
}
