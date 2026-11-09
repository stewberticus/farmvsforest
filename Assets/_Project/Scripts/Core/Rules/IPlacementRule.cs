using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>Decides which squares a player may put a card on. Swap this out to try different rules.</summary>
    public interface IPlacementRule
    {
        bool CanPlace(Match match, PlayerState player, CardDefinition card, Coord square);
    }

    /// <summary>
    /// Default rule: the square must be empty and either inside your corner's home zone
    /// or next to one of your own cards (optionally your allies' too). Your territory grows outward from your corner.
    /// </summary>
    public class HomeZoneOrAdjacentPlacement : IPlacementRule
    {
        public bool CanPlace(Match match, PlayerState player, CardDefinition card, Coord square)
        {
            var board = match.Board;
            if (!board.IsEmpty(square)) return false;
            if (match.IsInHomeZone(player, square)) return true;

            return board.Neighbours(square).Any(n =>
            {
                var occupant = board.Get(n);
                if (occupant == null) return false;
                if (occupant.Owner == player) return true;
                return match.Config.canPlaceNextToAllies && occupant.Owner.IsAlliedWith(player);
            });
        }
    }
}
