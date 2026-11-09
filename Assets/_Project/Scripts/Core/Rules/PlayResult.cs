namespace FarmVsForest.Core
{
    /// <summary>Why a card can or can't be played. Ok means it was (or can be) played.</summary>
    public enum PlayResult
    {
        Ok = 0,
        GameOver,
        NotYourTurn,
        CardNotInHand,
        NoPlaysLeft,
        CannotAfford,
        InvalidSquare,
    }
}
