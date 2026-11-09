using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    /// <summary>Maps effect kinds (and custom ids) to the code that runs them.</summary>
    public class EffectRegistry
    {
        private readonly Dictionary<EffectKind, IEffect> _builtIn = new Dictionary<EffectKind, IEffect>();
        private readonly Dictionary<string, IEffect> _custom = new Dictionary<string, IEffect>(StringComparer.Ordinal);

        public static EffectRegistry CreateDefault()
        {
            var r = new EffectRegistry();
            r.Register(EffectKind.GainResource, new GainResourceEffect());
            r.Register(EffectKind.DrawCard, new DrawCardEffect());
            r.Register(EffectKind.ConvertResource, new ConvertResourceEffect());
            r.Register(EffectKind.GainPerTouching, new GainPerTouchingEffect());
            r.Register(EffectKind.AddCardToHand, new AddCardToHandEffect());
            DarkForestCards.RegisterEffects(r);
            r.Register(RunTaggedCardsEffect.Id, new RunTaggedCardsEffect());
            return r;
        }

        public void Register(EffectKind kind, IEffect effect) => _builtIn[kind] = effect;

        /// <summary>Register a custom effect, used by EffectDefinitions with kind Custom and this id.</summary>
        public void Register(string customId, IEffect effect) => _custom[customId] = effect;

        public IEffect Resolve(EffectDefinition def)
        {
            if (def.kind == EffectKind.Custom)
            {
                if (def.customEffectId != null && _custom.TryGetValue(def.customEffectId, out var c)) return c;
                throw new KeyNotFoundException("No custom effect registered with id '" + def.customEffectId + "'.");
            }
            if (_builtIn.TryGetValue(def.kind, out var e)) return e;
            throw new KeyNotFoundException("No effect registered for kind " + def.kind + ".");
        }
    }
}
