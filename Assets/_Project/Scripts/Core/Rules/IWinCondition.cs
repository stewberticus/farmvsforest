using System.Linq;

namespace FarmVsForest.Core
{
    /// <summary>Decides who is out and who has won. Called after anything that could change the outcome.</summary>
    public interface IWinCondition
    {
        /// <summary>Should this player be knocked out now?</summary>
        bool IsEliminated(Match match, PlayerState player);
    }

    /// <summary>Default: a player is out when their home card is destroyed. The last alliance with players left wins.</summary>
    public class HomeCardWinCondition : IWinCondition
    {
        public bool IsEliminated(Match match, PlayerState player) =>
            player.HomeCard != null && !player.HomeCard.IsOnBoard;
    }

    internal static class WinConditionHelpers
    {
        public static int[] RemainingAlliances(Match match) =>
            match.Players.Where(p => !p.IsEliminated).Select(p => p.AllianceId).Distinct().ToArray();
    }
}
