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

        private static int Get(Match m, ResourceType r) => m.Players[0].Resources.Get(r);

        [Test]
        public void Deck_Has30Cards_AndEveryCardIsUsed()
        {
            Assert.AreEqual(30, FactoryCards.Deck().Count);
            CollectionAssert.AreEquivalent(FactoryCards.All().Select(c => c.id), FactoryCards.DeckCounts.Keys);
        }

        [Test]
        public void Headquarters_MakesAPartEachDay()
        {
            var e = FactoryCards.Headquarters().effects.Single();
            Assert.AreEqual(EffectTrigger.Day, e.trigger);
            Assert.AreEqual(ResourceType.Parts, e.resource);
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
        public void BigOrder_CountsDownThreeDays_ThenRunsEveryAssemblerTwiceMore_Once()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 1, 0);
            Put(m, 0, F(FactoryCards.ScrapPileId), 0, 1);
            m.GainResource(m.Players[0], ResourceType.Parts, 60);
            var order = m.AddToHand(m.Players[0], F(FactoryCards.BigOrderId));
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, order, new Coord(2, 0)));
            Assert.AreEqual(FactoryCards.BigOrderDays, FactoryCards.OrderDaysLeft(order));

            var toysPerRound = new System.Collections.Generic.List<int>();
            var daysLeft = new System.Collections.Generic.List<int>();
            for (int r = 0; r < 5; r++)
            {
                PassRound(m);
                daysLeft.Add(FactoryCards.OrderDaysLeft(order));
                toysPerRound.Add(m.Players[0].GainedThisRound.Get(ResourceType.Toys));
            }
            // Each round 2 Assemblers make 1 Toy each; on the payoff day each runs twice more (+4). The Scrap Pile doesn't run.
            CollectionAssert.AreEqual(new[] { 2, 2, 6, 2, 2 }, toysPerRound);
            CollectionAssert.AreEqual(new[] { 2, 1, -1, -1, -1 }, daysLeft);
        }

        [Test]
        public void ToysFromTheBigOrder_CountTowardsTheRoundGoal()
        {
            var m = FactoryMatch(goal: 6);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 1, 0);
            m.GainResource(m.Players[0], ResourceType.Parts, 40);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, m.AddToHand(m.Players[0], F(FactoryCards.BigOrderId)), new Coord(2, 0)));
            for (int r = 0; r < 4 && !m.IsOver; r++) PassRound(m);
            Assert.IsTrue(m.IsOver);
        }

        [Test]
        public void Forecast_SeesTheOrderPayOff_AndTheBotHoldsItUntilThen()
        {
            var m = FactoryMatch(goal: 6);
            var p = m.Players[0];
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 1, 0);
            var def = F(FactoryCards.BigOrderId);
            Assert.AreEqual(0, FactoryCards.ForecastToys(m, p, 3, extraOrders: 1), "no Parts and no Parts income: nothing to build with");
            Assert.IsFalse(Bots.OrderIsReady(m, p, def));
            m.GainResource(p, ResourceType.Parts, 40);
            Assert.AreEqual(2, FactoryCards.ForecastToys(m, p, 3));
            Assert.AreEqual(6, FactoryCards.ForecastToys(m, p, 3, extraOrders: 1));
            Assert.IsTrue(Bots.OrderIsReady(m, p, def));
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

        [Test]
        public void Assemblers_OnlyUseSpareParts_AlwaysLeaving3()
        {
            var m = FactoryMatch();
            Put(m, 0, F(FactoryCards.ToyAssemblerId), 0, 0);
            Put(m, 0, F(FactoryCards.NightShiftAssemblerId), 1, 0);
            m.GainResource(m.Players[0], ResourceType.Parts, 6);
            PassRound(m);
            Assert.AreEqual(1, Get(m, ResourceType.Toys), "day: 6 -> 4 Parts and a Toy; night: 4 isn't enough to keep 3");
            Assert.AreEqual(4, Get(m, ResourceType.Parts));
        }

        [Test]
        public void SparePartsBin_Gives2NowAnd1EveryThirdDay()
        {
            var m = FactoryMatch();
            var bin = m.AddToHand(m.Players[0], F(FactoryCards.SparePartsBinId));
            Put(m, 0, F(FactoryCards.ScrapPileId), 0, 0);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, bin, new Coord(1, 0)));
            Assert.AreEqual(2, Get(m, ResourceType.Parts));
            int before = Get(m, ResourceType.Parts);
            PassRound(m); PassRound(m); PassRound(m);
            Assert.AreEqual(before + 3 + 1, Get(m, ResourceType.Parts), "3 from the Scrap Pile, 1 from the Bin on its 3rd day");
        }
    }
}
