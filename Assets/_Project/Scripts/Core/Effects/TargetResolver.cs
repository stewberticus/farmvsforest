using System.Collections.Generic;
using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>Turns an EffectTarget into concrete cards and players.</summary>
    public static class TargetResolver
    {
        public static void Resolve(Match match, CardInstance source, EffectDefinition effect,
            List<CardInstance> cards, List<PlayerState> players)
        {
            var owner = source.Owner;
            switch (effect.target)
            {
                case EffectTarget.Owner:
                    players.Add(owner);
                    return;
                case EffectTarget.OwnerAlliance:
                    players.AddRange(match.Players.Where(p => !p.IsEliminated && p.IsAlliedWith(owner)));
                    return;
                case EffectTarget.Opponents:
                    players.AddRange(match.Players.Where(p => !p.IsEliminated && !p.IsAlliedWith(owner)));
                    return;
                case EffectTarget.Self:
                    if (source.IsOnBoard) cards.Add(source);
                    break;
                case EffectTarget.AdjacentCards:
                case EffectTarget.AdjacentAllies:
                case EffectTarget.AdjacentEnemies:
                    // Uses the card's last position, so "when destroyed, damage adjacent" still works.
                    foreach (var sq in match.Board.Neighbours(source.Position, effect.range < 1 ? 1 : effect.range))
                    {
                        var c = match.Board.Get(sq);
                        if (c == null) continue;
                        bool allied = c.Owner.IsAlliedWith(owner);
                        if (effect.target == EffectTarget.AdjacentAllies && !allied) continue;
                        if (effect.target == EffectTarget.AdjacentEnemies && allied) continue;
                        cards.Add(c);
                    }
                    break;
                case EffectTarget.AllEnemyCards:
                    cards.AddRange(match.Board.AllCards().Where(c => !c.Owner.IsAlliedWith(owner)));
                    break;
                case EffectTarget.AllAlliedCards:
                    cards.AddRange(match.Board.AllCards().Where(c => c.Owner.IsAlliedWith(owner)));
                    break;
            }
            players.AddRange(cards.Select(c => c.Owner).Distinct());
        }
    }
}
