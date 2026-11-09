namespace FarmVsForest.Core
{
    /// <summary>A specific copy of a card in a match: in a deck, in hand, on the board or in the discard.</summary>
    public class CardInstance
    {
        public int InstanceId { get; }
        public CardDefinition Definition { get; }
        public PlayerState Owner { get; }
        public Coord Position { get; internal set; }
        public bool IsOnBoard { get; internal set; }
        public int Health { get; internal set; }
        public int MaxHealth { get; internal set; }

        /// <summary>The turn number on which the card was placed.</summary>
        public int PlayedOnTurn { get; internal set; }

        /// <summary>Cards with 0 or less max health can't be damaged.</summary>
        public bool IsIndestructible => MaxHealth <= 0;

        internal CardInstance(int instanceId, CardDefinition definition, PlayerState owner)
        {
            InstanceId = instanceId;
            Definition = definition;
            Owner = owner;
            MaxHealth = definition.health;
            Health = definition.health;
        }

        public override string ToString() => Definition + "#" + InstanceId;
    }
}
