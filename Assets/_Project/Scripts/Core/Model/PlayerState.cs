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

        public int CardsPlayedThisTurn { get; internal set; }
        public bool IsEliminated { get; internal set; }

        /// <summary>The card that must survive for this player to stay in the game, if the match uses one.</summary>
        public CardInstance HomeCard { get; internal set; }

        internal PlayerState(int id, string name, Team team, Corner corner, int allianceId)
        {
            Id = id;
            Name = name;
            Team = team;
            Corner = corner;
            AllianceId = allianceId;
        }

        public bool IsAlliedWith(PlayerState other) => other != null && other.AllianceId == AllianceId;

        public override string ToString() => Name + " (" + Team + ")";
    }
}
