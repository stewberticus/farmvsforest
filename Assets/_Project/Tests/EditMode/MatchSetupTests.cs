using System;
using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class MatchSetupTests
    {
        [TestCase(GameMode.OneVsOne, 2, 2)]
        [TestCase(GameMode.TwoVsTwo, 4, 2)]
        [TestCase(GameMode.FreeForAll, 4, 4)]
        public void Modes_HaveExpectedPlayersAndAlliances(GameMode mode, int players, int alliances)
        {
            var m = Started(PlaceholderCards.DefaultConfig(mode));
            Assert.AreEqual(players, m.Players.Count);
            Assert.AreEqual(alliances, m.Players.Select(p => p.AllianceId).Distinct().Count());
            Assert.AreEqual(players, m.Players.Select(p => p.Corner).Distinct().Count(), "each player gets their own corner");
        }

        [Test]
        public void TwoVsTwo_TurnsAlternateBetweenSides()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.TwoVsTwo));
            int last = -1;
            for (int i = 0; i < 4; i++)
            {
                Assert.AreNotEqual(last, m.CurrentPlayer.AllianceId);
                last = m.CurrentPlayer.AllianceId;
                m.EndTurn(m.CurrentPlayer.Id);
            }
        }

        [Test]
        public void WrongPlayerCount_Throws()
        {
            var config = PlaceholderCards.DefaultConfig(GameMode.OneVsOne);
            config.mode = GameMode.FreeForAll;
            Assert.Throws<InvalidOperationException>(() => new Match(config).Start());
        }

        [Test]
        public void HomeCards_ArePlacedInCorners()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.FreeForAll));
            foreach (var p in m.Players)
            {
                var corner = p.Corner.ToCoord(m.Board.Width, m.Board.Height);
                Assert.AreSame(p.HomeCard, m.Board.Get(corner));
            }
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
        public void Rounds_AdvanceAfterEveryoneHasPlayed()
        {
            var m = Started(SimpleConfig(Card("x")));
            Assert.AreEqual(1, m.Round);
            m.EndTurn(0);
            Assert.AreEqual(1, m.Round);
            m.EndTurn(1);
            Assert.AreEqual(2, m.Round);
        }

        [Test]
        public void RoundLimit_EndsInDraw()
        {
            var config = SimpleConfig(Card("x"));
            config.roundLimit = 2;
            var m = Started(config);
            for (int i = 0; i < 4; i++) m.EndTurn(m.CurrentPlayer.Id);
            Assert.IsTrue(m.IsOver);
            Assert.IsNull(m.WinningAllianceId);
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
