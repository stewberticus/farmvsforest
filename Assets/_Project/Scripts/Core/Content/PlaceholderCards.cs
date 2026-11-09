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
                    return FarmCards.Starting();
                case Team.Forest:
                    return ForestCards.HugeTree;
                case Team.Factory:
                    return Card("factory.headquarters", "Headquarters", team, "Every day: gain 1 Part.", Cost(),
                        new[] { Tags.Start }, Every(EffectTrigger.Day, ResourceType.Parts, 1));
                default:
                    return Card("dark.evil_tree", "Evil Tree", team, "Every night: gain 1 Bat.", Cost(),
                        new[] { Tags.Start, Tags.Tree }, Every(EffectTrigger.Night, ResourceType.Bats, 1));
            }
        }

        // ---------------------------------------------------------------- deck cards

        public static List<CardDefinition> ForTeam(Team team)
        {
            switch (team)
            {
                case Team.Farm: return FarmCards.All();
                case Team.Forest: return ForestCards.All();
                case Team.Factory: return Factory();
                default: return DarkForest();
            }
        }

        public static IEnumerable<CardDefinition> All() =>
            new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest }.SelectMany(ForTeam);

        /// <summary>A 40-card deck: 12 copies of the team's free card and 7 of each other card.</summary>
        public static List<CardDefinition> StarterDeck(Team team)
        {
            if (team == Team.Farm) return FarmCards.StarterDeck();
            if (team == Team.Forest) return ForestCards.Deck();
            var deck = new List<CardDefinition>();
            foreach (var c in ForTeam(team))
            {
                int copies = c.IsFree ? 12 : 7;
                for (int i = 0; i < copies; i++) deck.Add(c);
            }
            return deck;
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

        private static List<CardDefinition> DarkForest()
        {
            const Team T = Team.DarkForest;
            return new List<CardDefinition>
            {
                Card("dark.mushroom_ring", "Mushroom Ring", T, "Every night: gain 1 Spider.", Cost(), new string[0],
                    Every(EffectTrigger.Night, ResourceType.Spiders, 1)),
                Card("dark.bat_cave", "Bat Cave", T, "Every night: gain 2 Bats.", Cost(ResourceType.Spiders, 1), new string[0],
                    Every(EffectTrigger.Night, ResourceType.Bats, 2)),
                Card("dark.graveyard", "Spooky Graveyard", T, "Every 3 nights: gain 1 Ghoul.", Cost(ResourceType.Bats, 2),
                    new string[0], Every(EffectTrigger.Night, ResourceType.Ghouls, 1, interval: 3)),
                Card("dark.haunted_lantern", "Haunted Lantern", T, "Every night: gain 1 Ghoul.",
                    Cost(ResourceType.Bats, 3, ResourceType.Spiders, 2), new string[0],
                    Every(EffectTrigger.Night, ResourceType.Ghouls, 1)),
                Card("dark.sleepy_web", "Sleepy Web", T, "Every day: gain 1 Spider.", Cost(ResourceType.Bats, 1), new string[0],
                    Every(EffectTrigger.Day, ResourceType.Spiders, 1)),
            };
        }

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
