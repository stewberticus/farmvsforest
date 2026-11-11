using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    /// <summary>Mulligan and scout effects (phase 4 of the 2026-11-11 plan).</summary>
    public class SteeringDrawTests
    {
        private static CardDefinition Scout(int n = 3)
        {
            var def = Card("scout");
            def.effects.Add(new EffectDefinition(EffectTrigger.OnPlay, EffectKind.Scout, n));
            return def;
        }

        private static Match Simple(CardDefinition deckCard = null, int mulligans = 1)
        {
            var config = SimpleConfig(deckCard ?? Card("free"), deckSize: 12);
            config.mulligans = mulligans;
            return Started(config);
        }

        [Test]
        public void Mulligan_SwapsTheWholeHand_OnceOnTheFirstTurn()
        {
            var m = Simple();
            var p = m.CurrentPlayer;
            int hand = p.Hand.Count, deck = p.Deck.Count;
            var before = p.Hand.ToList();
            Assert.IsTrue(m.Mulligan(p.Id));
            Assert.AreEqual(hand, p.Hand.Count);
            Assert.AreEqual(deck, p.Deck.Count);
            Assert.IsFalse(m.CanMulligan(p.Id), "only one");
            Assert.IsFalse(before.SequenceEqual(p.Hand), "new cards");
        }

        [Test]
        public void Mulligan_NotAfterPlaying_NorOnLaterTurns_NorWhenOff()
        {
            var m = Simple();
            var p = m.CurrentPlayer;
            Put(m, p.Id, Card("start"), 0, 0);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(p.Id, p.Hand[0], new Coord(1, 0)));
            Assert.IsFalse(m.CanMulligan(p.Id));

            var later = Simple();
            var q = later.CurrentPlayer;
            PassRound(later);
            Assert.AreEqual(q, later.CurrentPlayer);
            Assert.IsFalse(later.CanMulligan(q.Id));

            var off = Simple(mulligans: 0);
            Assert.IsFalse(off.CanMulligan(off.CurrentPlayer.Id));
        }

        [Test]
        public void Scout_ShowsTopThree_KeepsOne_RestGoToTheBottom()
        {
            var m = Simple();
            var p = m.CurrentPlayer;
            Put(m, p.Id, Card("start"), 0, 0);
            var top3 = p.Deck.Skip(p.Deck.Count - 3).Reverse().ToList();
            p.Hand.Add(m.CreateInstance(Scout(), p));
            int hand = p.Hand.Count;
            int deck = p.Deck.Count;
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(p.Id, p.Hand.Last(), new Coord(1, 0)));
            Assert.IsNotNull(m.PendingScout);
            CollectionAssert.AreEqual(top3, m.PendingScout.Cards);
            Assert.AreEqual(PlayResult.ChoicePending, m.CheckPlay(p.Id, p.Hand[0], new Coord(0, 1)));

            Assert.IsTrue(m.ChooseScouted(p.Id, 1));
            Assert.IsNull(m.PendingScout);
            Assert.AreSame(top3[1], p.Hand.Last());
            Assert.AreEqual(hand, p.Hand.Count, "played the scout, kept one");
            Assert.AreEqual(deck - 1, p.Deck.Count);
            CollectionAssert.Contains(p.Deck.Take(2).ToList(), top3[0]);
            CollectionAssert.Contains(p.Deck.Take(2).ToList(), top3[2]);
        }

        [Test]
        public void Scout_LeftOpen_KeepsTheFirstCardWhenTheTurnEnds()
        {
            var m = Simple();
            var p = m.CurrentPlayer;
            Put(m, p.Id, Card("start"), 0, 0);
            var top = p.Deck.Last();
            p.Hand.Add(m.CreateInstance(Scout(), p));
            m.PlayCard(p.Id, p.Hand.Last(), new Coord(1, 0));
            m.EndTurn(p.Id);
            Assert.IsNull(m.PendingScout);
            CollectionAssert.Contains(p.Hand, top);
        }

        [Test]
        public void EveryTeam_HasTwoOrThreeScoutCards_InA30CardDeck()
        {
            foreach (var team in BalanceReport.AllTeams)
            {
                var deck = PlaceholderCards.StarterDeck(team);
                Assert.AreEqual(30, deck.Count, team.ToString());
                int scouts = deck.Count(c => c.effects.Any(e => e.kind == EffectKind.Scout));
                Assert.That(scouts, Is.InRange(2, 3), team.ToString());
            }
        }

        [Test]
        public void GreedyBot_ResolvesItsScouts_AndFinishesGames([Values(1, 2, 3)] int seed)
        {
            foreach (var team in BalanceReport.AllTeams)
            {
                var r = new BalanceSimulator().Play(team, team == Team.Farm ? Team.Forest : Team.Farm, "greedy", "greedy", seed);
                Assert.AreNotEqual(EndKind.Unfinished, r.End, team.ToString());
            }
        }
    }
}
