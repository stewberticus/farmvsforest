using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class CardPlayTests
    {
        [Test]
        public void WithNoCardsOnBoard_YouStartOnYourCorner()
        {
            var m = Started(SimpleConfig(Card("free")));
            var p0 = m.Players[0];
            var squares = m.LegalSquares(p0, p0.Hand[0].Definition).ToList();
            CollectionAssert.AreEqual(new[] { new Coord(0, 0) }, squares);
        }

        [Test]
        public void OnlyOneCardPerTurn()
        {
            var m = Started(SimpleConfig(Card("free")));
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, 0, new Coord(0, 0)));
            Assert.AreEqual(PlayResult.NoPlaysLeft, m.PlayCard(0, 0, new Coord(1, 0)));
        }

        [Test]
        public void CannotPlayOnSomeoneElsesTurn()
        {
            var m = Started(SimpleConfig(Card("free")));
            Assert.AreEqual(PlayResult.NotYourTurn, m.PlayCard(1, 0, new Coord(5, 5)));
        }

        [Test]
        public void CannotAfford_WithoutResources_AndSpendsWhenPlayed()
        {
            var m = Started(SimpleConfig(Card("pricey", wheatCost: 2)));
            Assert.AreEqual(PlayResult.CannotAfford, m.PlayCard(0, 0, new Coord(0, 0)));

            m.Players[0].Resources.Add(ResourceType.Wheat, 3);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, 0, new Coord(0, 0)));
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Wheat));
        }

        [Test]
        public void NewCards_MustTouchYourOwnCard_EdgeToEdge()
        {
            var m = Started(SimpleConfig(Card("free")));
            var p0 = m.Players[0];
            var def = p0.Hand[0].Definition;
            Put(m, 0, def, 2, 2);

            Assert.IsTrue(m.CanPlaceOn(p0, def, new Coord(3, 2)), "right");
            Assert.IsTrue(m.CanPlaceOn(p0, def, new Coord(2, 1)), "below");
            Assert.IsFalse(m.CanPlaceOn(p0, def, new Coord(3, 3)), "diagonal doesn't count");
            Assert.IsFalse(m.CanPlaceOn(p0, def, new Coord(4, 2)), "two squares away");
            Assert.IsFalse(m.CanPlaceOn(p0, def, new Coord(2, 2)), "occupied");
            Assert.IsFalse(m.CanPlaceOn(m.Players[1], def, new Coord(3, 2)), "opponent can't build off my card");
        }

        [Test]
        public void StartingCards_ArePlacedInCorners_AndYouBuildFromThem()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.OneVsOne));
            var p0 = m.Players[0];
            Assert.AreEqual("farm.farm", m.Board.Get(new Coord(0, 0)).Definition.id);
            Assert.AreEqual("forest.huge_tree", m.Board.Get(new Coord(9, 9)).Definition.id);

            var squares = m.LegalSquares(p0, p0.Hand[0].Definition).ToList();
            CollectionAssert.AreEquivalent(new[] { new Coord(1, 0), new Coord(0, 1) }, squares);
        }

        [Test]
        public void OwningSquares_IsHavingACardOnThem()
        {
            var m = Started(SimpleConfig(Card("free")));
            Put(m, 0, Card("a"), 0, 0);
            Put(m, 0, Card("a"), 1, 0);
            Put(m, 1, Card("a"), 5, 5);
            Assert.AreEqual(2, m.SquaresOwnedBy(m.Players[0]));
            Assert.AreEqual(1, m.SquaresOwnedBy(m.Players[1]));
        }
    }
}
