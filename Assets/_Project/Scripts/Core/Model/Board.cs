using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>
    /// A square grid. Each square holds at most one card, and a card on a square means its owner owns it.
    /// Some squares can be Sunny Glades: shared bonus squares nobody can build on (<see cref="MatchConfig.sunnyGladeBonus"/>).
    /// </summary>
    public class Board
    {
        private readonly CardInstance[,] _cells;
        private readonly HashSet<Coord> _glades = new HashSet<Coord>();
        private int _occupied;

        public int Width { get; }
        public int Height { get; }
        public AdjacencyRule Adjacency { get; }

        public Board(int width, int height, AdjacencyRule adjacency = AdjacencyRule.Orthogonal)
        {
            if (width < 2 || height < 2) throw new ArgumentException("Board must be at least 2x2.");
            Width = width;
            Height = height;
            Adjacency = adjacency;
            _cells = new CardInstance[width, height];
        }

        public int SquareCount => Width * Height;
        /// <summary>Squares a card can go on: every square except the glades.</summary>
        public int BuildableCount => SquareCount - _glades.Count;
        public int OccupiedCount => _occupied;
        public bool IsFull => _occupied >= BuildableCount;

        public bool InBounds(Coord c) => c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height;

        public CardInstance Get(Coord c) => InBounds(c) ? _cells[c.x, c.y] : null;

        /// <summary>No card and not a glade: a card could go here.</summary>
        public bool IsEmpty(Coord c) => InBounds(c) && _cells[c.x, c.y] == null && !_glades.Contains(c);

        /// <summary>
        /// No card on it (an empty square or a glade). Forest's light rule uses this: a Sunny Glade lets the sun in.
        /// </summary>
        public bool IsOpen(Coord c) => InBounds(c) && _cells[c.x, c.y] == null;

        public bool IsGlade(Coord c) => _glades.Contains(c);
        public IReadOnlyCollection<Coord> Glades => _glades;

        /// <summary>True if any square touching <paramref name="c"/> is a glade.</summary>
        public bool TouchesGlade(Coord c)
        {
            if (_glades.Count == 0) return false;
            foreach (var n in Neighbours(c))
                if (_glades.Contains(n)) return true;
            return false;
        }

        internal void AddGlade(Coord c)
        {
            if (!InBounds(c) || _cells[c.x, c.y] != null) throw new InvalidOperationException("Can't put a glade on " + c + ".");
            _glades.Add(c);
        }

        internal void Place(CardInstance card, Coord c)
        {
            if (!IsEmpty(c)) throw new InvalidOperationException("Square " + c + " is not empty.");
            _cells[c.x, c.y] = card;
            card.Position = c;
            _occupied++;
        }

        /// <summary>The squares touching <paramref name="center"/>, using the board's adjacency rule.</summary>
        public IEnumerable<Coord> Neighbours(Coord center)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (Adjacency == AdjacencyRule.Orthogonal && dx != 0 && dy != 0) continue;
                var c = new Coord(center.x + dx, center.y + dy);
                if (InBounds(c)) yield return c;
            }
        }

        public IEnumerable<CardInstance> AllCards()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (_cells[x, y] != null) yield return _cells[x, y];
        }
    }
}
