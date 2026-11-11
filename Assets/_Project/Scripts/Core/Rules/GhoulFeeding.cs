using System;

namespace FarmVsForest.Core
{
    /// <summary>
    /// Dark Forest's upkeep rule. Every morning, after day effects, each Ghoul eats
    /// <see cref="MatchConfig.ghoulFoodEach"/> Bat or Spider (whichever you have more of). Free meals from cards like
    /// Picnic Blanket are used first. If there isn't enough food, the Ghouls eat what there is and
    /// <see cref="MatchConfig.ghoulsLostWhenHungry"/> Ghoul wanders home.
    /// </summary>
    public static class GhoulFeeding
    {
        public static void FeedAll(Match match)
        {
            foreach (var p in match.Players) Feed(match, p);
        }

        public static void Feed(Match match, PlayerState p)
        {
            int ghouls = p.Resources.Get(ResourceType.Ghouls);
            int free = p.FreeMeals;
            p.FreeMeals = 0;
            if (ghouls <= 0) return;

            int needed = Math.Max(0, ghouls - free) * match.Config.ghoulFoodEach;
            // Eat from the bigger pile first, one mouthful at a time, so both piles stay even.
            int bats = p.Resources.Get(ResourceType.Bats), spiders = p.Resources.Get(ResourceType.Spiders);
            int eatBats = 0, eatSpiders = 0;
            while (needed > 0 && bats + spiders > 0)
            {
                if (bats >= spiders) { bats--; eatBats++; }
                else { spiders--; eatSpiders++; }
                needed--;
            }
            match.SpendResource(p, ResourceType.Bats, eatBats);
            match.SpendResource(p, ResourceType.Spiders, eatSpiders);

            if (needed > 0)
            {
                int lost = Math.Min(ghouls, match.Config.ghoulsLostWhenHungry);
                match.SpendResource(p, ResourceType.Ghouls, lost);
                match.RaiseEvent(new GameEvent { Type = GameEventType.GhoulsWentHome, Player = p, Resource = ResourceType.Ghouls,
                    Amount = lost, Message = p + " couldn't feed every Ghoul, so " + lost + " went home hungry" });
            }
        }
    }
}
