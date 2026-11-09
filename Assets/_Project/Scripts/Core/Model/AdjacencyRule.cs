namespace FarmVsForest.Core
{
    /// <summary>Which squares count as "touching" a square.</summary>
    public enum AdjacencyRule
    {
        /// <summary>Up, down, left, right: edge to edge only. This is the game's rule.</summary>
        Orthogonal = 0,
        /// <summary>All 8 surrounding squares, diagonals included. Kept for experiments.</summary>
        EightWay = 1,
    }
}
