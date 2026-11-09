using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class GoalTests
    {
        private static MatchConfig Vs(Team a, Team b) => SimpleConfig(Card("x"), teams: new[] { a, b }, boardSize: 8);

        [Test]
        public void Farm_WinsWithEnoughOfEachResource()
        {
            var m = Started(Vs(Team.Farm, Team.Forest));
            var farm = m.Players[0];
            int goal = m.Config.farmGoalEach;
            farm.Resources.Add(ResourceType.Wheat, goal);
            farm.Resources.Add(ResourceType.Milk, goal);
            farm.Resources.Add(ResourceType.Eggs, goal - 1);
            Put(m, 0, Card("hen", effects: Gain(EffectTrigger.Day, ResourceType.Eggs)), 0, 0);
            m.EndTurn(0);
            m.EndTurn(1);
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(farm.AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void Forest_WinsWith30TouchingTrees_ButNotScatteredOnes()
        {
            var config = Vs(Team.Forest, Team.Farm);
            var m = Started(config);
            var tree = Card("tree", tags: new[] { Tags.Tree });
            // Two separate groups of 20 and 9 don't count.
            for (int i = 0; i < 20; i++) Put(m, 0, tree, i % 8, i / 8);
            for (int i = 0; i < 9; i++) Put(m, 0, tree, i % 8, 4 + i / 8);
            var goal = (ConnectedTaggedCardsGoal)m.GoalOf(m.Players[0]);
            Assert.AreEqual(20, goal.LargestGroup(m, m.Players[0]));

            // Joining them up (row 3) makes one forest of 37.
            m.Players[0].Hand.Add(m.CreateInstance(tree, m.Players[0]));
            for (int x = 0; x < 8; x++) if (x != 7) Put(m, 0, tree, x, 3);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, m.Players[0].Hand.Count - 1, new Coord(7, 3)));
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(m.Players[0].AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void Factory_WinsByMaking10GoodsInOneRound_NotSavedUp()
        {
            var m = Started(Vs(Team.Factory, Team.Farm));
            var factory = m.Players[0];
            Put(m, 0, Card("press", effects: Gain(EffectTrigger.Day, ResourceType.Goods, 6)), 0, 0);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(12, factory.Resources.Get(ResourceType.Goods));
            Assert.IsFalse(m.IsOver, "12 Goods saved up over two rounds doesn't count");

            Put(m, 0, Card("night shift", effects: Gain(EffectTrigger.Night, ResourceType.Goods, 4)), 1, 0);
            PassRound(m);
            Assert.IsTrue(m.IsOver, "6 in the day + 4 at night is 10 in one round");
            Assert.AreEqual(factory.AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void DarkForest_WinsWith13Ghouls()
        {
            var m = Started(Vs(Team.Farm, Team.DarkForest));
            var dark = m.Players[1];
            dark.Resources.Add(ResourceType.Ghouls, 12);
            Put(m, 1, Card("lantern", effects: Gain(EffectTrigger.Night, ResourceType.Ghouls)), 7, 7);
            m.EndTurn(0); // Farm's day turn ends, night falls
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(dark.AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void FullBoard_MostSquaresWins()
        {
            var m = Started(SimpleConfig(Card("x"), boardSize: 2));
            Put(m, 1, Card("a"), 1, 1);
            Put(m, 1, Card("a"), 1, 0);
            Put(m, 1, Card("a"), 0, 1);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(0, 0, new Coord(0, 0)), "last square, on P0's corner");
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(m.Players[1].AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void FullBoard_TieIsADraw()
        {
            var m = Started(SimpleConfig(Card("x"), boardSize: 2));
            Put(m, 0, Card("a"), 0, 0);
            Put(m, 0, Card("a"), 0, 1);
            Put(m, 1, Card("a"), 1, 1);
            m.EndTurn(0);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(1, 0, new Coord(1, 0)));
            Assert.IsTrue(m.IsOver);
            Assert.IsNull(m.WinningAllianceId);
        }

        [Test]
        public void TwoVsTwo_EitherPartnerReachingTheirGoalWinsForBoth()
        {
            var config = SimpleConfig(Card("x"), mode: GameMode.TwoVsTwo,
                teams: new[] { Team.Farm, Team.Forest, Team.DarkForest, Team.Factory });
            var m = Started(config);
            var dark = m.Players[2];
            dark.Resources.Add(ResourceType.Ghouls, 13);
            PassRound(m);
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(dark.AllianceId, m.WinningAllianceId);
            Assert.AreEqual(m.Players[0].AllianceId, m.WinningAllianceId, "Farm is Dark Forest's partner");
        }
    }
}
