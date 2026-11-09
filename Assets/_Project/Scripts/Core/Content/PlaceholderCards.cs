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
                    return FactoryCards.Headquarters();
                default:
                    return DarkForestCards.EvilTree;
            }
        }

        // ---------------------------------------------------------------- deck cards

        public static List<CardDefinition> ForTeam(Team team)
        {
            switch (team)
            {
                case Team.Farm: return FarmCards.All();
                case Team.Forest: return ForestCards.All();
                case Team.Factory: return FactoryCards.All();
                default: return DarkForest();
            }
        }

        public static IEnumerable<CardDefinition> All() =>
            new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest }.SelectMany(ForTeam);

        /// <summary>A team's starting deck. Designed teams use their own list; placeholder teams get 40 cards: 12 copies of the team's free card and 7 of each other card.</summary>
        public static List<CardDefinition> StarterDeck(Team team)
        {
            if (team == Team.Farm) return FarmCards.StarterDeck();
            if (team == Team.DarkForest) return DarkForestCards.StarterDeck();
            if (team == Team.Forest) return ForestCards.Deck();
            if (team == Team.Factory) return FactoryCards.Deck();
            var deck = new List<CardDefinition>();
            foreach (var c in ForTeam(team))
            {
                int copies = c.IsFree ? 12 : 7;
                for (int i = 0; i < copies; i++) deck.Add(c);
            }
            return deck;
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
