using System.Collections.Generic;

namespace FarmVsForest.Core
{
    public class PlayerState
    {
        public int Id { get; }
        public string Name { get; }
        public Team Team { get; }
        public Corner Corner { get; }

        /// <summary>Players with the same alliance id are on the same side (2v2). In 1v1 and free-for-all everyone has their own.</summary>
        public int AllianceId { get; }

        /// <summary>Top of the deck is the end of the list.</summary>
        public List<CardInstance> Deck { get; } = new List<CardInstance>();
        public List<CardInstance> Hand { get; } = new List<CardInstance>();
        public List<CardInstance> Discard { get; } = new List<CardInstance>();
        public ResourcePool Resources { get; } = new ResourcePool();

        /// <summary>
        /// Everything this player has gained since the current round's day began. Factory's goal
        /// ("make 10 Toys in one round") reads this.
        /// </summary>
        public ResourcePool GainedThisRound { get; } = new ResourcePool();

        /// <summary>The card placed in this player's corner at the start, if any.</summary>
        public CardInstance StartingCard { get; internal set; }

        public int CardsPlayedThisTurn { get; internal set; }
        /// <summary>Forest's Compost rule: used once a turn.</summary>
        public bool CompostedThisTurn { get; internal set; }

        /// <summary>Dark Forest: Ghouls that eat for free this morning (Picnic Blanket). Reset after Ghouls eat.</summary>
        public int FreeMeals { get; internal set; }

        internal PlayerState(int id, string name, Team team, Corner corner, int allianceId)
        {
            Id = id;
            Name = name;
            Team = team;
            Corner = corner;
            AllianceId = allianceId;
        }

        /// <summary>Farm, Forest and Factory play during the day; Dark Forest plays at night.</summary>
        public TimeOfDay PlaysDuring => TeamRules.PlaysDuring(Team);

        public bool IsAlliedWith(PlayerState other) => other != null && other.AllianceId == AllianceId;

        public override string ToString() => Name + " (" + Team + ")";
    }
}
