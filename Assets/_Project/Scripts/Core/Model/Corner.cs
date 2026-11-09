namespace FarmVsForest.Core
{
    /// <summary>The four starting corners, listed in clockwise seating order.</summary>
    public enum Corner
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
    }

    public static class CornerExtensions
    {
        public static Coord ToCoord(this Corner corner, int width, int height)
        {
            switch (corner)
            {
                case Corner.BottomLeft: return new Coord(0, 0);
                case Corner.TopLeft: return new Coord(0, height - 1);
                case Corner.TopRight: return new Coord(width - 1, height - 1);
                default: return new Coord(width - 1, 0);
            }
        }
    }
}
