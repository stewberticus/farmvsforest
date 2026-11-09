using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The Farm team's designed cards (first pass, agreed with Alexander 2026-11-09). See docs/teams/FARM.md.
    /// Core loop: land -> Wheat -> Milk and Eggs. Crops make Wheat, Wheat raises animals, animals make Milk and Eggs,
    /// and Milk and Eggs pay for buildings that put new cards into your hand.
    /// </summary>
    public static class FarmCards
    {
        public const string Crop = "crop";
        public const string Animal = "animal";
        public const string Building = "building";

        public const string FarmId = "farm.farm";
        public const string WheatFieldId = "farm.wheat_field";
        public const string ScarecrowId = "farm.scarecrow";
        public const string CowId = "farm.cow";
        public const string ChickenCoopId = "farm.chicken_coop";
        public const string BarnCatId = "farm.barn_cat";
        public const string BigRedBarnId = "farm.big_red_barn";
        public const string NestingBoxId = "farm.nesting_box";
        public const string FarmhandsCottageId = "farm.farmhands_cottage";

        /// <summary>The corner card. It does nothing: it's just where the farm grows from.</summary>
        public static CardDefinition Starting() =>
            Card(FarmId, "Farm", "Home sweet home. (Does nothing.)", Cost(), new[] { Tags.Start });

        /// <summary>One of each Farm deck card.</summary>
        public static List<CardDefinition> All() => new List<CardDefinition>
        {
            Card(WheatFieldId, "Wheat Field", "Every day: gain 1 Wheat.", Cost(), new[] { Crop },
                Gain(EffectTrigger.Day, ResourceType.Wheat, 1)),

            Card(ScarecrowId, "Scarecrow", "Every day: gain 1 Wheat for each touching crop. Shoos the crows away.",
                Cost(ResourceType.Wheat, 1), new string[0],
                PerTouching(EffectTrigger.Day, ResourceType.Wheat, 1, Crop)),

            Card(CowId, "Cow", "Every day: gain 1 Milk for each touching crop. She's happiest grazing by the fields.",
                Cost(ResourceType.Wheat, 3), new[] { Animal },
                PerTouching(EffectTrigger.Day, ResourceType.Milk, 1, Crop)),

            Card(ChickenCoopId, "Chicken Coop", "Every night: gain 1 Egg for each touching crop. The hens peck spilled grain and leave eggs by morning.",
                Cost(ResourceType.Wheat, 3), new[] { Animal },
                PerTouching(EffectTrigger.Night, ResourceType.Eggs, 1, Crop)),

            Card(BarnCatId, "Barn Cat", "Every night: gain 1 Wheat for each touching crop. A saucer of milk, and she keeps the mice out of the fields.",
                Cost(ResourceType.Milk, 1), new[] { Animal },
                PerTouching(EffectTrigger.Night, ResourceType.Wheat, 1, Crop)),

            Card(BigRedBarnId, "Big Red Barn", "Every 2 days: add a Cow to your hand. Calves are born in the barn.",
                Cost(ResourceType.Milk, 3), new[] { Building },
                AddToHand(EffectTrigger.Day, CowId, 2)),

            Card(NestingBoxId, "Nesting Box", "Every 2 days: add a Chicken Coop to your hand. Eggs hatch into chicks, and chicks need a coop.",
                Cost(ResourceType.Eggs, 3), new[] { Building },
                AddToHand(EffectTrigger.Day, ChickenCoopId, 2)),

            Card(FarmhandsCottageId, "Farmhand's Cottage", "Every 2 days: add a Wheat Field to your hand. Feed a farmhand a good breakfast and they'll plant a new field.",
                Cost(ResourceType.Milk, 1, ResourceType.Eggs, 1), new[] { Building },
                AddToHand(EffectTrigger.Day, WheatFieldId, 2)),
        };

        /// <summary>How many copies of each card go in the 30-card starter deck.</summary>
        public static readonly IReadOnlyDictionary<string, int> DeckCopies = new Dictionary<string, int>
        {
            { WheatFieldId, 10 },
            { ScarecrowId, 3 },
            { CowId, 4 },
            { ChickenCoopId, 4 },
            { BarnCatId, 3 },
            { BigRedBarnId, 2 },
            { NestingBoxId, 2 },
            { FarmhandsCottageId, 2 },
        };

        public static List<CardDefinition> StarterDeck()
        {
            var deck = new List<CardDefinition>();
            foreach (var card in All())
                for (int i = 0; i < (DeckCopies.TryGetValue(card.id, out var n) ? n : 0); i++)
                    deck.Add(card);
            return deck;
        }

        // ---------------------------------------------------------------- helpers

        private static CardDefinition Card(string id, string name, string text, List<ResourceAmount> cost, string[] tags,
            params EffectDefinition[] effects) =>
            new CardDefinition
            {
                id = id, displayName = name, team = Team.Farm, rulesText = text, cost = cost,
                tags = tags.ToList(), effects = effects.ToList(),
            };

        private static List<ResourceAmount> Cost(params object[] pairs)
        {
            var list = new List<ResourceAmount>();
            for (int i = 0; i + 1 < pairs.Length; i += 2) list.Add(new ResourceAmount((ResourceType)pairs[i], (int)pairs[i + 1]));
            return list;
        }

        private static EffectDefinition Gain(EffectTrigger trigger, ResourceType resource, int amount) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource);

        private static EffectDefinition PerTouching(EffectTrigger trigger, ResourceType resource, int amount, string tag) =>
            new EffectDefinition(trigger, EffectKind.GainPerTouching, amount, resource) { tag = tag };

        private static EffectDefinition AddToHand(EffectTrigger trigger, string cardId, int interval) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, 1, interval: interval) { cardId = cardId, skipIfInHand = true };
    }
}
