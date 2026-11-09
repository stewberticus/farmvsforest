using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>Seeded randomness so matches are reproducible.</summary>
    public class GameRandom
    {
        private readonly Random _random;

        public GameRandom(int seed) => _random = new Random(seed);

        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
