using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class DarkForestTests
    {
        private static Match DarkVsFarm() => Started(SimpleConfig(Card("x"), teams: new[] { Team.DarkForest, Team.Farm }, boardSize: 8));

        private static void Give(PlayerState p, int ghouls, int bats, int spiders)
        {
            p.Resources.Add(ResourceType.Ghouls, ghouls);
            p.Resources.Add(ResourceType.Bats, bats);
            p.Resources.Add(ResourceType.Spiders, spiders);
        }

        [Test]
        public void EachGhoul_EatsOneBatOrSpider_EveryMorning()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Give(dark, 3, 2, 2);
            PassRound(m);
            Assert.AreEqual(3, dark.Resources.Get(ResourceType.Ghouls));
            Assert.AreEqual(1, dark.Resources.Get(ResourceType.Bats) + dark.Resources.Get(ResourceType.Spiders));
        }

        [Test]
        public void HungryMorning_EatsWhatThereIs_AndExactlyOneGhoulGoesHome()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Give(dark, 5, 1, 1);
            PassRound(m);
            Assert.AreEqual(4, dark.Resources.Get(ResourceType.Ghouls));
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Bats));
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Spiders));
        }

        [Test]
        public void PicnicBlanket_FeedsTwoGhoulsForFree()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Put(m, 0, DarkForestCards.PicnicBlanket, 1, 0);
            Give(dark, 3, 1, 0);
            PassRound(m);
            Assert.AreEqual(3, dark.Resources.Get(ResourceType.Ghouls));
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Bats));
        }

        [Test]
        public void SleepyWeb_SpinsBeforeBreakfast()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Put(m, 0, DarkForestCards.SleepyWeb, 1, 0);
            Give(dark, 2, 0, 0);
            PassRound(m);
            Assert.AreEqual(2, dark.Resources.Get(ResourceType.Ghouls), "the web's 2 Spiders feed both Ghouls");
        }

        [Test]
        public void FullMoonShrine_CountsTouchingHaunts_EveryThirdNight()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Put(m, 0, DarkForestCards.FullMoonShrine, 3, 3);
            Put(m, 0, DarkForestCards.BatRoost, 3, 4);
            var haunt = new CardDefinition { id = "haunt", tags = { DarkForestCards.Haunt } };
            Put(m, 0, haunt, 2, 3);
            Put(m, 0, haunt, 4, 3);
            dark.Resources.Add(ResourceType.Spiders, 100);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Ghouls));
            PassRound(m);
            Assert.AreEqual(2, dark.Resources.Get(ResourceType.Ghouls));
        }

        [Test]
        public void Graveyard_AloneMakesNothing_ButHauntsTogetherDo()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            dark.Resources.Add(ResourceType.Bats, 100);
            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 3);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Ghouls), "a lonely Graveyard makes no Ghouls");

            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 4);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(2, dark.Resources.Get(ResourceType.Ghouls), "two touching Graveyards make 1 Ghoul each");
        }

        [Test]
        public void Lantern_MakesGhoulsForEachTouchingGlowMushrooms()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            dark.Resources.Add(ResourceType.Bats, 100);
            Put(m, 0, DarkForestCards.HauntedLantern, 3, 3);
            Put(m, 0, DarkForestCards.GlowMushrooms, 2, 3);
            Put(m, 0, DarkForestCards.GlowMushrooms, 4, 3);
            PassRound(m);
            PassRound(m);
            Assert.AreEqual(2, dark.Resources.Get(ResourceType.Ghouls));
        }

        [Test]
        public void EvilTree_DoesNothing_AndDeckIs30Cards()
        {
            Assert.IsEmpty(DarkForestCards.EvilTree.effects);
            Assert.AreEqual(30, DarkForestCards.StarterDeck().Count);
            Assert.IsTrue(DarkForestCards.All().All(c => c.team == Team.DarkForest));
        }
    }
}
