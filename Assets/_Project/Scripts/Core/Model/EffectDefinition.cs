using System;

namespace FarmVsForest.Core
{
    /// <summary>When an effect fires.</summary>
    public enum EffectTrigger
    {
        /// <summary>Once, right after the card is placed.</summary>
        OnPlay = 0,
        /// <summary>At the start of each of the owner's turns, before they draw.</summary>
        OnOwnerTurnStart = 1,
        /// <summary>At the end of each of the owner's turns.</summary>
        OnOwnerTurnEnd = 2,
        /// <summary>When the card is destroyed and leaves the board.</summary>
        OnDestroyed = 3,
    }

    /// <summary>What the effect does. Add new built-ins here, or use Custom with a registered id.</summary>
    public enum EffectKind
    {
        GainResource = 0,
        Damage = 1,
        Heal = 2,
        DrawCard = 3,
        /// <summary>Looks up <see cref="EffectDefinition.customEffectId"/> in the EffectRegistry.</summary>
        Custom = 100,
    }

    /// <summary>Who or what the effect applies to.</summary>
    public enum EffectTarget
    {
        // Player targets
        Owner = 0,
        OwnerAlliance = 1,
        Opponents = 2,

        // Card targets
        Self = 10,
        AdjacentCards = 11,
        AdjacentAllies = 12,
        AdjacentEnemies = 13,
        AllEnemyCards = 14,
        AllAlliedCards = 15,
    }

    /// <summary>One line of card behaviour, e.g. "At the start of your turn, deal 1 damage to adjacent enemies."</summary>
    [Serializable]
    public class EffectDefinition
    {
        public EffectTrigger trigger = EffectTrigger.OnOwnerTurnStart;
        public EffectKind kind = EffectKind.GainResource;
        public EffectTarget target = EffectTarget.Owner;
        public int amount = 1;
        /// <summary>Used by GainResource.</summary>
        public ResourceType resource;
        /// <summary>How far "adjacent" reaches for Adjacent* targets. 1 means touching squares.</summary>
        public int range = 1;
        /// <summary>Used when kind is Custom.</summary>
        public string customEffectId;

        public EffectDefinition() { }

        public EffectDefinition(EffectTrigger trigger, EffectKind kind, EffectTarget target, int amount,
            ResourceType resource = ResourceType.Food, int range = 1)
        {
            this.trigger = trigger;
            this.kind = kind;
            this.target = target;
            this.amount = amount;
            this.resource = resource;
            this.range = range;
        }

        public static bool IsCardTarget(EffectTarget t) => (int)t >= 10;
    }
}
