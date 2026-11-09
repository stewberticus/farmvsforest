using System;
using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class MatchSetupTests
    {
        [TestCase(GameMode.OneVsOne, 2, 2, 10)]
        [TestCase(GameMode.TwoVsTwo, 4, 2, 16)]
        [TestCase(GameMode.FreeForAll, 4, 4, 16)]
        public void Modes_HaveExpectedPlayersAlliancesAndBoardSize(GameMode mode, int players, int alliances, int size)
        {
            var m = Started(PlaceholderCards.DefaultConfig(mode));
            Assert.AreEqual(players, m.Players.Count);
            Assert.AreEqual(alliances, m.Players.Select(p => p.AllianceId).Distinct().Count());
            Assert.AreEqual(players, m.Players.Select(p => p.Corner).Distinct().Count(), "each player gets their own corner");
            Assert.AreEqual(size, m.Board.Width);
            Assert.AreEqual(size, m.Board.Height);
        }

        [Test]
        public void StartingCards_ArePlacedInEachCorner()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.FreeForAll));
            foreach (var p in m.Players)
            {
                var corner = p.Corner.ToCoord(m.Board.Width, m.Board.Height);
                Assert.AreSame(p.StartingCard, m.Board.Get(corner));
                Assert.IsTrue(p.StartingCard.Definition.HasTag(Tags.Start));
            }
        }

        [Test]
        public void DayTeamsPlayInTheDay_DarkForestPlaysAtNight()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.FreeForAll));
            // Seats: Farm, Forest, Factory, Dark Forest.
            var order = new[] { (0, TimeOfDay.Day), (1, TimeOfDay.Day), (2, TimeOfDay.Day), (3, TimeOfDay.Night), (0, TimeOfDay.Day) };
            foreach (var (id, time) in order)
            {
                Assert.AreEqual(id, m.CurrentPlayer.Id);
                Assert.AreEqual(time, m.TimeOfDay);
                m.EndTurn(id);
            }
            Assert.AreEqual(2, m.Round);
        }

        [Test]
        public void WithoutDarkForest_BothPlayInTheDay_AndNightJustPasses()
        {
            var m = Started(SimpleConfig(Card("x"), teams: new[] { Team.Farm, Team.Factory }));
            Assert.AreEqual(0, m.CurrentPlayer.Id);
            m.EndTurn(0);
            Assert.AreEqual(1, m.CurrentPlayer.Id);
            Assert.AreEqual(TimeOfDay.Day, m.TimeOfDay);
            m.EndTurn(1);
            Assert.AreEqual(0, m.CurrentPlayer.Id);
            Assert.AreEqual(2, m.Round);
            Assert.IsTrue(m.Log.Any(e => e.Type == GameEventType.NightStarted));
        }

        [Test]
        public void WrongPlayerCount_Throws()
        {
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne);
            config.mode = GameMode.FreeForAll;
            Assert.Throws<InvalidOperationException>(() => new Match(config).Start());
        }

        [Test]
        public void StartingHands_AreDealt_AndFirstPlayerSkipsDrawByDefault()
        {
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne);
            var m = Started(config);
            Assert.AreEqual(config.startingHandSize, m.Players[0].Hand.Count);
            Assert.AreEqual(config.startingHandSize, m.Players[1].Hand.Count);
            m.EndTurn(0);
            Assert.AreEqual(config.startingHandSize + 1, m.Players[1].Hand.Count, "second player draws on their turn");
        }

        [Test]
        public void SameSeed_GivesSameShuffle()
        {
            var a = Started(PlaceholderCards.DefaultConfig(GameMode.OneVsOne, seed: 7));
            var b = Started(PlaceholderCards.DefaultConfig(GameMode.OneVsOne, seed: 7));
            CollectionAssert.AreEqual(
                a.Players[0].Hand.Select(c => c.Definition.id).ToList(),
                b.Players[0].Hand.Select(c => c.Definition.id).ToList());
        }

        [Test]
        public void RoundLimit_EndsWithMostSquaresWinning()
        {
            var config = SimpleConfig(Card("x"));
            config.roundLimit = 2;
            var m = Started(config);
            Put(m, 1, Card("a"), 5, 5);
            for (int i = 0; i < 4; i++) m.EndTurn(m.CurrentPlayer.Id);
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(m.Players[1].AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void FullHand_BurnsExtraDraws()
        {
            var config = SimpleConfig(Card("x"), deckSize: 20);
            config.maxHandSize = 3;
            var m = Started(config);
            Assert.AreEqual(3, m.Players[0].Hand.Count);
            Assert.AreEqual(1, m.Players[0].Discard.Count, "first player drew a 4th card at turn start and burned it");
        }
    }
}
