namespace FarmVsForest.Core
{
    /// <summary>
    /// Every resource in the game. Each team has its own: Farm makes Wheat, Milk and Eggs; Forest makes Acorns and
    /// Honey; Factory makes Parts and Toys; Dark Forest makes Bats, Spiders and Ghouls.
    /// Values are explicit so card assets keep their meaning if the list is reordered.
    /// </summary>
    public enum ResourceType
    {
        // Farm
        Wheat = 0,
        Milk = 1,
        Eggs = 2,

        // Forest
        Acorns = 10,
        Honey = 11,

        // Factory
        Parts = 20,
        Toys = 21,
        /// <summary>Old name for <see cref="Toys"/>, kept so older code still compiles. Use Toys.</summary>
        [System.Obsolete("Goods was renamed to Toys.")]
        Goods = Toys,

        // Dark Forest
        Bats = 30,
        Spiders = 31,
        Ghouls = 32,
    }
}
