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

        /// <summary>
        /// Board size for 2-player games (square). 2026-11-11: down from 10x10, where the teams played the same as on a
        /// board where they never touch. Decided 9x9 (Alexander). 8x8 was the plan; 9x9 tunes better because on 8x8 Forest fills
        /// the board and wins on most squares. See balance-reports/2026-11-11-phase2-3-board-and-ghouls.md.
        /// </summary>
        public int boardSize = 9;

        /// <summary>
        /// Sunny Glades (decided 2026-11-11): shared squares near the middle that nobody can build on. A card touching
        /// one makes this many extra of whatever it makes, once a round. 0 means no glades at all.
        /// </summary>
        public int sunnyGladeBonus = 1;
        /// <summary>
        /// Where the glades go. Null uses the default layout for 2-player boards (<see cref="DefaultGlades"/>); an empty
        /// list means no glades. 4-player boards get none unless listed here.
        /// </summary>
        [NonSerialized] public List<Coord> sunnyGladeSquares;
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
        /// <summary>
        /// Old rule: only the very first turn of the match skips its draw. It made going first weaker (seat 1 won
        /// 42% of Farm vs Forest) and always hit a day team, never Dark Forest. Ignored while
        /// <see cref="everyoneSkipsFirstDraw"/> is on.
        /// </summary>
        public bool firstPlayerSkipsDraw = true;
        /// <summary>
        /// Proposed rule (2026-11-09): nobody draws on their own first turn; the opening hand is your first draw.
        /// Every player gets the same number of cards by their Nth turn, so neither seat is a card behind.
        /// </summary>
        public bool everyoneSkipsFirstDraw = true;

        /// <summary>
        /// Old safety net from when starting cards made nothing: if true, the opening hand always has at least one free
        /// card (one is swapped in from the deck). Off since starting cards make a resource every day
        /// (<see cref="startingCardIncome"/>), which already means nobody gets stuck.
        /// </summary>
        public bool openingHandHasFreeCard = false;

        /// <summary>
        /// Starting cards work (2026-11-11): each team's corner card makes this many of its basic resource every day
        /// (Dark Forest's Evil Tree every night). Farm 1 Wheat, Huge Tree 1 Acorn, Headquarters 1 Part, Evil Tree 1 Bat.
        /// 0 turns it off.
        /// </summary>
        public int startingCardIncome = 1;

        /// <summary>
        /// Mulligan (decided 2026-11-11): this many times, at the start of your first turn before you do anything, you may
        /// shuffle your opening hand into your deck and draw the same number. 0 turns it off.
        /// </summary>
        public int mulligans = 1;

        public List<ResourceAmount> startingResources = new List<ResourceAmount>();

        // ---- Team goals (starting numbers, to tune)

        /// <summary>Farm wins with this many Wheat, Milk and Eggs at once.</summary>
        public int farmGoalEach = 66;
        /// <summary>Forest wins with this many Tree cards in one touching group.</summary>
        public int forestGoalTrees = 33;
        /// <summary>
        /// Forest's spreading rule: after their normal play, a Forest player can play extra Tree cards that turn by
        /// paying this many Acorns on top of each card's cost. 0 turns the rule off.
        /// </summary>
        public int forestExtraTreeAcorns = 2;
        /// <summary>
        /// Forest's growth goal: this many Old Trees in one connected forest. 0 uses the older goal of
        /// <see cref="forestGoalTrees"/> trees instead.
        /// </summary>
        public int forestGoalOldTrees = 22;
        /// <summary>Each further extra tree in the same turn costs this many more Acorns (2, then 3, then 4...).</summary>
        public int forestExtraTreeStep = 1;
        /// <summary>
        /// Forest's Compost rule: once a turn, a Forest player can discard a card from their hand to gain this many
        /// Acorns. 0 turns the rule off.
        /// </summary>
        public int forestCompostAcorns = 1;
        /// <summary>
        /// Forest's Pollinate rule: on your turn, spend this much Honey to make one of your growing trees grow one step
        /// right away (it doesn't need sunlight). As often as you can pay. 0 turns the rule off.
        /// </summary>
        public int forestPollinateHoney = 2;

        /// <summary>Factory wins by making this many Toys in one round (day + night).</summary>
        public int factoryGoalToys = 17;
        /// <summary>Dark Forest wins with this many Ghouls at once.</summary>
        public int darkForestGoalGhouls = 13;
        /// <summary>
        /// Dark Forest's Summon rule (decided 2026-11-11): on your turn, combine this many Bats and
        /// <see cref="darkForestGhoulSpiders"/> Spiders into 1 Ghoul, as often as you can pay. Haunts make it cheaper.
        /// Both 0 turns the rule off. The plan started at 3 and 3; the sim needed 6 and 6 plus
        /// <see cref="darkForestGhoulsPerPriceStep"/> to keep games 20+ rounds (see balance-reports, phase 3).
        /// </summary>
        public int darkForestGhoulBats = 6;
        public int darkForestGhoulSpiders = 6;
        /// <summary>Each further Ghoul in the same turn costs this many more Bats and Spiders. Off: the party price below does the job.</summary>
        public int darkForestGhoulStep = 0;
        /// <summary>
        /// At most this many Ghouls summoned per turn (0 means no limit). Without it, saving up and summoning the whole
        /// party on the last turn is the best play, and the Ghouls never have to be fed.
        /// </summary>
        public int darkForestGhoulsPerTurn = 0;
        /// <summary>
        /// A bigger party is harder to impress: for every this many Ghouls you already have, the next one costs 1 more
        /// Bat and 1 more Spider. 0 turns it off.
        /// </summary>
        public int darkForestGhoulsPerPriceStep = 1;
        /// <summary>Haunts can't bring the Bats or the Spiders part of the price below this.</summary>
        public int darkForestGhoulMinEach = 2;

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

        /// <summary>The glade squares for this match, or an empty list.</summary>
        public List<Coord> GladesFor(int playerCount, int size)
        {
            if (sunnyGladeBonus <= 0) return new List<Coord>();
            if (sunnyGladeSquares != null) return new List<Coord>(sunnyGladeSquares);
            return playerCount > 2 ? new List<Coord>() : DefaultGlades(size);
        }

        /// <summary>
        /// Four glades near the middle of a 1v1 board (players in opposite corners, bottom-left and top-right). Two sit on
        /// the line between the players, one a step closer to each, and two sit level with both players, on the other
        /// diagonal. On 8x8: (3,3), (4,4), (2,5) and (5,2). Symmetric, so neither seat is favoured.
        /// </summary>
        public static List<Coord> DefaultGlades(int size)
        {
            if (size < 6) return new List<Coord>();
            int c = size / 2;
            return new List<Coord>
            {
                new Coord(c - 1, c - 1), new Coord(size - c, size - c),
                new Coord(c - 2, size - c + 1), new Coord(size - c + 1, c - 2),
            };
        }
    }
}
