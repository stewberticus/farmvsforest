namespace FarmVsForest.Core
{
    public enum GameEventType
    {
        MatchStarted,
        DayStarted,
        NightStarted,
        TurnStarted,
        TurnEnded,
        CardDrawn,
        CardBurned,
        CardPlayed,
        ResourcesGained,
        ResourcesSpent,
        GoalReached,
        MatchEnded,
    }

    /// <summary>
    /// Something that happened in the match. The rules raise these so the presentation layer
    /// (animations, sounds, log) can react without the rules knowing about Unity.
    /// </summary>
    public class GameEvent
    {
        public GameEventType Type;
        public PlayerState Player;
        public CardInstance Card;
        public CardInstance Source;
        public int Amount;
        public ResourceType Resource;
        public string Message;

        public override string ToString() => Message ?? Type.ToString();
    }
}
