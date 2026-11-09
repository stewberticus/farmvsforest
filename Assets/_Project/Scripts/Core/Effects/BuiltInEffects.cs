using System.Linq;

namespace FarmVsForest.Core
{
    public class GainResourceEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.GainResource(p, ctx.Effect.resource, ctx.Effect.amount, ctx.Source);
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

    /// <summary>Spend some of one resource to gain another, e.g. "Every night: turn 2 Parts into 1 Goods".</summary>
    public class ConvertResourceEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            var e = ctx.Effect;
            foreach (var p in ctx.TargetPlayers)
            {
                if (p.Resources.Get(e.fromResource) < e.fromAmount) continue;
                ctx.Match.SpendResource(p, e.fromResource, e.fromAmount, ctx.Source);
                ctx.Match.GainResource(p, e.resource, e.amount, ctx.Source);
            }
        }
    }

    /// <summary>Gain resources for each of the owner's cards touching this one, optionally only those with a tag.</summary>
    public class GainPerTouchingEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            var e = ctx.Effect;
            var board = ctx.Match.Board;
            int count = board.Neighbours(ctx.Source.Position)
                .Select(board.Get)
                .Count(c => c != null && c.Owner == ctx.Source.Owner &&
                            (string.IsNullOrEmpty(e.tag) || c.Definition.HasTag(e.tag)));
            if (count == 0) return;
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.GainResource(p, e.resource, e.amount * count, ctx.Source);
        }
    }
}
