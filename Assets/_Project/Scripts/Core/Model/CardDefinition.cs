using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>
    /// The static data for a card: what it costs and what it does once it's on the board.
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

        /// <summary>
        /// Free-form tags that effects and goals key off. For example Forest's goal counts cards tagged
        /// <see cref="Tags.Tree"/>, and "gain 1 Egg per touching crop" counts cards tagged "crop".
        /// </summary>
        public List<string> tags = new List<string>();

        public List<EffectDefinition> effects = new List<EffectDefinition>();

        public bool IsFree => cost == null || cost.TrueForAll(c => c.amount <= 0);

        public bool HasTag(string tag) => tags != null && tags.Contains(tag);

        public override string ToString() => displayName ?? id;
    }

    /// <summary>Tags the rules themselves look for.</summary>
    public static class Tags
    {
        /// <summary>Counts towards Forest's goal of a connected forest.</summary>
        public const string Tree = "tree";
        /// <summary>Marks a team's starting corner card.</summary>
        public const string Start = "start";
    }
}
