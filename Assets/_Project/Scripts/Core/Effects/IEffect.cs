using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>Everything an effect needs to run.</summary>
    public class EffectContext
    {
        public Match Match;
        /// <summary>The card whose effect is firing.</summary>
        public CardInstance Source;
        public EffectDefinition Effect;
        /// <summary>Resolved card targets (for card target types). For player targets this is empty.</summary>
        public List<CardInstance> TargetCards = new List<CardInstance>();
        /// <summary>Resolved player targets. For card target types this is the distinct owners of the target cards.</summary>
        public List<PlayerState> TargetPlayers = new List<PlayerState>();
    }

    /// <summary>
    /// A piece of card behaviour. Implement this and register it with <see cref="EffectRegistry"/>
    /// to add new mechanics without touching the turn or card-play code.
    /// </summary>
    public interface IEffect
    {
        void Apply(EffectContext context);
    }
}
