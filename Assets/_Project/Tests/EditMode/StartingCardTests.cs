using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;

namespace FarmVsForest.Tests
{
    /// <summary>Starting cards make 1 basic resource a day (Evil Tree: a night). Phase 1 of the 2026-11-11 plan.</summary>
    public class StartingCardTests
    {
        private static Match Start(Team a, Team b, int income = 1)
        {
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne, 7, a, b);
            config.startingCardIncome = income;
            var m = new Match(config);
            m.Start();
            return m;
        }

        [TestCase(Team.Farm, ResourceType.Wheat)]
        [TestCase(Team.Forest, ResourceType.Acorns)]
        [TestCase(Team.Factory, ResourceType.Parts)]
        public void DayTeamStartingCard_GivesOneOnTheFirstMorning(Team team, ResourceType resource)
        {
            var m = Start(team, Team.DarkForest);
            Assert.AreEqual(1, m.Players[0].Resources.Get(resource));
        }

        [Test]
        public void EvilTree_GivesABatAtNight_NotInTheMorning()
        {
            var m = Start(Team.Farm, Team.DarkForest);
            var dark = m.Players[1];
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Bats));
            m.EndTurn(m.CurrentPlayer.Id); // Farm's day turn ends, night falls
            Assert.AreEqual(1, dark.Resources.Get(ResourceType.Bats));
        }

        [Test]
        public void IncomeSwitch_ZeroTurnsItOff()
        {
            var m = Start(Team.Farm, Team.Factory, income: 0);
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Wheat));
            Assert.AreEqual(0, m.Players[1].Resources.Get(ResourceType.Parts));
        }

        [Test]
        public void FreeCardOpeningRule_IsOffByDefault()
        {
            Assert.IsFalse(new MatchConfig().openingHandHasFreeCard);
        }

        [Test]
        public void EveryTeam_CanPlaySomethingWithin3Turns_WithoutTheFreeCardRule([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int seed)
        {
            foreach (var team in BalanceReport.AllTeams)
            {
                var record = new BalanceSimulator().Play(team, team == Team.Farm ? Team.Forest : Team.Farm, "greedy", "pass", seed);
                Assert.Less(record.Seats[0].PassTurns, record.Seats[0].Turns, team + " never played");
            }
        }
    }
}
