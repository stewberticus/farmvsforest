using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>A player's stockpile of resources.</summary>
    public class ResourcePool
    {
        private readonly Dictionary<ResourceType, int> _amounts = new Dictionary<ResourceType, int>();

        public int Get(ResourceType type) => _amounts.TryGetValue(type, out var v) ? v : 0;

        public void Add(ResourceType type, int amount)
        {
            _amounts[type] = Math.Max(0, Get(type) + amount);
        }

        public void Add(IEnumerable<ResourceAmount> amounts)
        {
            if (amounts == null) return;
            foreach (var a in amounts) Add(a.type, a.amount);
        }

        /// <summary>True if every cost line can be paid. Duplicate lines of the same type are summed.</summary>
        public bool CanAfford(IEnumerable<ResourceAmount> cost)
        {
            if (cost == null) return true;
            return cost
                .GroupBy(c => c.type)
                .All(g => Get(g.Key) >= g.Sum(c => c.amount));
        }

        /// <summary>Spends the cost. Returns false and spends nothing if it can't be afforded.</summary>
        public bool TrySpend(IEnumerable<ResourceAmount> cost)
        {
            if (cost == null) return true;
            var list = cost.ToList();
            if (!CanAfford(list)) return false;
            foreach (var c in list) Add(c.type, -c.amount);
            return true;
        }

        public IReadOnlyDictionary<ResourceType, int> Snapshot() => new Dictionary<ResourceType, int>(_amounts);

        public override string ToString() =>
            string.Join(", ", _amounts.Where(kv => kv.Value > 0).Select(kv => kv.Value + " " + kv.Key));
    }
}
