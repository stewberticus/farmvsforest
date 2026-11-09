using System;

namespace FarmVsForest.Core
{
    /// <summary>A square on the board. (0,0) is the bottom-left corner.</summary>
    [Serializable]
    public struct Coord : IEquatable<Coord>
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>Chess "king distance": how many king moves apart two squares are.</summary>
        public int ChebyshevDistance(Coord other) => Math.Max(Math.Abs(x - other.x), Math.Abs(y - other.y));

        public int ManhattanDistance(Coord other) => Math.Abs(x - other.x) + Math.Abs(y - other.y);

        public bool Equals(Coord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Coord c && Equals(c);
        public override int GetHashCode() => (x * 397) ^ y;
        public static bool operator ==(Coord a, Coord b) => a.Equals(b);
        public static bool operator !=(Coord a, Coord b) => !a.Equals(b);
        public override string ToString() => "(" + x + "," + y + ")";
    }
}
