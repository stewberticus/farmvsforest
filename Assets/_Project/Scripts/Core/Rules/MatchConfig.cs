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
        /// <summary>
        /// The card put in this player's corner at the start (Farm, Huge Tree, Headquarters, Evil Tree).
        /// Null means no starting card; the player then starts by placing on their corner square.
        /// </summary>
        public CardDefinition startingCard;

        public PlayerSetup() { }

        public PlayerSetup(string name, Team team, List<CardDefinition> deck, CardDefinition startingCard = null)
        {
            this.name = name;
            this.team = team;
            this.deck = deck;
            this.startingCard = startingCard;
        }
    }

    /// <summary>
    /// Every tunable rule of a match in one place. Decided rules are noted; the rest are starting numbers
    /// to tune in playtests. See docs/DESIGN.md.
    /// </summary>
    [Serializable]
    public class MatchConfig
    {
        public GameMode mode = GameMode.OneVsOne;

        /// <summary>Board size for 2-player games (square). Starting point, to tune.</summary>
        public int boardSize = 12;
        /// <summary>Board size for 4-player games (square). Starting point, to tune.</summary>
        public int fourPlayerBoardSize = 16;
        /// <summary>Decided: cards must touch edge to edge, no diagonals.</summary>
        public AdjacencyRule adjacency = AdjacencyRule.Orthogonal;
        /// <summary>If true, you can also place touching your 2v2 partner's cards, not just your own.</summary>
        public bool canPlaceNextToAllies = false;

        public int startingHandSize = 4;
        public int maxHandSize = 8;
        public int drawsPerTurn = 1;
        /// <summary>Decided: one card per turn.</summary>
        public int cardsPerTurn = 1;
        /// <summary>The very first turn of the match skips its draw, to soften first-player advantage.</summary>
        public bool firstPlayerSkipsDraw = true;

        public List<ResourceAmount> startingResources = new List<ResourceAmount>();

        // ---- Team goals (starting numbers, to tune)

        /// <summary>Farm wins with this many Wheat, Milk and Eggs at once.</summary>
        public int farmGoalEach = 20;
        /// <summary>Forest wins with this many Tree cards in one touching group.</summary>
        public int forestGoalTrees = 30;
        /// <summary>Factory wins by making this many Goods in one round.</summary>
        public int factoryGoalGoods = 10;
        /// <summary>Dark Forest wins with this many Ghouls at once.</summary>
        public int darkForestGoalGhouls = 13;
        /// <summary>Decided: every morning each Ghoul eats this many Bats or Spiders.</summary>
        public int ghoulFoodEach = 1;
        /// <summary>Decided: a morning when you can't feed all your Ghouls, this many wander home.</summary>
        public int ghoulsLostWhenHungry = 1;

        /// <summary>
        /// Safety net: after this many rounds the match ends and whoever owns the most squares wins. 0 means no limit,
        /// so the match only ends on a goal or a full board.
        /// </summary>
        public int roundLimit = 0;

        /// <summary>Seed for shuffling. Same seed and same actions give the same match (useful for replays and netcode).</summary>
        public int seed = 12345;

        /// <summary>
        /// Cards that effects can create by id (AddCardToHand) even if they aren't in any deck. Cards in the players'
        /// decks and starting cards are always findable, so only list extras here.
        /// </summary>
        /// <remarks>Not serialized: in Unity, MatchRunner fills this.</remarks>
        [NonSerialized] public List<CardDefinition> cardCatalog = new List<CardDefinition>();

        /// <summary>One entry per seat, in seat order. Must match the mode's player count.</summary>
        /// <remarks>Not serialized: in Unity, MatchRunner fills this from DeckAssets.</remarks>
        [NonSerialized] public List<PlayerSetup> players = new List<PlayerSetup>();

        public int BoardSizeFor(int playerCount) => playerCount > 2 ? fourPlayerBoardSize : boardSize;
    }
}
