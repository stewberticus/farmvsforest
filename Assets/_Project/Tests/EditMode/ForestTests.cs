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
            Assert.AreEqual(PlayResult.Ok, PlayAnywhere(m, p.Hand[0]), "and another");
            Assert.AreEqual(1, p.Resources.Get(ResourceType.Acorns));
            Assert.AreEqual(PlayResult.CannotAfford, PlayAnywhere(m, p.Hand[0]), "out of Acorns");
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
        public void StarterDeck_Is30Cards_AndHugeTreeDoesNothing()
        {
            var deck = ForestCards.Deck();
            Assert.AreEqual(30, deck.Count);
            Assert.IsTrue(deck.All(c => c.team == Team.Forest));
            Assert.IsEmpty(ForestCards.HugeTree.effects);
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
    }
}
