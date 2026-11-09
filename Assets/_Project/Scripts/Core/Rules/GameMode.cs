using System.Collections.Generic;

namespace FarmVsForest.Core
{
    public enum GameMode
    {
        OneVsOne = 0,
        TwoVsTwo = 1,
        FreeForAll = 2,
    }

    /// <summary>A seat at the table: which corner it starts in and which alliance it belongs to.</summary>
    public struct Seat
    {
        public Corner Corner;
        public int AllianceId;

        public Seat(Corner corner, int allianceId)
        {
            Corner = corner;
            AllianceId = allianceId;
        }
    }

    public static class GameModes
    {
        public static int PlayerCount(GameMode mode) => mode == GameMode.OneVsOne ? 2 : 4;

        /// <summary>
        /// Seats in turn order. 1v1 uses opposite corners. 2v2 puts partners on the same side of the board
        /// (left vs right) and alternates turns between the sides. Free-for-all goes clockwise.
        /// </summary>
        public static IReadOnlyList<Seat> Seats(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.OneVsOne:
                    return new[] { new Seat(Corner.BottomLeft, 0), new Seat(Corner.TopRight, 1) };
                case GameMode.TwoVsTwo:
                    return new[]
                    {
                        new Seat(Corner.BottomLeft, 0),
                        new Seat(Corner.TopRight, 1),
                        new Seat(Corner.TopLeft, 0),
                        new Seat(Corner.BottomRight, 1),
                    };
                default:
                    return new[]
                    {
                        new Seat(Corner.BottomLeft, 0),
                        new Seat(Corner.TopLeft, 1),
                        new Seat(Corner.TopRight, 2),
                        new Seat(Corner.BottomRight, 3),
                    };
            }
        }
    }
}
