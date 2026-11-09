namespace FarmVsForest.Core
{
    /// <summary>A specific copy of a card in a match: in a deck, in hand, or on the board.</summary>
    public class CardInstance
    {
        public int InstanceId { get; }
        public CardDefinition Definition { get; }
        public PlayerState Owner { get; }
        public Coord Position { get; internal set; }
        public bool IsOnBoard { get; internal set; }

        /// <summary>The turn number on which the card was placed.</summary>
        public int PlayedOnTurn { get; internal set; }

        /// <summary>How many days have started since this card was placed. Drives "every X days" effects.</summary>
        public int DaysSeen { get; internal set; }
        /// <summary>How many nights have started since this card was placed. Drives "every X nights" effects.</summary>
        public int NightsSeen { get; internal set; }

        internal CardInstance(int instanceId, CardDefinition definition, PlayerState owner)
        {
            InstanceId = instanceId;
            Definition = definition;
            Owner = owner;
        }

        public override string ToString() => Definition + "#" + InstanceId;
    }
}
