using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class FactoryCardTests
    {
        private static CardDefinition F(string id) => FactoryCards.All().Single(c => c.id == id);

        /// <summary>Player 0 is Factory. Nothing in hand matters; cards are put straight on the board.</summary>
        private static Match FactoryMatch(int goal = 99)
        {
            var config = SimpleConfig(Card("x"), teams: new[] { Team.Factory, Team.Farm }, boardSize: 8);
            config.cardCatalog = FactoryCards.All();
            config.maxHandSize = 30;
            config.factoryGoalToys = goal;
            return Started(config);
        }

        private static PlayResult Whistle(Match m, int x, int y)
        {
            var card = m.AddToHand(m.Players[0], F(FactoryCards.BigWhistleId));
            return m.PlayCard(0, card, new Coord(x, y));
        }

        private static int Get(Match m, ResourceType r) => m.Players[0].Resources.Get(r);

        [Test]
        public void Deck_Has30Cards_AndEveryCardIsUsed()
        {
            Assert.AreEqual(30, FactoryCards.Deck().Count);
            CollectionAssert.AreEquivalent(FactoryCards.All().Select(c => c.id), FactoryCards.DeckCounts.Keys);
        }

        [Test]
        public void Headquarters_DoesNothing()
        {
            Assert.IsEmpty(FactoryCards.Headquarters().effects);
            Assert.IsTrue(FactoryCards.Headquarters().IsFree);
        }

        [Test]
        public void TinkerShed_GivesOneMorePart_WhenTouchingAScrapPile()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.TinkerShedId), 0, 0);
            PassRound(m);
            Assert.AreEqual(1, Get(m, ResourceType.Parts));

            Put(m, 0, F(FactoryCards.ScrapPileId), 1, 0);
            PassRound(m);
            Assert.AreEqual(1 + 2 + 1, Get(m, ResourceType.Parts), "shed 2 (touching scrap) + scrap 1");
        }

        [Test]
        public void WrappingStation_GivesAToyPerTouchingAssembler_AtNight()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.WrappingStationId), 1, 1);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 1);
            Put(m, 0, F(FactoryCards.NightShiftAssemblerId), 1, 0);
            PassRound(m);
            Assert.AreEqual(2, Get(m, ResourceType.Toys), "no Parts, so the Assemblers make nothing; the Station still wraps 2");
        }

        [Test]
        public void BigWhistle_RunsEveryAssemblerOnce_RightNow()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.NightShiftAssemblerId), 1, 0);
            Put(m, 0, F(FactoryCards.ScrapPileId), 0, 1);
            m.GainResource(m.Players[0], ResourceType.Parts, 10);

            Assert.AreEqual(PlayResult.Ok, Whistle(m, 0, 2));
            Assert.AreEqual(2, Get(m, ResourceType.Toys));
            Assert.AreEqual(1, Get(m, ResourceType.Parts), "10 - 5 for the Whistle - 2 x 2 run; the Scrap Pile isn't an Assembler, so it doesn't run");
        }

        [Test]
        public void ToysMadeByTheWhistle_CountTowardsTheRoundGoal()
        {
            var m = FactoryMatch(goal: 3);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 1, 0);
            m.GainResource(m.Players[0], ResourceType.Parts, 20);
            PassRound(m);
            Assert.IsFalse(m.IsOver);
            Assert.AreEqual(PlayResult.Ok, Whistle(m, 2, 0));
            PassRound(m);
            Assert.IsTrue(m.IsOver);
        }

        [Test]
        public void BlueprintDesk_AddsAToyAssembler_EveryTwoDays()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.BlueprintDeskId), 0, 0);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(1, m.Players[0].Hand.Count(c => c.Definition.id == FactoryCards.ToyAssemblerId));
        }
    }
}
