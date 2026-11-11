using System.Collections.Generic;
using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    /// <summary>Sunny Glades: shared squares nobody builds on; a touching card makes 1 extra once a round. Phase 2.</summary>
    public class SunnyGladeTests
    {
        private static readonly CardDefinition Field = Card("field", 0, null, Gain(EffectTrigger.Day, ResourceType.Wheat));

        /// <summary>6x6 board, one glade at (2,2), both players Farm-ish day teams with no starting cards.</summary>
        private static Match WithGlade(int bonus = 1)
        {
            var config = SimpleConfig(Card("free"));
            config.sunnyGladeBonus = bonus;
            config.sunnyGladeSquares = new List<Coord> { new Coord(2, 2) };
            return Started(config);
        }

        [Test]
        public void DefaultLayout_IsFourSymmetricSquaresNearTheMiddle()
        {
            var glades = MatchConfig.DefaultGlades(8);
            CollectionAssert.AreEquivalent(new[] { new Coord(3, 3), new Coord(4, 4), new Coord(2, 5), new Coord(5, 2) }, glades);
            foreach (var g in glades) CollectionAssert.Contains(glades, new Coord(7 - g.x, 7 - g.y));
            var nine = MatchConfig.DefaultGlades(9);
            CollectionAssert.AreEquivalent(new[] { new Coord(3, 3), new Coord(5, 5), new Coord(2, 6), new Coord(6, 2) }, nine);
            foreach (var g in nine) CollectionAssert.Contains(nine, new Coord(8 - g.x, 8 - g.y));
        }

        [Test]
        public void DefaultMatch_Is9x9_WithFourGlades()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.OneVsOne));
            Assert.AreEqual(9, m.Board.Width);
            Assert.AreEqual(4, m.Board.Glades.Count);
            Assert.AreEqual(77, m.Board.BuildableCount);
        }

        [Test]
        public void FourPlayerBoard_HasNoGlades()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.FreeForAll));
            Assert.AreEqual(0, m.Board.Glades.Count);
        }

        [Test]
        public void BonusZero_TurnsGladesOff()
        {
            var m = WithGlade(bonus: 0);
            Assert.AreEqual(0, m.Board.Glades.Count);
        }

        [Test]
        public void NobodyCanBuildOnAGlade()
        {
            var m = WithGlade();
            var p = m.Players[0];
            Put(m, 0, Card("a"), 2, 1);
            Assert.IsFalse(m.CanPlaceOn(p, Card("b"), new Coord(2, 2)));
            Assert.IsNull(m.Summon(Card("b"), p, new Coord(2, 2)));
            Assert.IsTrue(m.CanPlaceOn(p, Card("b"), new Coord(1, 1)));
        }

        [Test]
        public void CardTouchingAGlade_MakesOneExtra_OnceARound()
        {
            var m = WithGlade();
            var p = m.Players[0];
            var maker = new CardDefinition { id = "maker", displayName = "maker" };
            maker.effects.Add(Gain(EffectTrigger.Day, ResourceType.Wheat));
            maker.effects.Add(Gain(EffectTrigger.Night, ResourceType.Wheat));
            Put(m, 0, maker, 2, 1);   // touches the glade
            Put(m, 0, Field, 0, 5);   // doesn't
            PassRound(m);             // now just after day 2's effects, which used day 2's bonus
            int before = p.Resources.Get(ResourceType.Wheat);
            PassRound(m);
            // Night 2: maker 1, the round's bonus is used. Day 3: maker 1 + bonus 1, field 1.
            Assert.AreEqual(before + 4, p.Resources.Get(ResourceType.Wheat));
        }

        [Test]
        public void GladeLetsTheSunIn_ForForestTrees()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm });
            config.sunnyGladeSquares = new List<Coord> { new Coord(1, 0) };
            config.sunnyGladeBonus = 1;
            var m = Started(config);
            var f = m.Players[0];
            // Sapling in the corner, its only other neighbour filled: the glade is its only light.
            var sapling = Put(m, 0, ForestCards.Sapling, 0, 0);
            Put(m, 0, Card("block"), 0, 1);
            PassRound(m);
            Assert.AreEqual(1, sapling.GrowthProgress);
            Assert.IsFalse(m.Board.IsEmpty(new Coord(1, 0)));
            Assert.IsTrue(m.Board.IsOpen(new Coord(1, 0)));
        }

        [Test]
        public void WildflowerMeadow_DoesNotCountAGladeAsEmpty()
        {
            var config = SimpleConfig(Card("free"), teams: new[] { Team.Forest, Team.Farm });
            config.sunnyGladeSquares = new List<Coord> { new Coord(1, 0) };
            config.sunnyGladeBonus = 1;
            var m = Started(config);
            var f = m.Players[0];
            Put(m, 0, ForestCards.WildflowerMeadow, 0, 0);
            Put(m, 0, Card("block"), 0, 1);
            PassRound(m);
            // No empty squares touching: 0 Honey, so there's nothing for the glade to add to either.
            Assert.AreEqual(0, f.Resources.Get(ResourceType.Honey));
        }

        [Test]
        public void BoardIsFull_WhenEverySquareButTheGladesHasACard()
        {
            var config = SimpleConfig(Card("free"), boardSize: 3);
            config.sunnyGladeSquares = new List<Coord> { new Coord(1, 1) };
            config.sunnyGladeBonus = 1;
            var m = Started(config);
            Assert.AreEqual(8, m.Board.BuildableCount);
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                if (x != 1 || y != 1) Put(m, 0, Card("a"), x, y);
            Assert.IsTrue(m.Board.IsFull);
        }
    }
}
