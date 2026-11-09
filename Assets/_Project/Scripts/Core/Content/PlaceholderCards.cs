using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// Stand-in cards so the systems can be played and tested before real content exists.
    /// Every name, number and effect here is a placeholder; replace them with designed cards
    /// (as CardDefinitionAssets in Unity) whenever you're ready.
    /// </summary>
    public static class PlaceholderCards
    {
        public static CardDefinition Home() => new CardDefinition
        {
            id = "neutral.home", displayName = "Home", health = 10, tags = { "home" },
            rulesText = "Your corner. If it's destroyed, you're out.",
        };

        public static List<CardDefinition> ForTeam(Team team)
        {
            switch (team)
            {
                case Team.Farm: return Farm();
                case Team.Forest: return Forest();
                case Team.Factory: return Factory();
                default: return DarkForest();
            }
        }

        public static IEnumerable<CardDefinition> All() =>
            new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest }.SelectMany(ForTeam);

        /// <summary>A 20-card deck: 8 copies of the team's free producer and 3 of each other card.</summary>
        public static List<CardDefinition> StarterDeck(Team team)
        {
            var cards = ForTeam(team);
            var deck = new List<CardDefinition>();
            foreach (var c in cards)
            {
                int copies = c.IsFree ? 8 : 3;
                for (int i = 0; i < copies; i++) deck.Add(c);
            }
            return deck;
        }

        // ---------------------------------------------------------------- helpers

        private static CardDefinition Card(string id, string name, Team team, int health, string text,
            ResourceType res, int cost, params EffectDefinition[] effects)
        {
            var def = new CardDefinition
            {
                id = id, displayName = name, team = team, health = health, rulesText = text,
                effects = effects.ToList(),
            };
            if (cost > 0) def.cost.Add(new ResourceAmount(res, cost));
            return def;
        }

        private static EffectDefinition Produce(ResourceType res, int amount) =>
            new EffectDefinition(EffectTrigger.OnOwnerTurnStart, EffectKind.GainResource, EffectTarget.Owner, amount, res);

        private static EffectDefinition Fx(EffectTrigger trigger, EffectKind kind, EffectTarget target, int amount, int range = 1) =>
            new EffectDefinition(trigger, kind, target, amount, ResourceType.Food, range);

        // ---------------------------------------------------------------- teams

        private static List<CardDefinition> Farm()
        {
            const ResourceType R = ResourceType.Food;
            const Team T = Team.Farm;
            return new List<CardDefinition>
            {
                Card("farm.wheat_field", "Wheat Field", T, 2, "Start of your turn: gain 1 Food.", R, 0, Produce(R, 1)),
                Card("farm.barn", "Barn", T, 4, "When played: draw a card.", R, 2,
                    Fx(EffectTrigger.OnPlay, EffectKind.DrawCard, EffectTarget.Owner, 1)),
                Card("farm.scarecrow", "Scarecrow", T, 3, "End of your turn: 1 damage to adjacent enemies.", R, 1,
                    Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1)),
                Card("farm.farmhouse", "Farmhouse", T, 5, "Start of your turn: heal adjacent allies by 1.", R, 3,
                    Fx(EffectTrigger.OnOwnerTurnStart, EffectKind.Heal, EffectTarget.AdjacentAllies, 1)),
                Card("farm.tractor", "Tractor", T, 4, "When played: 2 damage to adjacent enemies.", R, 4,
                    Fx(EffectTrigger.OnPlay, EffectKind.Damage, EffectTarget.AdjacentEnemies, 2)),
            };
        }

        private static List<CardDefinition> Forest()
        {
            const ResourceType R = ResourceType.Wood;
            const Team T = Team.Forest;
            return new List<CardDefinition>
            {
                Card("forest.sapling", "Sapling", T, 2, "Start of your turn: gain 1 Wood.", R, 0, Produce(R, 1)),
                Card("forest.old_oak", "Old Oak", T, 6, "Start of your turn: gain 1 Wood.", R, 2, Produce(R, 1)),
                Card("forest.thorn_bush", "Thorn Bush", T, 3, "End of your turn: 1 damage to adjacent enemies.", R, 1,
                    Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1)),
                Card("forest.druid_grove", "Druid Grove", T, 4, "Start of your turn: heal adjacent allies by 2.", R, 3,
                    Fx(EffectTrigger.OnOwnerTurnStart, EffectKind.Heal, EffectTarget.AdjacentAllies, 2)),
                Card("forest.wolf_den", "Wolf Den", T, 4, "When played: 2 damage to adjacent enemies.", R, 3,
                    Fx(EffectTrigger.OnPlay, EffectKind.Damage, EffectTarget.AdjacentEnemies, 2)),
            };
        }

        private static List<CardDefinition> Factory()
        {
            const ResourceType R = ResourceType.Scrap;
            const Team T = Team.Factory;
            return new List<CardDefinition>
            {
                Card("factory.scrap_heap", "Scrap Heap", T, 2, "Start of your turn: gain 1 Scrap.", R, 0, Produce(R, 1)),
                Card("factory.assembly_line", "Assembly Line", T, 3,
                    "Start of your turn: gain 2 Scrap. End of your turn: take 1 damage.", R, 2,
                    Produce(R, 2), Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.Self, 1)),
                Card("factory.smokestack", "Smokestack", T, 4, "Start of your turn: 1 damage to ALL adjacent cards.", R, 2,
                    Fx(EffectTrigger.OnOwnerTurnStart, EffectKind.Damage, EffectTarget.AdjacentCards, 1)),
                Card("factory.turret", "Turret", T, 3, "End of your turn: 1 damage to enemies within 2 squares.", R, 3,
                    Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1, range: 2)),
                Card("factory.generator", "Generator", T, 3, "When played: draw a card.", R, 1,
                    Fx(EffectTrigger.OnPlay, EffectKind.DrawCard, EffectTarget.Owner, 1)),
            };
        }

        private static List<CardDefinition> DarkForest()
        {
            const ResourceType R = ResourceType.Shadow;
            const Team T = Team.DarkForest;
            return new List<CardDefinition>
            {
                Card("dark.mushroom_ring", "Mushroom Ring", T, 2, "Start of your turn: gain 1 Shadow.", R, 0, Produce(R, 1)),
                Card("dark.hollow_tree", "Hollow Tree", T, 3, "When destroyed: 2 damage to adjacent enemies.", R, 1,
                    Fx(EffectTrigger.OnDestroyed, EffectKind.Damage, EffectTarget.AdjacentEnemies, 2)),
                Card("dark.wisp", "Wisp", T, 2, "Start of your turn: 1 damage to adjacent enemies.", R, 2,
                    Fx(EffectTrigger.OnOwnerTurnStart, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1)),
                Card("dark.nightshade", "Nightshade", T, 3, "When played: draw 2 cards.", R, 3,
                    Fx(EffectTrigger.OnPlay, EffectKind.DrawCard, EffectTarget.Owner, 2)),
                Card("dark.wraith_tree", "Wraith Tree", T, 5, "End of your turn: 1 damage to every enemy card.", R, 4,
                    Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AllEnemyCards, 1)),
            };
        }

        /// <summary>A ready-to-play config with placeholder decks, handy for tests and the prototype scene.</summary>
        public static MatchConfig DefaultConfig(GameMode mode, int seed = 12345)
        {
            var teams = new[] { Team.Farm, Team.Forest, Team.Factory, Team.DarkForest };
            var config = new MatchConfig { mode = mode, seed = seed, homeCard = Home() };
            for (int i = 0; i < GameModes.PlayerCount(mode); i++)
                config.players.Add(new PlayerSetup("Player " + (i + 1), teams[i], StarterDeck(teams[i])));
            return config;
        }
    }
}
