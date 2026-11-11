using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class ForestTests
    {
        private static Match ForestFirst(CardDefinition deckCard, Team first = Team.Forest)
        {
            var config = SimpleConfig(deckCard, deckSize: 10, teams: new[] { first, Team.Farm },
                start: Card("start", tags: new[] { Tags.Start, Tags.Tree }));
            return Started(config);
        }

        private static PlayResult PlayAnywhere(Match m, CardInstance card)
        {
            var p = m.CurrentPlayer;
            return m.PlayCard(p.Id, card, m.LegalSquares(p, card.Definition).First());
        }

        [Test]
        public void Forest_CanPayAcornsToPlayExtraTrees()
        {
            var m = ForestFirst(ForestCards.Sapling);
            var p = m.CurrentPlayer;
            Assert.AreEqual(Team.Forest, p.Team);
            m.GainResource(p, ResourceType.Acorns, 5);

            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]), "normal play is free");
            Assert.AreEqual(5, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]), "extra tree for 2 Acorns");
            Assert.AreEqual(3, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]), "the next extra tree costs 3");
            Assert.AreEqual(0, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(PlayResult.CannotAfford, PlayAnywhere(m, p.Hand[0]), "out of Acorns");
        }

        [Test]
        public void Sapling_GrowsOnSunnyMornings_IntoYoungThenOldTree()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm }, start: ForestCards.HugeTree);
            config.maxHandSize = 30;
            var m = Started(config);
            var s = Put(m, 0, ForestCards.Sapling, 2, 2); // open squares all round
            for (int i = 0; i < ForestCards.SaplingGrowDays; i++) PassRound(m);
            Assert.AreEqual(ForestCards.YoungTree.id, s.Definition.id);
            for (int i = 0; i < ForestCards.YoungGrowDays; i++) PassRound(m);
            Assert.AreEqual(ForestCards.OldTree.id, s.Definition.id);
            Assert.AreEqual(new Coord(2, 2), s.Position, "growing never moves a card");
        }

        [Test]
        public void Sapling_WithNoEmptySquareTouchingIt_DoesntGrow()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm }, start: ForestCards.HugeTree);
            config.maxHandSize = 30;
            var m = Started(config);
            var s = Put(m, 0, ForestCards.Sapling, 2, 2);
            var rock = Card("rock");
            Put(m, 0, rock, 1, 2); Put(m, 0, rock, 3, 2); Put(m, 0, rock, 2, 1); Put(m, 0, rock, 2, 3);
            for (int i = 0; i < 6; i++) PassRound(m);
            Assert.AreEqual(ForestCards.SaplingId, s.Definition.id);
        }

        [Test]
        public void Forest_WinsWithEnoughOldTreesInOneForest()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm }, start: ForestCards.HugeTree);
            config.forestGoalOldTrees = 3;
            var m = Started(config);
            Put(m, 0, ForestCards.Sapling, 1, 0);
            Put(m, 0, ForestCards.OldTree, 2, 0);
            Put(m, 0, ForestCards.OldTree, 4, 0); // not connected
            var goal = (ConnectedTaggedCardsGoal)m.GoalOf(m.Players[0]);
            Assert.AreEqual(2, goal.LargestGroup(m, m.Players[0]), "Huge Tree + the touching Old Tree; Saplings connect but don't count");
            Assert.IsFalse(m.IsOver);
            var old = m.CreateInstance(ForestCards.OldTree, m.Players[0]);
            m.Players[0].Hand.Add(old);
            Assert.AreEqual(PlayResult.Ok, m.PlayCard(m.CurrentPlayer.Id, old, new Coord(3, 0)));
            Assert.IsTrue(m.IsOver);
        }

        [Test]
        public void Compost_DiscardsACardForAnAcorn_OnceATurn()
        {
            var m = ForestFirst(ForestCards.Sapling);
            var p = m.CurrentPlayer;
            int hand = p.Hand.Count;
            Assert.IsTrue(m.Compost(p.Id, p.Hand[0]));
            Assert.AreEqual(1, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(hand - 1, p.Hand.Count);
            Assert.IsFalse(m.Compost(p.Id, p.Hand[0]), "once a turn");
        }

        [Test]
        public void Compost_IsOnlyForForest()
        {
            var m = ForestFirst(ForestCards.Sapling, first: Team.Farm);
            Assert.IsFalse(m.Compost(m.CurrentPlayer.Id, m.CurrentPlayer.Hand[0]));
        }

        [Test]
        public void WildflowerMeadow_MakesHoneyPerEmptySquare()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm }, start: ForestCards.HugeTree);
            var m = Started(config);
            Put(m, 0, ForestCards.WildflowerMeadow, 2, 2);
            Put(m, 0, Card("rock"), 1, 2);
            PassRound(m);
            Assert.AreEqual(3, m.Players[0].Resources.Get(ResourceType.Honey));
        }

        [Test]
        public void ExtraPlay_AddsTheCardsOwnCost()
        {
            var m = ForestFirst(ForestCards.Oak);
            var p = m.CurrentPlayer;
            m.GainResource(p, ResourceType.Acorns, 6);
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]));
            Assert.AreEqual(4, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]), "Oak 2 + extra 2");
            Assert.AreEqual(0, p.Resources.Get(ResourceType.Acorns));
        }

        [Test]
        public void ExtraPlay_IsOnlyForTrees()
        {
            var m = ForestFirst(ForestCards.SquirrelDrey);
            var p = m.CurrentPlayer;
            m.GainResource(p, ResourceType.Acorns, 10);
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]));
            Assert.AreEqual(PlayResult.NoPlaysLeft, PlayAnywhere(m, p.Hand[0]));
        }

        [Test]
        public void ExtraPlay_IsOnlyForForest()
        {
            var m = ForestFirst(ForestCards.Sapling, first: Team.Farm);
            var p = m.CurrentPlayer;
            Assert.AreEqual(Team.Farm, p.Team);
            m.GainResource(p, ResourceType.Acorns, 10);
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]));
            Assert.AreEqual(PlayResult.NoPlaysLeft, PlayAnywhere(m, p.Hand[0]));
        }

        [Test]
        public void ExtraPlay_CanBeTurnedOff()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm },
                start: Card("start", tags: new[] { Tags.Start, Tags.Tree }));
            config.forestExtraTreeAcorns = 0;
            var m = Started(config);
            var p = m.CurrentPlayer;
            m.GainResource(p, ResourceType.Acorns, 10);
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]));
            Assert.AreEqual(PlayResult.NoPlaysLeft, PlayAnywhere(m, p.Hand[0]));
        }

        [Test]
        public void StarterDeck_Is30Cards_AndHugeTreeMakesAnAcorn()
        {
            var deck = ForestCards.Deck();
            Assert.AreEqual(30, deck.Count);
            Assert.IsTrue(deck.All(c => c.team == Team.Forest));
            Assert.AreEqual(ResourceType.Acorns, ForestCards.HugeTree.effects.Single().resource);
            StringAssert.Contains("Compost", ForestCards.HugeTree.rulesText);
            Assert.IsTrue(ForestCards.HugeTree.HasTag(Tags.Tree));
        }

        [Test]
        public void SaplingMakers_PutRealSaplingsInHand()
        {
            var config = SimpleConfig(ForestCards.Sapling, teams: new[] { Team.Forest, Team.Farm },
                start: ForestCards.HugeTree);
            config.maxHandSize = 20;
            var m = Started(config);
            var p = m.Players[0];
            Put(m, 0, ForestCards.WeepingWillow, 0, 1);
            int before = p.Hand.Count(c => c.Definition.id == ForestCards.SaplingId);
            PassRound(m);
            int after = p.Hand.Count(c => c.Definition.id == ForestCards.SaplingId);
            Assert.Greater(after, before + 1, "the normal draw plus the Willow's Sapling");
        }

        [Test]
        public void Pollinate_SpendsHoneyToGrowATreeOneStep_EvenInShade()
        {
            var m = ForestFirst(ForestCards.Sapling);
            var p = m.CurrentPlayer;
            var s = Put(m, p.Id, ForestCards.Sapling, 2, 2);
            var rock = Card("rock");
            Put(m, p.Id, rock, 1, 2); Put(m, p.Id, rock, 3, 2); Put(m, p.Id, rock, 2, 1); Put(m, p.Id, rock, 2, 3);
            Assert.IsFalse(m.Pollinate(p.Id, s), "no Honey yet");
            m.GainResource(p, ResourceType.Honey, 4);
            Assert.IsTrue(m.Pollinate(p.Id, s));
            Assert.IsTrue(m.Pollinate(p.Id, s));
            Assert.AreEqual(0, p.Resources.Get(ResourceType.Honey));
            Assert.AreEqual(ForestCards.YoungTree.id, s.Definition.id, "2 steps turn a Sapling into a Young Tree");
        }

        [Test]
        public void Pollinate_OnlyYourOwnGrowingTrees()
        {
            var m = ForestFirst(ForestCards.Sapling);
            var p = m.CurrentPlayer;
            m.GainResource(p, ResourceType.Honey, 10);
            var old = Put(m, p.Id, ForestCards.OldTree, 2, 2);
            var squirrel = Put(m, p.Id, ForestCards.SquirrelDrey, 3, 3);
            var theirs = Put(m, 1, ForestCards.Sapling, 4, 4);
            Assert.IsFalse(m.Pollinate(p.Id, old), "already old");
            Assert.IsFalse(m.Pollinate(p.Id, squirrel), "not a tree");
            Assert.IsFalse(m.Pollinate(p.Id, theirs), "not yours");
        }
    }
}
