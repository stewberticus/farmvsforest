using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class DarkForestTests
    {
        /// <summary>Dark Forest (seat 0) vs Farm, with a simple Ghoul recipe for testing: 3 Bats and 3 Spiders, +1 of each per
        /// extra Ghoul in the same turn, no party price, Haunts down to 1 of each.</summary>
        private static Match DarkVsFarm()
        {
            var config = SimpleConfig(Card("x"), teams: new[] { Team.DarkForest, Team.Farm }, boardSize: 8);
            config.darkForestGhoulBats = 3;
            config.darkForestGhoulSpiders = 3;
            config.darkForestGhoulStep = 1;
            config.darkForestGhoulsPerPriceStep = 0;
            config.darkForestGhoulMinEach = 1;
            return Started(config);
        }

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

        /// <summary>Ends turns until it's Dark Forest's (night) turn.</summary>
        private static void ToNight(Match m)
        {
            for (int i = 0; i < 5 && m.CurrentPlayer.Team != Team.DarkForest; i++) m.EndTurn(m.CurrentPlayer.Id);
        }

        private static int Cost(Match m, PlayerState p, ResourceType t) => m.GhoulCost(p).Single(c => c.type == t).amount;

        [Test]
        public void FullMoonShrine_GivesOneGhoul_EveryThirdNight_IfItTouchesAHaunt()
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
            Assert.AreEqual(1, dark.Resources.Get(ResourceType.Ghouls), "two Haunts touching still give just one Ghoul");
        }

        [Test]
        public void Summon_Combines3BatsAnd3Spiders_IntoAGhoul_OnYourTurn()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Give(dark, 0, 3, 3);
            Assert.IsFalse(m.CanSummonGhoul(dark.Id), "not Dark Forest's turn yet");
            ToNight(m);
            Assert.IsTrue(m.SummonGhoul(dark.Id));
            Assert.AreEqual(1, dark.Resources.Get(ResourceType.Ghouls));
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Bats));
            Assert.AreEqual(0, dark.Resources.Get(ResourceType.Spiders));
            Assert.IsFalse(m.CanSummonGhoul(dark.Id));
        }

        [Test]
        public void Summon_EachExtraGhoulInATurn_CostsOneMoreOfEach()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Give(dark, 0, 20, 20);
            ToNight(m);
            Assert.IsTrue(m.SummonGhoul(dark.Id));
            Assert.AreEqual(4, Cost(m, dark, ResourceType.Bats));
            Assert.IsTrue(m.SummonGhoul(dark.Id));
            Assert.AreEqual(20 - 3 - 4, dark.Resources.Get(ResourceType.Spiders));
            m.EndTurn(dark.Id);
            ToNight(m);
            Assert.AreEqual(3, Cost(m, dark, ResourceType.Bats), "the price resets next turn");
        }

        [Test]
        public void Summon_OnlyDarkForest_AndOffWhenTheRecipeIsZero()
        {
            var config = SimpleConfig(Card("x"), teams: new[] { Team.DarkForest, Team.Farm }, boardSize: 8);
            config.darkForestGhoulBats = 0;
            config.darkForestGhoulSpiders = 0;
            var m = Started(config);
            Assert.IsNull(m.GhoulCost(m.Players[0]));
            Assert.IsNull(DarkVsFarm().GhoulCost(m.Players[1]));
        }

        [Test]
        public void Graveyard_MakesGhoulsCheaper_OnlyWhileTouchingAnotherHaunt()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 3);
            Assert.AreEqual(3, Cost(m, dark, ResourceType.Bats), "a lonely Graveyard does nothing");
            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 4);
            // Each Graveyard now touches a Haunt: each takes 1 Bat and 1 Spider off.
            Assert.AreEqual(1, Cost(m, dark, ResourceType.Bats));
            Assert.AreEqual(1, Cost(m, dark, ResourceType.Spiders));
        }

        [Test]
        public void Lantern_MakesGhoulsCheaper_WhileTouchingGlowMushrooms_NeverBelowOneEach()
        {
            var m = DarkVsFarm();
            var dark = m.Players[0];
            Put(m, 0, DarkForestCards.HauntedLantern, 3, 3);
            Assert.AreEqual(3, Cost(m, dark, ResourceType.Bats));
            Put(m, 0, DarkForestCards.GlowMushrooms, 2, 3);
            Assert.AreEqual(2, Cost(m, dark, ResourceType.Bats));
            Assert.AreEqual(2, Cost(m, dark, ResourceType.Spiders));
            // Six more switched-on Lanterns: 7 off, but each part of the price stops at the minimum.
            for (int x = 0; x < 6; x++)
            {
                Put(m, 0, DarkForestCards.HauntedLantern, x, 6);
                Put(m, 0, DarkForestCards.GlowMushrooms, x, 7);
            }
            Assert.AreEqual(1, Cost(m, dark, ResourceType.Bats));
            Assert.AreEqual(1, Cost(m, dark, ResourceType.Spiders));
        }

        [Test]
        public void DefaultRecipe_Is6And6_PlusOneOfEachPerGhoulYouHave()
        {
            var m = Started(SimpleConfig(Card("x"), teams: new[] { Team.DarkForest, Team.Farm }, boardSize: 8));
            var dark = m.Players[0];
            Assert.AreEqual(6, Cost(m, dark, ResourceType.Bats));
            dark.Resources.Add(ResourceType.Ghouls, 4);
            Assert.AreEqual(10, Cost(m, dark, ResourceType.Bats));
            Assert.AreEqual(10, Cost(m, dark, ResourceType.Spiders));
            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 3);
            Put(m, 0, DarkForestCards.GigglingGraveyard, 3, 4);
            Assert.AreEqual(8, Cost(m, dark, ResourceType.Bats), "two switched-on Graveyards take 2 of each off");
        }

        [Test]
        public void CarefulBot_DoesntSummonGhoulsItCantFeed_RandomBotDoes()
        {
            var careful = DarkVsFarm();
            Give(careful.Players[0], 0, 3, 3);
            ToNight(careful);
            Bots.SummonGhouls(careful, careful: true);
            Assert.AreEqual(0, careful.Players[0].Resources.Get(ResourceType.Ghouls), "nothing left to feed it tomorrow");

            var reckless = DarkVsFarm();
            Give(reckless.Players[0], 0, 3, 3);
            ToNight(reckless);
            Bots.SummonGhouls(reckless, careful: false);
            Assert.AreEqual(1, reckless.Players[0].Resources.Get(ResourceType.Ghouls));
            PassRound(reckless);
            Assert.AreEqual(0, reckless.Players[0].Resources.Get(ResourceType.Ghouls), "it went home hungry");
        }

        [Test]
        public void HungryGhoul_RaisesAnEvent()
        {
            var m = DarkVsFarm();
            Give(m.Players[0], 2, 0, 0);
            int home = 0;
            m.EventRaised += e => { if (e.Type == GameEventType.GhoulsWentHome) home += e.Amount; };
            PassRound(m);
            Assert.AreEqual(1, home);
        }

        [Test]
        public void EvilTree_MakesABatEachNight_AndDeckIs30Cards()
        {
            var e = DarkForestCards.EvilTree.effects.Single();
            Assert.AreEqual(EffectTrigger.Night, e.trigger);
            Assert.AreEqual(ResourceType.Bats, e.resource);
            Assert.AreEqual(30, DarkForestCards.StarterDeck().Count);
            Assert.IsTrue(DarkForestCards.All().All(c => c.team == Team.DarkForest));
        }
    }
}
