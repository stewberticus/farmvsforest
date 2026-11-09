using System;
using System.Collections.Generic;

namespace FarmVsForest.Core
{
    [Serializable]
    public class PlayerSetup
    {
        public string name;
        public Team team;
        /// <summary>The cards in this player's deck, one entry per copy.</summary>
        public List<CardDefinition> deck = new List<CardDefinition>();

        public PlayerSetup() { }

        public PlayerSetup(string name, Team team, List<CardDefinition> deck)
        {
            this.name = name;
            this.team = team;
            this.deck = deck;
        }
    }

    /// <summary>Every tunable rule of a match in one place. Defaults are first guesses, not design decisions.</summary>
    [Serializable]
    public class MatchConfig
    {
        public GameMode mode = GameMode.OneVsOne;

        public int boardWidth = 8;
        public int boardHeight = 8;
        public AdjacencyRule adjacency = AdjacencyRule.EightWay;

        public int startingHandSize = 4;
        public int maxHandSize = 8;
        public int drawsPerTurn = 1;
        public int cardsPerTurn = 1;
        /// <summary>The very first turn of the match skips its draw, to soften first-player advantage.</summary>
        public bool firstPlayerSkipsDraw = true;

        public List<ResourceAmount> startingResources = new List<ResourceAmount>();

        /// <summary>Size of the square "home zone" in each player's corner where they may always place cards.</summary>
        public int homeZoneSize = 3;
        /// <summary>If true, you can also place next to your allies' cards, not just your own.</summary>
        public bool canPlaceNextToAllies = false;

        /// <summary>
        /// Optional card put in each player's corner at the start. If set, losing it eliminates the player
        /// (like a chess king). If null, nobody is eliminated and the match ends at the turn limit.
        /// Not serialized: in Unity, MatchRunner fills this from a CardDefinitionAsset.
        /// </summary>
        [NonSerialized] public CardDefinition homeCard;

        /// <summary>Number of full rounds before the match ends in a draw. 0 means no limit.</summary>
        public int roundLimit = 0;

        /// <summary>Seed for shuffling. Same seed and same actions give the same match (useful for replays and netcode).</summary>
        public int seed = 12345;

        /// <summary>One entry per seat, in turn order. Must match the mode's player count.</summary>
        /// <remarks>Not serialized: in Unity, MatchRunner fills this from DeckAssets.</remarks>
        [NonSerialized] public List<PlayerSetup> players = new List<PlayerSetup>();
    }
}
