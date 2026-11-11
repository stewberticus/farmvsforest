using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The designed Dark Forest team (see docs/teams/DARK_FOREST.md). Cute-spooky: Bats and Spiders come out at night,
    /// and on your turn you combine them into Ghouls for a party (<see cref="Match.SummonGhoul"/>). Haunts make Ghouls
    /// cheaper. Every Ghoul eats one Bat or Spider each morning (<see cref="GhoulFeeding"/>), so food and Ghouls draw on
    /// the same pile.
    /// </summary>
    public static class DarkForestCards
    {
        private const Team T = Team.DarkForest;

        /// <summary>Tag for the cards that invite Ghouls. Full Moon Shrine counts touching Haunts.</summary>
        public const string Haunt = "haunt";
        /// <summary>Glow Mushrooms grow brighter next to each other.</summary>
        public const string Mushroom = "mushroom";
        /// <summary>Custom effect id: this many Ghouls eat for free this morning.</summary>
        public const string FreeMealsEffect = "dark.free_meals";
        /// <summary>
        /// Passive effect id: while this card touches one of your cards with the effect's tag, Ghouls cost
        /// <c>amount</c> less to summon (<see cref="Match.GhoulCost"/>).
        /// </summary>
        public const string CheaperGhoulsEffect = "dark.cheaper_ghouls";

        /// <summary>How much cheaper the player's switched-on Haunts make their next Ghoul.</summary>
        public static int GhoulDiscount(Match match, PlayerState player)
        {
            var board = match.Board;
            int off = 0;
            foreach (var card in board.AllCards())
            {
                if (card.Owner != player || card.Definition.effects == null) continue;
                foreach (var e in card.Definition.effects)
                {
                    if (e.trigger != EffectTrigger.Passive || e.customEffectId != CheaperGhoulsEffect) continue;
                    if (IsSwitchedOn(match, card, e)) off += e.amount;
                }
            }
            return off;
        }

        /// <summary>A Haunt's discount works while it touches one of its owner's cards with the effect's tag.</summary>
        public static bool IsSwitchedOn(Match match, CardInstance card, EffectDefinition e)
        {
            var board = match.Board;
            return board.Neighbours(card.Position).Select(board.Get)
                .Any(n => n != null && n.Owner == card.Owner && (string.IsNullOrEmpty(e.tag) || n.Definition.HasTag(e.tag)));
        }

        /// <summary>The starting corner card. Makes 1 Bat a night so you can always get going.</summary>
        public static CardDefinition EvilTree => Card("dark.evil_tree", "Evil Tree",
            "Every night: gain 1 Bat. Your home in the dark woods. It looks spooky, but the bats think it's cosy.",
            Cost(), new[] { Tags.Start }, StartingIncomeEffect.Every(EffectTrigger.Night, ResourceType.Bats));

        public static CardDefinition BatRoost => Card("dark.bat_roost", "Bat Roost",
            "Every night: gain 1 Bat.", Cost(), new string[0],
            Every(EffectTrigger.Night, ResourceType.Bats, 1));

        public static CardDefinition CobwebCorner => Card("dark.cobweb_corner", "Cobweb Corner",
            "Every night: gain 1 Spider.", Cost(), new string[0],
            Every(EffectTrigger.Night, ResourceType.Spiders, 1));

        public static CardDefinition GlowMushrooms => Card("dark.glow_mushrooms", "Glow Mushrooms",
            "Every night: gain 1 Bat for each touching Glow Mushrooms. Bats love a night light.", Cost(),
            new[] { Mushroom },
            new EffectDefinition(EffectTrigger.Night, EffectKind.GainPerTouching, 1, ResourceType.Bats) { tag = Mushroom });

        public static CardDefinition SleepyWeb => Card("dark.sleepy_web", "Sleepy Web",
            "Every day: gain 2 Spiders. They spin while you sleep, just in time for breakfast.",
            Cost(ResourceType.Bats, 3), new string[0],
            Every(EffectTrigger.Day, ResourceType.Spiders, 2));

        public static CardDefinition BatBelfry => Card("dark.bat_belfry", "Bat Belfry",
            "Every night: gain 1 Bat for each of your cards touching it.", Cost(ResourceType.Spiders, 5), new string[0],
            new EffectDefinition(EffectTrigger.Night, EffectKind.GainPerTouching, 1, ResourceType.Bats));

        public static CardDefinition PicnicBlanket => Card("dark.picnic_blanket", "Picnic Blanket",
            "Every day: 2 of your Ghouls eat for free.", Cost(ResourceType.Bats, 2), new string[0],
            new EffectDefinition(EffectTrigger.Day, EffectKind.Custom, 2) { customEffectId = FreeMealsEffect });

        // Haunts (2026-11-11): Ghouls are summoned from Bats and Spiders (Match.SummonGhoul); Haunts make that cheaper.

        public static CardDefinition GigglingGraveyard => Card("dark.giggling_graveyard", "Giggling Graveyard",
            "While this touches another Haunt: Ghouls cost 1 less to summon. Ghouls love a crowd.",
            Cost(ResourceType.Bats, 2, ResourceType.Spiders, 2), new[] { Haunt },
            Cheaper(Haunt));

        public static CardDefinition HauntedLantern => Card("dark.haunted_lantern", "Haunted Lantern",
            "While this touches Glow Mushrooms: Ghouls cost 1 less to summon. Ghouls follow the glow.",
            Cost(ResourceType.Bats, 3, ResourceType.Spiders, 2), new[] { Haunt },
            Cheaper(Mushroom));

        public static CardDefinition FullMoonShrine => Card("dark.full_moon_shrine", "Full Moon Shrine",
            "Every 3 nights: if this touches a Graveyard or Lantern, a Ghoul drops by for free.", Cost(ResourceType.Spiders, 4),
            new string[0],
            new EffectDefinition(EffectTrigger.Night, EffectKind.GainPerTouching, 1, ResourceType.Ghouls)
                { tag = Haunt, interval = 3, maxCount = 1 });

        public static CardDefinition FortuneToad => Card("dark.fortune_toad", "Fortune-Telling Toad",
            "When played: look at the top 3 cards of your deck, keep 1 and put the rest on the bottom. Every night: gain 1 Spider. It croaks your fortune for a fly.",
            Cost(ResourceType.Bats, 1), new string[0],
            new EffectDefinition(EffectTrigger.OnPlay, EffectKind.Scout, 3),
            Every(EffectTrigger.Night, ResourceType.Spiders, 1));

        /// <summary>One of each deck card (not the Evil Tree).</summary>
        public static List<CardDefinition> All() => new List<CardDefinition>
        {
            BatRoost, CobwebCorner, GlowMushrooms, SleepyWeb, BatBelfry, PicnicBlanket, GigglingGraveyard,
            HauntedLantern, FullMoonShrine, FortuneToad,
        };

        /// <summary>How many copies of each card go in the 30-card starter deck.</summary>
        public static readonly Dictionary<string, int> Copies = new Dictionary<string, int>
        {
            { "dark.bat_roost", 6 }, { "dark.cobweb_corner", 6 }, { "dark.glow_mushrooms", 4 }, { "dark.sleepy_web", 3 },
            { "dark.bat_belfry", 2 }, { "dark.picnic_blanket", 0 }, { "dark.fortune_toad", 2 }, { "dark.giggling_graveyard", 3 },
            { "dark.haunted_lantern", 2 }, { "dark.full_moon_shrine", 2 },
        };

        public static List<CardDefinition> StarterDeck() =>
            All().SelectMany(c => Enumerable.Repeat(c, Copies.TryGetValue(c.id, out var n) ? n : 0)).ToList();

        /// <summary>Registers Dark Forest's custom effects.</summary>
        public static void RegisterEffects(EffectRegistry registry) => registry.Register(FreeMealsEffect, new FreeMealsEffect());

        private static CardDefinition Card(string id, string name, string text, List<ResourceAmount> cost, string[] tags,
            params EffectDefinition[] effects) =>
            new CardDefinition
            {
                id = id, displayName = name, team = T, rulesText = text, cost = cost,
                tags = tags.ToList(), effects = effects.ToList(),
            };

        private static List<ResourceAmount> Cost() => new List<ResourceAmount>();

        private static List<ResourceAmount> Cost(ResourceType type, int amount) =>
            new List<ResourceAmount> { new ResourceAmount(type, amount) };

        private static List<ResourceAmount> Cost(ResourceType a, int amountA, ResourceType b, int amountB) =>
            new List<ResourceAmount> { new ResourceAmount(a, amountA), new ResourceAmount(b, amountB) };

        private static EffectDefinition Cheaper(string touching) =>
            new EffectDefinition(EffectTrigger.Passive, EffectKind.Custom, 1) { customEffectId = CheaperGhoulsEffect, tag = touching };

        private static EffectDefinition Every(EffectTrigger trigger, ResourceType resource, int amount, int interval = 1) =>
            new EffectDefinition(trigger, EffectKind.GainResource, amount, resource, interval);
    }

    /// <summary>"N of your Ghouls eat for free this morning." Used by Picnic Blanket; fires before Ghouls eat.</summary>
    public class FreeMealsEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var p in ctx.TargetPlayers) p.FreeMeals += ctx.Effect.amount;
        }
    }
}
