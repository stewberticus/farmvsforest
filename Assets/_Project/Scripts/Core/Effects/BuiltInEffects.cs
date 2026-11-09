namespace FarmVsForest.Core
{
    /// <summary>Target players gain resources. With a card target, the owners of those cards gain them.</summary>
    public class GainResourceEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.GainResource(p, ctx.Effect.resource, ctx.Effect.amount, ctx.Source);
        }
    }

    public class DamageEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var c in ctx.TargetCards)
                ctx.Match.DealDamage(c, ctx.Effect.amount, ctx.Source);
        }
    }

    public class HealEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var c in ctx.TargetCards)
                ctx.Match.Heal(c, ctx.Effect.amount, ctx.Source);
        }
    }

    public class DrawCardEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.DrawCards(p, ctx.Effect.amount);
        }
    }
}
