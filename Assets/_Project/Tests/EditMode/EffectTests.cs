using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class EffectTests
    {
        [Test]
        public void DayEffects_FireWhenEachDayStarts_ForEveryPlayersCards()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("field", effects: Gain(EffectTrigger.Day, ResourceType.Wheat)), 0, 0);
            Put(m, 1, Card("oak", effects: Gain(EffectTrigger.Day, ResourceType.Acorns)), 5, 5);

            PassRound(m);
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Wheat));
            Assert.AreEqual(1, m.Players[1].Resources.Get(ResourceType.Acorns));
            PassRound(m);
            Assert.AreEqual(2, m.Players[0].Resources.Get(ResourceType.Wheat));
        }

        [Test]
        public void NightEffects_FireAtNightEvenWhenNobodyPlaysAtNight()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("cat", effects: Gain(EffectTrigger.Night, ResourceType.Wheat)), 0, 0);

            m.EndTurn(0);
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Wheat));
            m.EndTurn(1); // day over; night fires with nobody to play, then day 2 begins
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Wheat));
            Assert.AreEqual(2, m.Round);
            Assert.AreEqual(TimeOfDay.Day, m.TimeOfDay);
        }

        [Test]
        public void EveryXDays_Repeats()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("sapling", effects: Gain(EffectTrigger.Day, ResourceType.Acorns, interval: 2)), 0, 0);
            int[] expected = { 0, 1, 1, 2, 2, 3 };
            foreach (int e in expected)
            {
                PassRound(m);
                Assert.AreEqual(e, m.Players[0].Resources.Get(ResourceType.Acorns));
            }
        }

        [Test]
        public void OnceAfterXNights_FiresOnlyOnce()
        {
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, Card("egg", effects: Gain(EffectTrigger.Night, ResourceType.Eggs, 5, interval: 2, repeat: false)), 0, 0);
            PassRound(m);
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Eggs));
            PassRound(m);
            Assert.AreEqual(5, m.Players[0].Resources.Get(ResourceType.Eggs));
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(5, m.Players[0].Resources.Get(ResourceType.Eggs));
        }

        [Test]
        public void IntervalsCount_FromWhenTheCardWasPlayed()
        {
            var m = Started(SimpleConfig(Card("x")));
            PassRound(m);
            PassRound(m);
            Put(m, 0, Card("late", effects: Gain(EffectTrigger.Day, ResourceType.Wheat, interval: 2)), 0, 0);
            PassRound(m);
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Wheat), "first day since placing");
            PassRound(m);
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Wheat), "second day since placing");
        }

        [Test]
        public void OnPlay_DrawCard()
        {
            var barn = Card("barn", effects: new EffectDefinition(EffectTrigger.OnPlay, EffectKind.DrawCard, 2));
            var m = Started(SimpleConfig(barn));
            int before = m.Players[0].Hand.Count;
            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.AreEqual(before - 1 + 2, m.Players[0].Hand.Count);
        }

        [Test]
        public void Convert_SpendsOneResourceForAnother_OnlyIfAffordable()
        {
            var assembler = Card("assembler", effects: new EffectDefinition(EffectTrigger.Day, EffectKind.ConvertResource, 1,
                ResourceType.Goods) { fromResource = ResourceType.Parts, fromAmount = 2 });
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, assembler, 0, 0);
            m.Players[0].Resources.Add(ResourceType.Parts, 3);

            PassRound(m);
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Parts));
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Goods));
            PassRound(m);
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Parts), "can't afford, nothing happens");
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Goods));
        }

        [Test]
        public void GainPerTouching_CountsOnlyYourTaggedNeighbours()
        {
            var coop = Card("coop", effects: new EffectDefinition(EffectTrigger.Day, EffectKind.GainPerTouching, 1,
                ResourceType.Eggs) { tag = "crop" });
            var crop = Card("crop", tags: new[] { "crop" });
            var m = Started(SimpleConfig(Card("x")));
            Put(m, 0, coop, 2, 2);
            Put(m, 0, crop, 1, 2);
            Put(m, 0, crop, 3, 2);
            Put(m, 0, crop, 3, 3); // diagonal: doesn't touch
            Put(m, 0, Card("other"), 2, 3); // touching but not a crop
            Put(m, 1, crop, 2, 1); // touching but not mine

            PassRound(m);
            Assert.AreEqual(2, m.Players[0].Resources.Get(ResourceType.Eggs));
        }

        [Test]
        public void CustomEffect_CanBeRegistered()
        {
            var registry = EffectRegistry.CreateDefault();
            var custom = new CountingEffect();
            registry.Register("count", custom);
            var def = Card("c", effects: new EffectDefinition
                { trigger = EffectTrigger.OnPlay, kind = EffectKind.Custom, customEffectId = "count" });

            var m = new Match(SimpleConfig(def), registry);
            m.Start();
            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.AreEqual(1, custom.Calls);
        }

        private class CountingEffect : IEffect
        {
            public int Calls;
            public void Apply(EffectContext context) => Calls++;
        }
    }
}
