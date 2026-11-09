using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>Decides which squares a player may put a card on. Swap this out to try different rules.</summary>
    public interface IPlacementRule
    {
        bool CanPlace(Match match, PlayerState player, CardDefinition card, Coord square);
    }

    /// <summary>
    /// The game's rule: the square must be empty and touch one of your own cards (edge to edge, using the board's
    /// adjacency). Your territory grows outward from your starting corner card. A player with nothing on the board
    /// (only possible when starting cards are turned off) may place on their corner square.
    /// </summary>
    public class TouchOwnCardPlacement : IPlacementRule
    {
        public bool CanPlace(Match match, PlayerState player, CardDefinition card, Coord square)
        {
            var board = match.Board;
            if (!board.IsEmpty(square)) return false;

            bool touches = board.Neighbours(square).Any(n =>
            {
                var occupant = board.Get(n);
                if (occupant == null) return false;
                if (occupant.Owner == player) return true;
                return match.Config.canPlaceNextToAllies && occupant.Owner.IsAlliedWith(player);
            });
            if (touches) return true;

            return square == player.Corner.ToCoord(board.Width, board.Height) &&
                   !board.AllCards().Any(c => c.Owner == player);
        }
    }
}
