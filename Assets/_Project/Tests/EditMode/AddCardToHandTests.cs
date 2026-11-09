using System.Collections.Generic;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class AddCardToHandTests
    {
        private static EffectDefinition AddCard(string cardId, EffectTrigger trigger = EffectTrigger.Day, int amount = 1,
            int interval = 1) =>
            new EffectDefinition(trigger, EffectKind.AddCardToHand, amount, interval: interval) { cardId = cardId };

        private static MatchConfig ConfigWithCatalog(params CardDefinition[] catalog)
        {
            var config = SimpleConfig(Card("x"));
            config.cardCatalog = new List<CardDefinition>(catalog);
            return config;
        }

        [Test]
        public void EveryTwoDays_AddsANewCopyOfTheNamedCard()
        {
            var cow = Card("farm.cow", wheatCost: 2);
            var config = ConfigWithCatalog(cow);
            config.maxHandSize = 20;
            var m = Started(config);
            Put(m, 0, Card("barn", effects: AddCard("farm.cow", interval: 2)), 0, 0);
            var hand = m.Players[0].Hand;
            int deckBefore = m.Players[0].Deck.Count;

            PassRound(m);
            Assert.AreEqual(0, hand.FindAll(c => c.Definition == cow).Count);
            PassRound(m);
            Assert.AreEqual(1, hand.FindAll(c => c.Definition == cow).Count);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(2, hand.FindAll(c => c.Definition == cow).Count);
            // The new cards are created, not taken from the deck.
            Assert.AreEqual(deckBefore - 4, m.Players[0].Deck.Count, "only the normal one-per-turn draws come from the deck");
            Assert.AreNotSame(hand.Find(c => c.Definition == cow), hand.FindLast(c => c.Definition == cow));
        }

        [Test]
        public void AddedCard_StillCostsItsNormalPrice()
        {
            var cow = Card("farm.cow", wheatCost: 2);
            var m = Started(ConfigWithCatalog(cow));
            Put(m, 0, Card("barn", effects: AddCard("farm.cow")), 0, 0);
            PassRound(m);
            var added = m.Players[0].Hand.Find(c => c.Definition == cow);
            Assert.IsNotNull(added);
            Assert.AreEqual(PlayResult.CannotAfford, m.PlayCard(0, added, new Coord(1, 0)));
        }

        [Test]
        public void FindsCardsFromDecksWithoutACatalog()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("copier", effects: AddCard("x")), 0, 0);
            int before = m.Players[0].Hand.Count;
            PassRound(m);
            Assert.AreEqual(before + 2, m.Players[0].Hand.Count, "one normal draw plus one added card");
        }

        [Test]
        public void FullHand_LosesTheCard()
        {
            var cow = Card("farm.cow");
            var config = ConfigWithCatalog(cow);
            config.maxHandSize = 3; // starting hand is 3
            var m = Started(config);
            int burned = 0;
            m.EventRaised += e => { if (e.Type == GameEventType.CardBurned) burned++; };
            var player = m.Players[0];
            Assert.AreEqual(3, player.Hand.Count);
            Assert.IsNull(m.AddToHand(player, cow));
            Assert.AreEqual(3, player.Hand.Count);
            Assert.AreEqual(1, burned);
        }

        [Test]
        public void UnknownCardId_Throws()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("typo", effects: AddCard("farm.coww")), 0, 0);
            Assert.Throws<KeyNotFoundException>(() => PassRound(m));
        }
    }
}
