using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>A way for one team to win. Each team has its own.</summary>
    public interface ITeamGoal
    {
        bool IsMet(Match match, PlayerState player);
        /// <summary>Short progress text for the UI, e.g. "Wheat 12/20, Milk 3/20, Eggs 0/20".</summary>
        string Progress(Match match, PlayerState player);
    }

    /// <summary>Per-team rules: when each team plays and how each team wins.</summary>
    public static class TeamRules
    {
        /// <summary>Farm, Forest and Factory play during the day; Dark Forest plays at night.</summary>
        public static TimeOfDay PlaysDuring(Team team) => team == Team.DarkForest ? TimeOfDay.Night : TimeOfDay.Day;

        public static IReadOnlyList<ResourceType> Resources(Team team)
        {
            switch (team)
            {
                case Team.Farm: return new[] { ResourceType.Wheat, ResourceType.Milk, ResourceType.Eggs };
                case Team.Forest: return new[] { ResourceType.Acorns, ResourceType.Honey };
                case Team.Factory: return new[] { ResourceType.Parts, ResourceType.Goods };
                default: return new[] { ResourceType.Bats, ResourceType.Spiders, ResourceType.Ghouls };
            }
        }

        /// <summary>
        /// The extra cost to play <paramref name="card"/> after the player has used their normal plays this turn, or
        /// null if they can't. Forest: any Tree card, for <see cref="MatchConfig.forestExtraTreeAcorns"/> Acorns.
        /// </summary>
        public static List<ResourceAmount> ExtraPlayCost(Team team, CardDefinition card, MatchConfig config)
        {
            if (team == Team.Forest && config.forestExtraTreeAcorns > 0 && card.HasTag(Tags.Tree))
                return new List<ResourceAmount> { new ResourceAmount(ResourceType.Acorns, config.forestExtraTreeAcorns) };
            return null;
        }

        /// <summary>The goal for a team, with numbers taken from the match config.</summary>
        public static ITeamGoal DefaultGoal(Team team, MatchConfig config)
        {
            switch (team)
            {
                case Team.Farm:
                    return new HaveResourcesGoal(config.farmGoalEach, ResourceType.Wheat, ResourceType.Milk, ResourceType.Eggs);
                case Team.Forest:
                    return new ConnectedTaggedCardsGoal(Tags.Tree, config.forestGoalTrees, "Trees");
                case Team.Factory:
                    return new GainInOneRoundGoal(ResourceType.Goods, config.factoryGoalGoods);
                default:
                    return new HaveResourcesGoal(config.darkForestGoalGhouls, ResourceType.Ghouls);
            }
        }
    }

    /// <summary>Hold at least N of each listed resource at once. Farm: 20 Wheat, Milk and Eggs. Dark Forest: 13 Ghouls.</summary>
    public class HaveResourcesGoal : ITeamGoal
    {
        public int Amount { get; }
        public IReadOnlyList<ResourceType> Types { get; }

        public HaveResourcesGoal(int amount, params ResourceType[] types)
        {
            Amount = amount;
            Types = types;
        }

        public bool IsMet(Match match, PlayerState player) => Types.All(t => player.Resources.Get(t) >= Amount);

        public string Progress(Match match, PlayerState player) =>
            string.Join(", ", Types.Select(t => t + " " + player.Resources.Get(t) + "/" + Amount));
    }

    /// <summary>Have N of your cards with a tag in one touching group. Forest: 30 Tree cards all touching.</summary>
    public class ConnectedTaggedCardsGoal : ITeamGoal
    {
        public string Tag { get; }
        public int Amount { get; }
        public string Label { get; }

        public ConnectedTaggedCardsGoal(string tag, int amount, string label)
        {
            Tag = tag;
            Amount = amount;
            Label = label;
        }

        public bool IsMet(Match match, PlayerState player) => LargestGroup(match, player) >= Amount;

        public string Progress(Match match, PlayerState player) =>
            "Largest group of " + Label + " " + LargestGroup(match, player) + "/" + Amount;

        public int LargestGroup(Match match, PlayerState player)
        {
            var board = match.Board;
            var seen = new HashSet<Coord>();
            int best = 0;
            foreach (var start in board.AllCards())
            {
                if (!Counts(start, player) || seen.Contains(start.Position)) continue;
                int size = 0;
                var stack = new Stack<Coord>();
                stack.Push(start.Position);
                seen.Add(start.Position);
                while (stack.Count > 0)
                {
                    var c = stack.Pop();
                    size++;
                    foreach (var n in board.Neighbours(c))
                    {
                        if (seen.Contains(n) || !Counts(board.Get(n), player)) continue;
                        seen.Add(n);
                        stack.Push(n);
                    }
                }
                if (size > best) best = size;
            }
            return best;
        }

        private bool Counts(CardInstance card, PlayerState player) =>
            card != null && card.Owner == player && card.Definition.HasTag(Tag);
    }

    /// <summary>
    /// Gain N of a resource within one round (from the start of a day to the start of the next).
    /// Factory: make 10 Goods in a single turn.
    /// </summary>
    public class GainInOneRoundGoal : ITeamGoal
    {
        public ResourceType Type { get; }
        public int Amount { get; }

        public GainInOneRoundGoal(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public bool IsMet(Match match, PlayerState player) => player.GainedThisRound.Get(Type) >= Amount;

        public string Progress(Match match, PlayerState player) =>
            Type + " made this round " + player.GainedThisRound.Get(Type) + "/" + Amount;
    }
}
