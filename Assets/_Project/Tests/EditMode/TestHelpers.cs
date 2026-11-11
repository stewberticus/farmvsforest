using System.Collections.Generic;
using FarmVsForest.Core;

namespace FarmVsForest.Tests
{
    internal static class TestHelpers
    {
        public static CardDefinition Card(string id, int wheatCost = 0, string[] tags = null, params EffectDefinition[] effects)
        {
            var def = new CardDefinition { id = id, displayName = id };
            if (wheatCost > 0) def.cost.Add(new ResourceAmount(ResourceType.Wheat, wheatCost));
            if (tags != null) def.tags.AddRange(tags);
            def.effects.AddRange(effects);
            return def;
        }

        public static EffectDefinition Gain(EffectTrigger trigger, ResourceType res, int amount = 1, int interval = 1,
            bool repeat = true) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, res, interval, repeat);

        public static List<CardDefinition> Copies(CardDefinition def, int n)
        {
            var list = new List<CardDefinition>();
            for (int i = 0; i < n; i++) list.Add(def);
            return list;
        }

        /// <summary>
        /// A small board, both players with the same simple deck. No starting cards unless <paramref name="start"/> is
        /// given, so each player begins by placing on their own corner. Teams default to Farm then Forest (both day teams).
        /// </summary>
        public static MatchConfig SimpleConfig(CardDefinition deckCard, int deckSize = 10, GameMode mode = GameMode.OneVsOne,
            Team[] teams = null, CardDefinition start = null, int boardSize = 6)
        {
            var config = new MatchConfig
            {
                mode = mode, boardSize = boardSize, fourPlayerBoardSize = boardSize, startingHandSize = 3,
                firstPlayerSkipsDraw = false, everyoneSkipsFirstDraw = false, sunnyGladeBonus = 0,
            };
            teams = teams ?? new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest };
            for (int i = 0; i < GameModes.PlayerCount(mode); i++)
                config.players.Add(new PlayerSetup("P" + i, teams[i % teams.Length], Copies(deckCard, deckSize), start));
            return config;
        }

        public static Match Started(MatchConfig config)
        {
            var m = new Match(config);
            m.Start();
            return m;
        }

        public static CardInstance Put(Match m, int playerId, CardDefinition def, int x, int y) =>
            m.Summon(def, m.Players[playerId], new Coord(x, y));

        /// <summary>Ends turns until a full round (day and night) has passed.</summary>
        public static void PassRound(Match m)
        {
            int round = m.Round;
            int safety = 0;
            while (!m.IsOver && m.Round == round && safety++ < 20) m.EndTurn(m.CurrentPlayer.Id);
        }
    }
}
