using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// Stand-in cards so the systems can be played and tested before real content exists.
    /// Every name, number and effect here is a placeholder; replace them with designed cards
    /// (as CardDefinitionAssets in Unity) whenever you're ready. The starting cards' names are decided.
    /// </summary>
    public static class PlaceholderCards
    {
        // ---------------------------------------------------------------- starting corner cards

        public static CardDefinition StartingCard(Team team)
        {
            switch (team)
            {
                case Team.Farm:
                    return Card("farm.farm", "Farm", team, "Every day: gain 1 Wheat.", Cost(),
                        new[] { Tags.Start }, Every(EffectTrigger.Day, ResourceType.Wheat, 1));
                case Team.Forest:
                    return ForestCards.HugeTree;
                case Team.Factory:
                    return Card("factory.headquarters", "Headquarters", team, "Every day: gain 1 Part.", Cost(),
                        new[] { Tags.Start }, Every(EffectTrigger.Day, ResourceType.Parts, 1));
                default:
                    return DarkForestCards.EvilTree;
            }
        }

        // ---------------------------------------------------------------- deck cards

        public static List<CardDefinition> ForTeam(Team team)
        {
            switch (team)
            {
                case Team.Farm: return Farm();
                case Team.Forest: return ForestCards.All();
                case Team.Factory: return Factory();
                default: return DarkForest();
            }
        }

        public static IEnumerable<CardDefinition> All() =>
            new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest }.SelectMany(ForTeam);

        /// <summary>Dark Forest: its designed 30-card deck. Others: 12 copies of the free card and 7 of each other card.</summary>
        public static List<CardDefinition> StarterDeck(Team team)
        {
            if (team == Team.DarkForest) return DarkForestCards.StarterDeck();
            if (team == Team.Forest) return ForestCards.Deck();
            var deck = new List<CardDefinition>();
            foreach (var c in ForTeam(team))
            {
                int copies = c.IsFree ? 12 : 7;
                for (int i = 0; i < copies; i++) deck.Add(c);
            }
            return deck;
        }

        private static List<CardDefinition> Farm()
        {
            const Team T = Team.Farm;
            return new List<CardDefinition>
            {
                Card("farm.wheat_field", "Wheat Field", T, "Every day: gain 1 Wheat.", Cost(), new[] { "crop" },
                    Every(EffectTrigger.Day, ResourceType.Wheat, 1)),
                Card("farm.cow", "Cow", T, "Every day: gain 1 Milk.", Cost(ResourceType.Wheat, 2), new[] { "animal" },
                    Every(EffectTrigger.Day, ResourceType.Milk, 1)),
                Card("farm.chicken_coop", "Chicken Coop", T, "Every day: gain 1 Egg for each touching crop.",
                    Cost(ResourceType.Wheat, 2), new[] { "animal" },
                    new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1, ResourceType.Eggs) { tag = "crop" }),
                Card("farm.barn", "Barn", T, "When played: draw 2 cards.", Cost(ResourceType.Wheat, 3), new string[0],
                    new EffectDefinition(EffectTrigger.OnPlay, EffectKind.DrawCard, 2)),
                Card("farm.barn_cat", "Sleepy Barn Cat", T, "Every night: gain 1 Wheat.", Cost(ResourceType.Milk, 1),
                    new[] { "animal" }, Every(EffectTrigger.Night, ResourceType.Wheat, 1)),
            };
        }

        private static List<CardDefinition> Factory()
        {
            const Team T = Team.Factory;
            return new List<CardDefinition>
            {
                Card("factory.scrap_pile", "Scrap Pile", T, "Every day: gain 1 Part.", Cost(), new string[0],
                    Every(EffectTrigger.Day, ResourceType.Parts, 1)),
                Card("factory.workshop", "Workshop", T, "Every day: gain 2 Parts.", Cost(ResourceType.Parts, 2), new string[0],
                    Every(EffectTrigger.Day, ResourceType.Parts, 2)),
                Card("factory.assembler", "Night Shift Assembler", T, "Every night: turn 2 Parts into 1 Goods.",
                    Cost(ResourceType.Parts, 3), new string[0],
                    new EffectDefinition(EffectTrigger.Night, EffectKind.ConvertResource, 1, ResourceType.Goods)
                        { fromResource = ResourceType.Parts, fromAmount = 2 }),
                Card("factory.clock_tower", "Clock Tower", T, "Once, after 3 days: gain 4 Goods.", Cost(ResourceType.Parts, 4),
                    new string[0], Every(EffectTrigger.Day, ResourceType.Goods, 4, interval: 3, repeat: false)),
                Card("factory.warehouse", "Warehouse", T, "When played: draw a card.", Cost(ResourceType.Parts, 1), new string[0],
                    new EffectDefinition(EffectTrigger.OnPlay, EffectKind.DrawCard, 1)),
            };
        }

        private static List<CardDefinition> DarkForest() => DarkForestCards.All();

        // ---------------------------------------------------------------- config

        /// <summary>A ready-to-play config with placeholder decks and starting cards, for tests and the prototype scene.</summary>
        public static MatchConfig DefaultConfig(GameMode mode, int seed = 12345, params Team[] teams)
        {
            if (teams == null || teams.Length == 0) teams = new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest };
            var config = new MatchConfig { mode = mode, seed = seed, cardCatalog = All().ToList() };
            for (int i = 0; i < GameModes.PlayerCount(mode); i++)
            {
                var team = teams[i % teams.Length];
                config.players.Add(new PlayerSetup("Player " + (i + 1), team, StarterDeck(team), StartingCard(team)));
            }
            return config;
        }

        // ---------------------------------------------------------------- helpers

        private static CardDefinition Card(string id, string name, Team team, string text, List<ResourceAmount> cost,
            string[] tags, params EffectDefinition[] effects) =>
            new CardDefinition
            {
                id = id, displayName = name, team = team, rulesText = text, cost = cost,
                tags = tags.ToList(), effects = effects.ToList(),
            };

        private static List<ResourceAmount> Cost() => new List<ResourceAmount>();

        private static List<ResourceAmount> Cost(ResourceType type, int amount) =>
            new List<ResourceAmount> { new ResourceAmount(type, amount) };

        private static List<ResourceAmount> Cost(ResourceType a, int amountA, ResourceType b, int amountB) =>
            new List<ResourceAmount> { new ResourceAmount(a, amountA), new ResourceAmount(b, amountB) };

        private static EffectDefinition Every(EffectTrigger trigger, ResourceType resource, int amount, int interval = 1,
            bool repeat = true) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource, interval, repeat);
    }
}
