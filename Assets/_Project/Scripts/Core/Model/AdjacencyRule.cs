namespace FarmVsForest.Core
{
    /// <summary>Which squares count as "next to" a square.</summary>
    public enum AdjacencyRule
    {
        /// <summary>Up, down, left, right (4 neighbours).</summary>
        Orthogonal = 0,
        /// <summary>All 8 surrounding squares, like a chess king.</summary>
        EightWay = 1,
    }
}
