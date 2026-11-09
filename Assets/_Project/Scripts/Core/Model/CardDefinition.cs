using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The static data for a card: what it costs, how tough it is and what it does.
    /// Pure data so it can be authored in Unity (via CardDefinitionAsset), loaded from JSON, or built in code.
    /// </summary>
    [Serializable]
    public class CardDefinition
    {
        /// <summary>Unique, stable id such as "farm.wheat_field". Used for deck lists and saves.</summary>
        public string id;
        public string displayName;
        public Team team;
        public string rulesText;

        /// <summary>Resources spent to play the card. Empty means the card is free.</summary>
        public List<ResourceAmount> cost = new List<ResourceAmount>();

        /// <summary>How much damage the card takes before it is destroyed. 0 or less means it can't be damaged.</summary>
        public int health = 1;

        /// <summary>Free-form tags ("building", "animal", "home") for effects and filters to key off.</summary>
        public List<string> tags = new List<string>();

        public List<EffectDefinition> effects = new List<EffectDefinition>();

        public bool IsFree => cost == null || cost.TrueForAll(c => c.amount <= 0);

        public bool HasTag(string tag) => tags != null && tags.Contains(tag);

        public override string ToString() => displayName ?? id;
    }
}
