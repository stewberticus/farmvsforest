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

    /// <summary>Spend some of one resource to gain another, e.g. "Every night: turn 2 Parts into 1 Toy".</summary>
    public class ConvertResourceEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            var e = ctx.Effect;
            foreach (var p in ctx.TargetPlayers)
            {
                if (p.Resources.Get(e.fromResource) < e.fromAmount + e.fromKeep) continue;
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

namespace FarmVsForest.Core
{
    /// <summary>Puts new copies of a named card into the target players' hands, e.g. a Barn adding a Cow.</summary>
    public class AddCardToHandEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            var e = ctx.Effect;
            var def = ctx.Match.FindCard(e.cardId);
            if (def == null)
                throw new System.Collections.Generic.KeyNotFoundException(
                    ctx.Source + " wants to add card '" + e.cardId + "' to hand, but no card with that id is in the " +
                    "match. Add it to MatchConfig.cardCatalog or to a deck.");
            foreach (var p in ctx.TargetPlayers)
            {
                if (e.skipIfInHand && p.Hand.Exists(c => c.Definition.id == def.id)) continue;
                for (int i = 0; i < e.amount; i++)
                    ctx.Match.AddToHand(p, def, ctx.Source);
            }
        }
    }
}

namespace FarmVsForest.Core
{
    /// <summary>
    /// Custom effect "run_tagged": every card you own with <c>tag</c> runs its day and night effects once, right now.
    /// Factory's Big Whistle uses it with tag "assembler". Use kind Custom with customEffectId "run_tagged".
    /// </summary>
    public class RunTaggedCardsEffect : IEffect
    {
        public const string Id = "run_tagged";

        public void Apply(EffectContext ctx)
        {
            var tag = ctx.Effect.tag;
            var cards = ctx.Match.Board.AllCards()
                .Where(c => c != ctx.Source && ctx.TargetPlayers.Contains(c.Owner) &&
                            (string.IsNullOrEmpty(tag) || c.Definition.HasTag(tag)))
                .OrderBy(c => c.InstanceId)
                .ToList();
            foreach (var card in cards) ctx.Match.RunCardNow(card);
        }
    }
}

namespace FarmVsForest.Core
{
    /// <summary>
    /// Custom effect "start_income": a starting card's daily income. Gains <c>amount</c> x
    /// <see cref="MatchConfig.startingCardIncome"/> of <c>resource</c>, so the sim can switch it off with
    /// --set startingCardIncome=0.
    /// </summary>
    public class StartingIncomeEffect : IEffect
    {
        public const string Id = "start_income";

        public void Apply(EffectContext ctx)
        {
            int amount = ctx.Effect.amount * ctx.Match.Config.startingCardIncome;
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.GainResource(p, ctx.Effect.resource, amount, ctx.Source);
        }

        /// <summary>The effect line for a starting card: "every day (or night): gain 1 resource".</summary>
        public static EffectDefinition Every(EffectTrigger trigger, ResourceType resource) =>
            new EffectDefinition(trigger, EffectKind.Custom, 1, resource) { customEffectId = Id };
    }

    /// <summary>Gain resources for each empty square touching this card, e.g. a Wildflower Meadow that wants open space.</summary>
    public class GainPerEmptyTouchingEffect : IEffect
    {
        public void Apply(EffectContext ctx)
        {
            var board = ctx.Match.Board;
            int count = board.Neighbours(ctx.Source.Position).Count(board.IsEmpty);
            if (count == 0) return;
            foreach (var p in ctx.TargetPlayers)
                ctx.Match.GainResource(p, ctx.Effect.resource, ctx.Effect.amount * count, ctx.Source);
        }
    }
}
