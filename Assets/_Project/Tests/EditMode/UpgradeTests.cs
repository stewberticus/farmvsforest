using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class UpgradeTests
    {
        private static CardDefinition F(string id) => FarmCards.All().Single(c => c.id == id);

        /// <summary>Farm (seat 0) vs Forest. Player 0 has a Cow at (1,1) touching two Wheat Fields, and 10 Milk.</summary>
        private static (Match m, CardInstance cow) FarmWithCow(bool allowUpgrades = true)
        {
            var config = SimpleConfig(Card("x"), teams: new[] { Team.Farm, Team.Forest }, boardSize: 8);
            config.allowUpgrades = allowUpgrades;
            config.maxHandSize = 30;
            var m = Started(config);
            var cow = Put(m, 0, F(FarmCards.CowId), 1, 1);
            Put(m, 0, F(FarmCards.WheatFieldId), 0, 1);
            Put(m, 0, F(FarmCards.WheatFieldId), 1, 0);
            m.Players[0].Resources.Add(ResourceType.Milk, 10);
            return (m, cow);
        }

        [Test]
        public void BigRedBarn_GoesOntoYourCow_AndDoublesHerMilk()
        {
            var (m, cow) = FarmWithCow();
            var farm = m.Players[0];
            var barn = m.AddToHand(farm, F(FarmCards.BigRedBarnId));
            CollectionAssert.AreEqual(new[] { new Coord(1, 1) }, m.LegalSquares(farm, barn.Definition).ToArray(), "only onto the Cow");
            int squares = m.SquaresOwnedBy(farm);
            int daysSeen = cow.DaysSeen;

            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, barn, new Coord(1, 1)));
            Assert.AreEqual(7, farm.Resources.Get(ResourceType.Milk), "paid 3 Milk");
            Assert.AreSame(cow, m.Board.Get(new Coord(1, 1)), "the Cow stays where she is");
            Assert.AreEqual("Cow + Big Red Barn", cow.Definition.displayName);
            Assert.IsTrue(cow.Definition.HasTag(FarmCards.Animal), "still an animal");
            Assert.AreEqual(FarmCards.BigRedBarnId, cow.UpgradedWith.id);
            Assert.AreEqual(squares, m.SquaresOwnedBy(farm), "an upgrade takes no new square");
            Assert.AreEqual(daysSeen, cow.DaysSeen, "her timer carries on");
            Assert.IsFalse(farm.Hand.Contains(barn));

            // Two crops touching: 2 Milk every 2 days from the Cow, 2 more from the Barn.
            int before = farm.Resources.Get(ResourceType.Milk);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(before + 4, farm.Resources.Get(ResourceType.Milk));
        }

        [Test]
        public void Upgrades_NeedTheRightCard_OfYourOwn_NotUpgradedYet()
        {
            var (m, cow) = FarmWithCow();
            var farm = m.Players[0];
            var coop = Put(m, 0, F(FarmCards.ChickenCoopId), 2, 1);
            Put(m, 1, F(FarmCards.CowId), 5, 5);
            var barn = F(FarmCards.BigRedBarnId);
            Assert.IsFalse(m.CanPlaceOn(farm, barn, coop.Position), "a Barn is for Cows");
            Assert.IsFalse(m.CanPlaceOn(farm, barn, new Coord(5, 5)), "not someone else's Cow");
            Assert.IsFalse(m.CanPlaceOn(farm, barn, new Coord(2, 2)), "not an empty square");
            Assert.IsTrue(m.CanPlaceOn(farm, F(FarmCards.NestingBoxId), coop.Position));

            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, m.AddToHand(farm, barn), cow.Position));
            Assert.IsFalse(m.CanPlaceOn(farm, barn, cow.Position), "one upgrade per card");
        }

        [Test]
        public void Upgrades_SwitchedOff_CantBePlayed()
        {
            var (m, _) = FarmWithCow(allowUpgrades: false);
            Assert.IsEmpty(m.LegalSquares(m.Players[0], F(FarmCards.BigRedBarnId)).ToList());
        }

        [Test]
        public void GreedyBot_PutsTheBarnOnItsCow()
        {
            var (m, cow) = FarmWithCow();
            var farm = m.Players[0];
            farm.Hand.Clear();
            m.AddToHand(farm, F(FarmCards.BigRedBarnId));
            new GreedyBot(1).PlayTurn(m);
            Assert.IsNotNull(cow.UpgradedWith);
        }

        [Test]
        public void FarmDeck_StillHas30Cards_AndTheUpgradesAreIn()
        {
            Assert.AreEqual(30, FarmCards.StarterDeck().Count);
            Assert.IsTrue(FarmCards.StarterDeck().Count(c => c.IsUpgrade) >= 4);
        }
    }
}
