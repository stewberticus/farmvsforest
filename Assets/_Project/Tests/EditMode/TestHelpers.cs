using System.Collections.Generic;
using FarmVsForest.Core;

namespace FarmVsForest.Tests
{
    internal static class TestHelpers
    {
        public static CardDefinition Card(string id, int health = 2, int foodCost = 0, params EffectDefinition[] effects)
        {
            var def = new CardDefinition { id = id, displayName = id, health = health };
            if (foodCost > 0) def.cost.Add(new ResourceAmount(ResourceType.Food, foodCost));
            def.effects.AddRange(effects);
            return def;
        }

        public static List<CardDefinition> Copies(CardDefinition def, int n)
        {
            var list = new List<CardDefinition>();
            for (int i = 0; i < n; i++) list.Add(def);
            return list;
        }

        /// <summary>1v1 on a small board, no home cards, both players with the same simple deck.</summary>
        public static MatchConfig SimpleConfig(CardDefinition deckCard, int deckSize = 10, GameMode mode = GameMode.OneVsOne)
        {
            var config = new MatchConfig
            {
                mode = mode, boardWidth = 6, boardHeight = 6, startingHandSize = 3, firstPlayerSkipsDraw = false,
            };
            for (int i = 0; i < GameModes.PlayerCount(mode); i++)
                config.players.Add(new PlayerSetup("P" + i, (Team)i, Copies(deckCard, deckSize)));
            return config;
        }

        public static Match Started(MatchConfig config)
        {
            var m = new Match(config);
            m.Start();
            return m;
        }
    }
}
