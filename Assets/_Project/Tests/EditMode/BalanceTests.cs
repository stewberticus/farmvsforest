using System.Collections.Generic;
using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;

namespace FarmVsForest.Tests
{
    /// <summary>The card value model and the balance simulator (docs/BALANCE.md).</summary>
    public class BalanceTests
    {
        private static CardDefinition Card(Team team, List<ResourceAmount> cost, params EffectDefinition[] effects) =>
            new CardDefinition { id = "test", displayName = "Test", team = team, cost = cost, effects = effects.ToList() };

        private static List<ResourceAmount> Cost(ResourceType t, int n) => new List<ResourceAmount> { new ResourceAmount(t, n) };

        [Test]
        public void VanillaFreeProducer_IsExactlyOnCurve()
        {
            var vanilla = Card(Team.Farm, new List<ResourceAmount>(),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Wheat));
            var score = CardValuation.Evaluate(vanilla);
            Assert.AreEqual(10, score.Value, 1e-9);
            Assert.AreEqual(10, score.Par, 1e-9);
            Assert.AreEqual("on curve", score.Verdict);
        }

        [Test]
        public void DayAndNightTriggers_AreWorthTheSame()
        {
            var day = Card(Team.Farm, null, new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Wheat));
            var night = Card(Team.Farm, null, new EffectDefinition(EffectTrigger.Night, EffectKind.GainResource, 1, ResourceType.Wheat));
            Assert.AreEqual(CardValuation.Evaluate(day).Value, CardValuation.Evaluate(night).Value, 1e-9);
        }

        [Test]
        public void EveryThreeDays_IsAThirdOfEveryDay()
        {
            var s = new BalanceSettings();
            var every3 = new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 3, ResourceType.Wheat, interval: 3);
            Assert.AreEqual(s.horizon, CardValuation.Evaluate(Card(Team.Farm, null, every3), s).Value, 1e-9);
        }

        [Test]
        public void DelayedEffect_IsDiscountedForWaiting()
        {
            var once = new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 10, ResourceType.Wheat, interval: 4, repeat: false);
            // 10 V, minus 5% per round waited (4 rounds) = 8 V.
            Assert.AreEqual(8, CardValuation.Evaluate(Card(Team.Farm, null, once)).Value, 1e-9);
        }

        [Test]
        public void Cost_RaisesPar()
        {
            var s = new BalanceSettings();
            var cow = Card(Team.Farm, Cost(ResourceType.Wheat, 4),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Milk));
            var score = CardValuation.Evaluate(cow, s);
            Assert.AreEqual(4, score.Cost, 1e-9);
            Assert.AreEqual(s.parBase + s.parPerCost * 4, score.Par, 1e-9);
        }

        [Test]
        public void GoalTag_OnlyCountsForTheTeamWhoseGoalUsesIt()
        {
            var forestTree = Card(Team.Forest, null);
            forestTree.tags.Add(Tags.Tree);
            var darkTree = Card(Team.DarkForest, null);
            darkTree.tags.Add(Tags.Tree);
            Assert.Greater(CardValuation.Evaluate(forestTree).Value, 0);
            Assert.AreEqual(0, CardValuation.Evaluate(darkTree).Value, 1e-9);
        }

        [Test]
        public void UnknownCustomEffect_IsFlaggedForHandPricing()
        {
            var odd = Card(Team.Factory, null,
                new EffectDefinition(EffectTrigger.Day, EffectKind.Custom, 1) { customEffectId = "mystery" });
            Assert.AreEqual("price by hand", CardValuation.Evaluate(odd).Verdict);
        }

        [Test]
        public void OpeningHand_AlwaysHasAFreeCard([Values(1, 2, 3, 4, 5, 6, 7, 8)] int seed)
        {
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne, seed, Team.DarkForest, Team.Factory);
            var m = new Match(config);
            m.Start();
            Assert.IsTrue(m.Players.All(p => p.Hand.Any(c => c.Definition.IsFree)));
        }

        [Test]
        public void MadeCard_IsPricedFromTheCardItMakes()
        {
            var cow = Card(Team.Farm, Cost(ResourceType.Wheat, 2),
                new EffectDefinition(EffectTrigger.Day, EffectKind.GainResource, 1, ResourceType.Milk));
            cow.id = "test.cow";
            var barn = Card(Team.Farm, Cost(ResourceType.Wheat, 3),
                new EffectDefinition(EffectTrigger.Day, EffectKind.AddCardToHand, 1, interval: 2) { cardId = "test.cow" });
            barn.id = "test.barn";
            var scores = CardValuation.EvaluateAll(new[] { cow, barn });
            var s = new BalanceSettings();
            var barnScore = scores.First(c => c.Card.id == "test.barn");
            Assert.IsEmpty(barnScore.Warnings);
            // 5 Cows over the horizon, each worth madeCardValue plus a share of how far the Cow is above its own par.
            double cowExtra = 20 - s.Par(2);
            Assert.AreEqual(5 * (s.madeCardValue + s.madeCardExcessShare * cowExtra), barnScore.Value, 1e-9);
        }

        [Test]
        public void SkipIfInHand_DoesNotStackCopies()
        {
            var cow = Card(Team.Farm, null);
            cow.id = "test.cow";
            var barn = Card(Team.Farm, null,
                new EffectDefinition(EffectTrigger.Day, EffectKind.AddCardToHand, 1) { cardId = "test.cow", skipIfInHand = true });
            barn.id = "test.barn";
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne, 4, Team.Farm, Team.Forest);
            config.cardCatalog.Add(cow);
            var m = new Match(config);
            m.Start();
            var farm = m.Players[0];
            m.Summon(barn, farm, new Coord(1, 0));
            for (int i = 0; i < 8 && !m.IsOver; i++) m.EndTurn(m.CurrentPlayer.Id);
            Assert.AreEqual(1, farm.Hand.Count(c => c.Definition.id == "test.cow"));
        }

        [Test]
        public void Simulator_IsDeterministicForASeed()
        {
            var sim = new BalanceSimulator();
            var a = sim.Play(Team.Farm, Team.Factory, "greedy", "greedy", 42);
            var b = sim.Play(Team.Farm, Team.Factory, "greedy", "greedy", 42);
            Assert.AreEqual(a.Rounds, b.Rounds);
            Assert.AreEqual(a.WinnerSeat, b.WinnerSeat);
            CollectionAssert.AreEqual(a.Seats[0].CardsPlayed, b.Seats[0].CardsPlayed);
        }

        [Test]
        public void Series_SwapsSeatsAndEveryGameEnds()
        {
            var sim = new BalanceSimulator();
            var games = sim.Series(Team.Farm, Team.DarkForest, 6);
            Assert.AreEqual(3, games.Count(g => g.Seats[0].Team == Team.Farm));
            Assert.IsTrue(games.All(g => g.End != EndKind.Unfinished));
        }

        [Test]
        public void Goldfish_OpponentNeverPlays()
        {
            var sim = new BalanceSimulator();
            var games = sim.Goldfish(Team.Farm, 3);
            Assert.IsTrue(games.All(g => g.Seats[1].CardsPlayed.Count == 0));
            Assert.IsTrue(games.All(g => g.Seats[0].CardsPlayed.Count > 0));
        }

        [Test]
        public void Report_RendersAllSections()
        {
            var sim = new BalanceSimulator();
            var flags = new List<string>();
            var series = new Dictionary<(Team, Team), List<MatchRecord>> { { (Team.Farm, Team.Forest), sim.Series(Team.Farm, Team.Forest, 4) } };
            StringAssert.Contains("Farm vs Forest", BalanceReport.Matchups(series, new BalanceTargets(), flags));
            StringAssert.Contains("Farm", BalanceReport.Goldfish(new Dictionary<Team, List<MatchRecord>> { { Team.Farm, sim.Goldfish(Team.Farm, 2) } },
                new BalanceTargets(), flags));
            StringAssert.Contains("Wheat Field", BalanceReport.Cards(PlaceholderCards.ForTeam(Team.Farm), new BalanceSettings(), series.Values.First(), flags));
        }
    }
}
