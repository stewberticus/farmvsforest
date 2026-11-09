using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>A rectangular grid of squares. Each square holds at most one card.</summary>
    public class Board
    {
        private readonly CardInstance[,] _cells;

        public int Width { get; }
        public int Height { get; }
        public AdjacencyRule Adjacency { get; }

        public Board(int width, int height, AdjacencyRule adjacency = AdjacencyRule.EightWay)
        {
            if (width < 2 || height < 2) throw new ArgumentException("Board must be at least 2x2.");
            Width = width;
            Height = height;
            Adjacency = adjacency;
            _cells = new CardInstance[width, height];
        }

        public bool InBounds(Coord c) => c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height;

        public CardInstance Get(Coord c) => InBounds(c) ? _cells[c.x, c.y] : null;

        public bool IsEmpty(Coord c) => InBounds(c) && _cells[c.x, c.y] == null;

        internal void Place(CardInstance card, Coord c)
        {
            if (!IsEmpty(c)) throw new InvalidOperationException("Square " + c + " is not empty.");
            _cells[c.x, c.y] = card;
            card.Position = c;
        }

        internal void Remove(CardInstance card)
        {
            if (Get(card.Position) == card) _cells[card.Position.x, card.Position.y] = null;
        }

        /// <summary>Squares within <paramref name="range"/> steps of <paramref name="center"/> (not including it).</summary>
        public IEnumerable<Coord> Neighbours(Coord center, int range = 1)
        {
            for (int dx = -range; dx <= range; dx++)
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var c = new Coord(center.x + dx, center.y + dy);
                if (!InBounds(c)) continue;
                int dist = Adjacency == AdjacencyRule.EightWay ? center.ChebyshevDistance(c) : center.ManhattanDistance(c);
                if (dist <= range) yield return c;
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
