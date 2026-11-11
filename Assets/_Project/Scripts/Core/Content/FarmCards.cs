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
        /// <summary>Upgrade cards, played onto one of your own cards (see <see cref="CardDefinition.upgradeFor"/>).</summary>
        public const string Upgrade = "upgrade";
        /// <summary>
        /// Animals (and their upgrades) make Milk or Eggs every this many days or nights. 2 since phase 6 (2026-11-11),
        /// which brought the goal down from 66 to a countable number.
        /// </summary>
        public const int AnimalDays = 2;

        public const string FarmId = "farm.farm";
        public const string WheatFieldId = "farm.wheat_field";
        public const string ScarecrowId = "farm.scarecrow";
        public const string CowId = "farm.cow";
        public const string ChickenCoopId = "farm.chicken_coop";
        public const string BarnCatId = "farm.barn_cat";
        public const string BigRedBarnId = "farm.big_red_barn";
        public const string NestingBoxId = "farm.nesting_box";
        public const string FarmhandsCottageId = "farm.farmhands_cottage";
        public const string WeathervaneId = "farm.weathervane";

        /// <summary>The corner card: where the farm grows from. Makes 1 Wheat a day so you can always get going.</summary>
        public static CardDefinition Starting() =>
            Card(FarmId, "Farm", "Every day: gain 1 Wheat. Home sweet home, with a little vegetable patch out back.",
                Cost(), new[] { Tags.Start }, StartingIncomeEffect.Every(EffectTrigger.Day, ResourceType.Wheat));

        /// <summary>One of each Farm deck card.</summary>
        public static List<CardDefinition> All() => new List<CardDefinition>
        {
            Card(WheatFieldId, "Wheat Field", "Every day: gain 1 Wheat.", Cost(), new[] { Crop },
                Gain(EffectTrigger.Day, ResourceType.Wheat, 1)),

            Card(ScarecrowId, "Scarecrow", "Every day: gain 1 Wheat for each touching crop. Shoos the crows away.",
                Cost(ResourceType.Wheat, 1), new string[0],
                PerTouching(EffectTrigger.Day, ResourceType.Wheat, 1, Crop)),

            Card(CowId, "Cow", "Every 2 days: gain 1 Milk for each touching crop. She's happiest grazing by the fields.",
                Cost(ResourceType.Wheat, 3), new[] { Animal },
                PerTouching(EffectTrigger.Day, ResourceType.Milk, 1, Crop, interval: AnimalDays)),

            Card(ChickenCoopId, "Chicken Coop", "Every 2 nights: gain 1 Egg for each touching crop. The hens peck spilled grain and leave eggs by morning.",
                Cost(ResourceType.Wheat, 3), new[] { Animal },
                PerTouching(EffectTrigger.Night, ResourceType.Eggs, 1, Crop, interval: AnimalDays)),

            Card(BarnCatId, "Barn Cat", "Every night: gain 1 Wheat for each touching crop. A saucer of milk, and she keeps the mice out of the fields.",
                Cost(ResourceType.Milk, 1), new[] { Animal },
                PerTouching(EffectTrigger.Night, ResourceType.Wheat, 1, Crop)),

            UpgradeCard(BigRedBarnId, "Big Red Barn", CowId,
                "Upgrade: play onto one of your Cows. Every 2 days: gain 1 more Milk for each crop touching her. A warm barn and fresh straw make for happy cows.",
                Cost(ResourceType.Milk, 3),
                PerTouching(EffectTrigger.Day, ResourceType.Milk, 1, Crop, interval: AnimalDays)),

            UpgradeCard(NestingBoxId, "Nesting Box", ChickenCoopId,
                "Upgrade: play onto one of your Chicken Coops. Every 2 nights: gain 1 more Egg for each crop touching it. Soft hay in every box, and the hens lay twice as well.",
                Cost(ResourceType.Eggs, 3),
                PerTouching(EffectTrigger.Night, ResourceType.Eggs, 1, Crop, interval: AnimalDays)),

            Card(WeathervaneId, "Weathervane", "When played: look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Every 2 days: gain 1 Wheat. The rooster on top always knows which way the wind blows.",
                Cost(ResourceType.Wheat, 1), new string[0],
                new EffectDefinition(EffectTrigger.OnPlay, EffectKind.Scout, 3),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Wheat, interval: 2)),

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
            { WeathervaneId, 2 },
            // Farmhand's Cottage (0.3 plays a game) made room for the Weathervane on 2026-11-11.
            { FarmhandsCottageId, 0 },
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

        private static CardDefinition UpgradeCard(string id, string name, string onto, string text, List<ResourceAmount> cost,
            params EffectDefinition[] effects)
        {
            var card = Card(id, name, text, cost, new[] { Upgrade }, effects);
            card.upgradeFor = onto;
            return card;
        }

        private static List<ResourceAmount> Cost(params object[] pairs)
        {
            var list = new List<ResourceAmount>();
            for (int i = 0; i + 1 < pairs.Length; i += 2) list.Add(new ResourceAmount((ResourceType)pairs[i], (int)pairs[i + 1]));
            return list;
        }

        private static EffectDefinition Gain(EffectTrigger trigger, ResourceType resource, int amount) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource);

        private static EffectDefinition PerTouching(EffectTrigger trigger, ResourceType resource, int amount, string tag,
            int interval = 1, int maxCount = 0) =>
            new EffectDefinition(trigger, EffectKind.GainPerTouching, amount, resource, interval: interval) { tag = tag, maxCount = maxCount };

        private static EffectDefinition AddToHand(EffectTrigger trigger, string cardId, int interval) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, 1, interval: interval) { cardId = cardId, skipIfInHand = true };
    }
}
