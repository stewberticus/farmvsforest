using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The Factory team: a cozy toy workshop. Parts are gathered (mostly by day), Assemblers turn them into Toys, and
    /// the Factory wins by making <see cref="MatchConfig.factoryGoalToys"/> Toys in one round (day + night).
    /// Design notes: docs/teams/FACTORY.md.
    /// </summary>
    public static class FactoryCards
    {
        private const Team T = Team.Factory;
        private const ResourceType Parts = ResourceType.Parts;
        private const ResourceType Toys = ResourceType.Toys;

        public const string HeadquartersId = "factory.headquarters";
        public const string ScrapPileId = "factory.scrap_pile";
        public const string TinkerShedId = "factory.tinker_shed";
        public const string MoonlightRobotId = "factory.moonlight_robot";
        public const string ToyAssemblerId = "factory.toy_assembler";
        public const string NightShiftAssemblerId = "factory.night_shift_assembler";
        public const string WrappingStationId = "factory.wrapping_station";
        public const string SparePartsBinId = "factory.spare_parts_bin";
        public const string BlueprintDeskId = "factory.blueprint_desk";
        public const string ChargingDockId = "factory.charging_dock";
        public const string BigWhistleId = "factory.big_whistle";

        /// <summary>The corner card: where the workshop starts. Makes 1 Part a day so you can always get going.</summary>
        public static CardDefinition Headquarters() =>
            Card(HeadquartersId, "Headquarters", "Every day: gain 1 Part. Where the workshop begins, and the junk drawer is never empty.",
                Free(), new[] { Tags.Start }, StartingIncomeEffect.Every(EffectTrigger.Day, Parts));

        /// <summary>One of each Factory deck card.</summary>
        public static List<CardDefinition> All() => new List<CardDefinition>
        {
            Card(ScrapPileId, "Scrap Pile", "Every day: gain 1 Part.", Free(), new[] { "scrap" },
                Gain(EffectTrigger.Day, Parts, 1)),

            Card(TinkerShedId, "Tinker Shed", "Every day: gain 1 Part, plus 1 if it touches a Scrap Pile.",
                Cost(2), new[] { "shed" },
                Gain(EffectTrigger.Day, Parts, 1),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1, Parts) { tag = "scrap", }),

            Card(MoonlightRobotId, "Moonlight Robot", "Every night: gain 2 Parts.", Cost(2), new string[0],
                Gain(EffectTrigger.Night, Parts, 2)),

            Card(ToyAssemblerId, "Toy Assembler", "Every day: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts).", Cost(3),
                new[] { Tags.Assembler }, Convert(EffectTrigger.Day)),

            Card(NightShiftAssemblerId, "Night Shift Assembler", "Every night: turn 2 spare Parts into 1 Toy (always leaves you 3 Parts).", Cost(3),
                new[] { Tags.Assembler }, Convert(EffectTrigger.Night)),

            Card(WrappingStationId, "Wrapping Station", "Every night: gain 1 Toy for each Assembler it touches.",
                Cost(3), new string[0],
                new EffectDefinition(EffectTrigger.Night, EffectKind.GainPerTouching, 1, Toys) { tag = Tags.Assembler }),

            Card(SparePartsBinId, "Spare Parts Bin", "When played: gain 2 Parts. Every 3 days: gain 1 Part.", Free(),
                new string[0], Gain(EffectTrigger.OnPlay, Parts, 2),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, Parts, interval: 3)),

            Card(BlueprintDeskId, "Blueprint Desk", "Every 2 days: add a Toy Assembler to your hand.", Cost(2),
                new string[0], AddToHand(EffectTrigger.Day, ToyAssemblerId)),

            Card(ChargingDockId, "Robot Charging Dock", "Every 2 nights: add a Moonlight Robot to your hand.", Cost(3),
                new string[0], AddToHand(EffectTrigger.Night, MoonlightRobotId)),

            Card(BigWhistleId, "Big Whistle", "When played: every Assembler you own runs once right now.", Cost(5),
                new string[0],
                new EffectDefinition(EffectTrigger.OnPlay, EffectKind.Custom, 1)
                    { customEffectId = RunTaggedCardsEffect.Id, tag = Tags.Assembler }),
        };

        /// <summary>How many copies of each card go in the 30-card starting deck.</summary>
        public static readonly Dictionary<string, int> DeckCounts = new Dictionary<string, int>
        {
            { ScrapPileId, 9 },
            { TinkerShedId, 3 },
            { MoonlightRobotId, 2 },
            { ToyAssemblerId, 3 },
            { NightShiftAssemblerId, 3 },
            { WrappingStationId, 4 },
            { SparePartsBinId, 3 },
            { BlueprintDeskId, 1 },
            { ChargingDockId, 1 },
            { BigWhistleId, 1 },
        };

        public static List<CardDefinition> Deck()
        {
            var deck = new List<CardDefinition>();
            foreach (var card in All())
                if (DeckCounts.TryGetValue(card.id, out int copies))
                    deck.AddRange(Enumerable.Repeat(card, copies));
            return deck;
        }

        // ---------------------------------------------------------------- helpers

        private static CardDefinition Card(string id, string name, string text, List<ResourceAmount> cost,
            string[] tags, params EffectDefinition[] effects)
            => new CardDefinition
            {
                id = id, displayName = name, team = T, rulesText = text, cost = cost,
                tags = tags.ToList(), effects = effects.ToList(),
            };

        private static List<ResourceAmount> Free() => new List<ResourceAmount>();

        private static List<ResourceAmount> Cost(int parts) =>
            new List<ResourceAmount> { new ResourceAmount(Parts, parts) };

        private static EffectDefinition Gain(EffectTrigger trigger, ResourceType resource, int amount) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource);

        /// <summary>Assemblers only use spare Parts: they always leave this many for building.</summary>
        public static int AssemblerKeepsParts = 3;

        private static EffectDefinition Convert(EffectTrigger trigger) =>
            new EffectDefinition(trigger, EffectKind.ConvertResource, 1, Toys)
                { fromResource = Parts, fromAmount = 2, fromKeep = AssemblerKeepsParts };

        private static EffectDefinition AddToHand(EffectTrigger trigger, string cardId) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, 1, interval: 2) { cardId = cardId, skipIfInHand = true };
    }
}
