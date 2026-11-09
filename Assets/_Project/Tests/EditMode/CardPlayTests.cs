using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class CardPlayTests
    {
        [Test]
        public void FreeCard_CanBePlayedInHomeZone()
        {
            var m = Started(SimpleConfig(Card("free")));
            var card = m.Players[0].Hand[0];
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, card, new Coord(1, 1)));
            Assert.AreSame(card, m.Board.Get(new Coord(1, 1)));
            Assert.IsFalse(m.Players[0].Hand.Contains(card));
        }

        [Test]
        public void OnlyOneCardPerTurn()
        {
            var m = Started(SimpleConfig(Card("free")));
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, 0, new Coord(0, 0)));
            Assert.AreEqual(PlayResult.NoPlaysLeft, m.PlayCard(0, 0, new Coord(1, 0)));
        }

        [Test]
        public void CannotPlayOnOpponentsTurn()
        {
            var m = Started(SimpleConfig(Card("free")));
            Assert.AreEqual(PlayResult.NotYourTurn, m.PlayCard(1, 0, new Coord(5, 5)));
        }

        [Test]
        public void CannotAfford_WithoutResources_AndSpendsWhenPlayed()
        {
            var m = Started(SimpleConfig(Card("pricey", foodCost: 2)));
            Assert.AreEqual(PlayResult.CannotAfford, m.PlayCard(0, 0, new Coord(0, 0)));

            m.Players[0].Resources.Add(ResourceType.Food, 3);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, 0, new Coord(0, 0)));
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Food));
        }

        [Test]
        public void Placement_HomeZoneOrNextToOwnCards()
        {
            var m = Started(SimpleConfig(Card("free")));
            var p0 = m.Players[0];
            var def = p0.Hand[0].Definition;

            Assert.IsTrue(m.CanPlaceOn(p0, def, new Coord(2, 2)), "edge of the 3x3 home zone");
            Assert.IsFalse(m.CanPlaceOn(p0, def, new Coord(3, 3)), "outside home zone, nothing adjacent");

            m.PlayCard(0, 0, new Coord(2, 2));
            Assert.IsTrue(m.CanPlaceOn(p0, def, new Coord(3, 3)), "now adjacent to own card");
            Assert.IsFalse(m.CanPlaceOn(p0, def, new Coord(2, 2)), "occupied");
            Assert.IsFalse(m.CanPlaceOn(m.Players[1], def, new Coord(3, 2)), "opponent can't build off my card");
        }

        [Test]
        public void OrthogonalAdjacency_ExcludesDiagonals()
        {
            var config = SimpleConfig(Card("free"));
            config.adjacency = AdjacencyRule.Orthogonal;
            var m = Started(config);
            m.PlayCard(0, 0, new Coord(2, 2));
            var def = m.Players[0].Hand[0].Definition;
            Assert.IsTrue(m.CanPlaceOn(m.Players[0], def, new Coord(3, 2)));
            Assert.IsFalse(m.CanPlaceOn(m.Players[0], def, new Coord(3, 3)));
        }

        [Test]
        public void LegalSquares_AtStart_IsTheHomeZone()
        {
            var m = Started(SimpleConfig(Card("free")));
            var squares = m.LegalSquares(m.Players[0], m.Players[0].Hand[0].Definition).ToList();
            Assert.AreEqual(9, squares.Count);
            Assert.IsTrue(squares.All(s => s.x <= 2 && s.y <= 2));
        }
    }
}
