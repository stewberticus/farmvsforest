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

        /// <summary>
        /// Set for upgrade cards: the id or tag of the cards it can be played onto (one of your own, not yet upgraded).
        /// Empty for normal cards. See <see cref="MatchConfig.allowUpgrades"/>.
        /// </summary>
        public string upgradeFor;

        public bool IsUpgrade => !string.IsNullOrEmpty(upgradeFor);

        /// <summary>Can this upgrade go onto a card with definition <paramref name="target"/>?</summary>
        public bool CanUpgrade(CardDefinition target) =>
            IsUpgrade && target != null && (target.id == upgradeFor || target.HasTag(upgradeFor));

        /// <summary>
        /// The card an upgraded card becomes: the base card's id, tags and effects plus the upgrade's effects, named
        /// after the upgrade (a Cow with a Big Red Barn reads "Cow + Big Red Barn").
        /// </summary>
        public static CardDefinition Upgraded(CardDefinition baseCard, CardDefinition upgrade)
        {
            var tags = new List<string>(baseCard.tags ?? new List<string>());
            var effects = new List<EffectDefinition>(baseCard.effects ?? new List<EffectDefinition>());
            effects.AddRange(upgrade.effects ?? new List<EffectDefinition>());
            return new CardDefinition
            {
                id = baseCard.id, displayName = baseCard.displayName + " + " + upgrade.displayName, team = baseCard.team,
                rulesText = baseCard.rulesText + " Upgraded with " + upgrade.displayName + ": " + upgrade.rulesText,
                cost = baseCard.cost, tags = tags, effects = effects,
            };
        }

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
        /// <summary>A fully grown tree. Forest's growth goal counts these.</summary>
        public const string Old = "old";
        /// <summary>Factory machines that turn Parts into Toys. Wrapping Station and Big Whistle care about them.</summary>
        public const string Assembler = "assembler";
    }
}
