using System;

namespace FarmVsForest.Core
{
    /// <summary>An amount of one resource, e.g. "2 Wood". Serializable so Unity can show it in the inspector.</summary>
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType type;
        public int amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }

        public override string ToString() => amount + " " + type;
    }
}
