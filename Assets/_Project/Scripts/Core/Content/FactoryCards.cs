using System;
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
        public const string CatalogueId = "factory.catalogue";
        public const string BigOrderId = "factory.big_order";
        /// <summary>Days from playing Big Order to its payoff.</summary>
        public const int BigOrderDays = 3;

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

            Card(CatalogueId, "Mail-Order Catalogue", "When played: gain 1 Part, then look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Circle the good bits!",
                Free(), new string[0], Gain(EffectTrigger.OnPlay, Parts, 1),
                new EffectDefinition(EffectTrigger.OnPlay, EffectKind.Scout, 3)),

            Card(BlueprintDeskId, "Blueprint Desk", "Every 2 days: add a Toy Assembler to your hand.", Cost(2),
                new string[0], AddToHand(EffectTrigger.Day, ToyAssemblerId)),

            Card(ChargingDockId, "Robot Charging Dock", "Every 2 nights: add a Moonlight Robot to your hand.", Cost(3),
                new string[0], AddToHand(EffectTrigger.Night, MoonlightRobotId)),

            Card(BigOrderId, "Big Order", "In " + BigOrderDays + " days: every Assembler you own runs twice that day. A whole town's worth of birthday presents, due Friday!",
                Cost(4), new[] { "order" },
                new EffectDefinition(EffectTrigger.Day, EffectKind.Custom, 2, interval: BigOrderDays, repeat: false)
                    { customEffectId = RunTaggedCardsEffect.Id, tag = Tags.Assembler }),

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
            { WrappingStationId, 3 },
            // Spare Parts Bin (0.4 plays a game) made room for the Mail-Order Catalogue on 2026-11-11.
            { SparePartsBinId, 0 },
            { CatalogueId, 3 },
            { BlueprintDeskId, 1 },
            { ChargingDockId, 1 },
            // Big Whistle (never played by the bot) made way for Big Order on 2026-11-11.
            { BigWhistleId, 0 },
            { BigOrderId, 2 },
        };

        public static List<CardDefinition> Deck()
        {
            var deck = new List<CardDefinition>();
            foreach (var card in All())
                if (DeckCounts.TryGetValue(card.id, out int copies))
                    deck.AddRange(Enumerable.Repeat(card, copies));
            return deck;
        }

        // ---------------------------------------------------------------- Big Order and the Toy forecast

        /// <summary>Days until this card's Big Order pays off, or -1 if it isn't a Big Order waiting to pay off.</summary>
        public static int OrderDaysLeft(CardInstance card)
        {
            var e = card?.Definition.effects?.FirstOrDefault(x => !x.repeat && x.trigger == EffectTrigger.Day &&
                                                                  x.customEffectId == RunTaggedCardsEffect.Id);
            if (e == null || !card.IsOnBoard) return -1;
            int left = e.interval - card.DaysSeen;
            return left >= 1 ? left : -1;
        }

        /// <summary>Toys the player has made this round, plus what their cards will still make before it ends.</summary>
        public static int ProjectedToysThisRound(Match match, PlayerState p)
        {
            int made = p.GainedThisRound.Get(Toys);
            if (match.TimeOfDay == TimeOfDay.Night) return made;
            return made + ToysFrom(match, p, EffectTrigger.Night, p.Resources.Get(Parts), 0);
        }

        /// <summary>
        /// Rough forecast of the Toys made on a whole round <paramref name="daysAhead"/> days from now, if the player
        /// keeps what's on the board: Parts income piles up until then, Assemblers convert, Wrapping Stations wrap, and
        /// Big Orders due that day run every Assembler twice more. <paramref name="extraOrders"/> adds orders not yet played.
        /// </summary>
        public static int ForecastToys(Match match, PlayerState p, int daysAhead, int extraOrders = 0)
        {
            var mine = match.Board.AllCards().Where(c => c.Owner == p).ToList();
            int partsIncome = 0, assemblers = 0;
            foreach (var c in mine)
            foreach (var e in c.Definition.effects ?? new List<EffectDefinition>())
            {
                if (e.resource == Parts && (e.kind == EffectKind.GainResource || e.customEffectId == StartingIncomeEffect.Id) && e.repeat &&
                    e.trigger != EffectTrigger.OnPlay)
                    partsIncome += e.amount * (e.interval <= 1 ? 1 : 0);
                if (e.kind == EffectKind.GainPerTouching && e.resource == Parts)
                    partsIncome += e.amount * match.Board.Neighbours(c.Position).Select(match.Board.Get)
                        .Count(n => n != null && n.Owner == p && (string.IsNullOrEmpty(e.tag) || n.Definition.HasTag(e.tag)));
            }
            assemblers = mine.Count(c => c.Definition.HasTag(Tags.Assembler));
            int orders = extraOrders + mine.Count(c => OrderDaysLeft(c) == daysAhead);
            // Parts saved up by then: each round the Assemblers use 2 each if they can.
            int parts = p.Resources.Get(Parts);
            for (int d = 0; d < daysAhead; d++) parts = Math.Max(Math.Min(parts, AssemblerKeepsParts), parts + partsIncome - 2 * assemblers);
            parts += partsIncome;
            int runs = assemblers * (1 + 2 * orders);
            int converts = Math.Min(runs, Math.Max(0, (parts - AssemblerKeepsParts) / 2));
            int wrapped = mine.Where(c => c.Definition.effects != null && c.Definition.effects.Any(e => e.kind == EffectKind.GainPerTouching && e.resource == Toys))
                .Sum(c => match.Board.Neighbours(c.Position).Select(match.Board.Get).Count(n => n != null && n.Owner == p && n.Definition.HasTag(Tags.Assembler)));
            return converts + wrapped;
        }

        private static int ToysFrom(Match match, PlayerState p, EffectTrigger trigger, int parts, int toys)
        {
            foreach (var c in match.Board.AllCards().Where(c => c.Owner == p).OrderBy(c => c.InstanceId))
            foreach (var e in c.Definition.effects ?? new List<EffectDefinition>())
            {
                if (e.trigger != trigger || !e.FiresOn(trigger == EffectTrigger.Day ? c.DaysSeen + 1 : c.NightsSeen + 1)) continue;
                if (e.kind == EffectKind.ConvertResource && e.resource == Toys && parts >= e.fromAmount + e.fromKeep)
                {
                    parts -= e.fromAmount;
                    toys += e.amount;
                }
                else if (e.kind == EffectKind.GainPerTouching && e.resource == Toys)
                    toys += e.amount * match.Board.Neighbours(c.Position).Select(match.Board.Get)
                        .Count(n => n != null && n.Owner == p && (string.IsNullOrEmpty(e.tag) || n.Definition.HasTag(e.tag)));
                else if (e.kind == EffectKind.GainResource && e.resource == Toys) toys += e.amount;
            }
            return toys;
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
