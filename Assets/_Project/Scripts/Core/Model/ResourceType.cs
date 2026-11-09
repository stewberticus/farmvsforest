namespace FarmVsForest.Core
{
    /// <summary>
    /// Every resource in the game. Each team has its own: Farm makes Wheat, Milk and Eggs; Forest makes Acorns and
    /// Honey; Factory makes Parts and Goods; Dark Forest makes Bats, Spiders and Ghouls.
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
        Goods = 21,

        // Dark Forest
        Bats = 30,
        Spiders = 31,
        Ghouls = 32,
    }
}
