using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;
using static FarmVsForest.Tests.TestHelpers;

namespace FarmVsForest.Tests
{
    public class EffectTests
    {
        private static EffectDefinition Fx(EffectTrigger t, EffectKind k, EffectTarget target, int amount, int range = 1) =>
            new EffectDefinition(t, k, target, amount, ResourceType.Food, range);

        private static CardInstance Put(Match m, int playerId, CardDefinition def, int x, int y) =>
            m.Summon(def, m.Players[playerId], new Coord(x, y));

        [Test]
        public void Producer_GivesResourcesAtStartOfOwnersTurn()
        {
            var field = Card("field", effects: Fx(EffectTrigger.OnOwnerTurnStart, EffectKind.GainResource, EffectTarget.Owner, 1));
            var m = Started(SimpleConfig(field));
            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Food));

            m.EndTurn(0);
            Assert.AreEqual(0, m.Players[0].Resources.Get(ResourceType.Food), "doesn't fire on opponent's turn");
            m.EndTurn(1);
            Assert.AreEqual(1, m.Players[0].Resources.Get(ResourceType.Food));
        }

        [Test]
        public void OnPlay_DrawCard()
        {
            var barn = Card("barn", effects: Fx(EffectTrigger.OnPlay, EffectKind.DrawCard, EffectTarget.Owner, 2));
            var m = Started(SimpleConfig(barn));
            int before = m.Players[0].Hand.Count;
            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.AreEqual(before - 1 + 2, m.Players[0].Hand.Count);
        }

        [Test]
        public void AdjacentEnemies_TakeDamage_AlliesDoNot()
        {
            var plain = Card("plain", health: 3);
            var thorns = Card("thorns", health: 3,
                effects: Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1));
            var m = Started(SimpleConfig(plain));

            var mine = Put(m, 0, thorns, 3, 3);
            var ally = Put(m, 0, plain, 3, 4);
            var enemy = Put(m, 1, plain, 4, 4);
            var farEnemy = Put(m, 1, plain, 5, 5);

            m.EndTurn(0);
            Assert.AreEqual(2, enemy.Health);
            Assert.AreEqual(3, ally.Health);
            Assert.AreEqual(3, farEnemy.Health, "out of range");
            Assert.AreEqual(3, mine.Health);
        }

        [Test]
        public void Range2_ReachesFurther()
        {
            var plain = Card("plain", health: 3);
            var turret = Card("turret",
                effects: Fx(EffectTrigger.OnOwnerTurnEnd, EffectKind.Damage, EffectTarget.AdjacentEnemies, 1, range: 2));
            var m = Started(SimpleConfig(plain));
            Put(m, 0, turret, 1, 1);
            var enemy = Put(m, 1, plain, 3, 3);
            m.EndTurn(0);
            Assert.AreEqual(2, enemy.Health);
        }

        [Test]
        public void LethalDamage_DestroysAndDiscards()
        {
            var plain = Card("plain", health: 1);
            var bomb = Card("bomb", effects: Fx(EffectTrigger.OnPlay, EffectKind.Damage, EffectTarget.AdjacentEnemies, 5));
            var m = Started(SimpleConfig(bomb));
            var enemy = Put(m, 1, plain, 1, 1);

            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.IsFalse(enemy.IsOnBoard);
            Assert.IsNull(m.Board.Get(new Coord(1, 1)));
            Assert.Contains(enemy, m.Players[1].Discard);
        }

        [Test]
        public void OnDestroyed_FiresDeathEffect()
        {
            var plain = Card("plain", health: 5);
            var hollow = Card("hollow", health: 1,
                effects: Fx(EffectTrigger.OnDestroyed, EffectKind.Damage, EffectTarget.AdjacentEnemies, 2));
            var m = Started(SimpleConfig(plain));
            var tree = Put(m, 0, hollow, 2, 2);
            var enemy = Put(m, 1, plain, 3, 3);

            m.DealDamage(tree, 1);
            Assert.AreEqual(3, enemy.Health);
        }

        [Test]
        public void Heal_CapsAtMaxHealth()
        {
            var plain = Card("plain", health: 4);
            var m = Started(SimpleConfig(plain));
            var c = Put(m, 0, plain, 0, 0);
            m.DealDamage(c, 1);
            m.Heal(c, 5);
            Assert.AreEqual(4, c.Health);
        }

        [Test]
        public void CustomEffect_CanBeRegistered()
        {
            var registry = EffectRegistry.CreateDefault();
            var custom = new CountingEffect();
            registry.Register("count", custom);
            var def = Card("c", effects: new EffectDefinition
                { trigger = EffectTrigger.OnPlay, kind = EffectKind.Custom, customEffectId = "count", target = EffectTarget.Owner });

            var m = new Match(SimpleConfig(def), registry);
            m.Start();
            m.PlayCard(0, 0, new Coord(0, 0));
            Assert.AreEqual(1, custom.Calls);
        }

        [Test]
        public void DestroyingHomeCard_EliminatesPlayer_AndEndsMatch()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.OneVsOne));
            m.DealDamage(m.Players[1].HomeCard, 999);
            Assert.IsTrue(m.Players[1].IsEliminated);
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(m.Players[0].AllianceId, m.WinningAllianceId);
        }

        [Test]
        public void FreeForAll_EliminatedPlayersAreSkipped()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.FreeForAll));
            m.DealDamage(m.Players[1].HomeCard, 999);
            Assert.IsFalse(m.IsOver);
            m.EndTurn(0);
            Assert.AreEqual(2, m.CurrentPlayer.Id);
        }

        [Test]
        public void TwoVsTwo_TeamWinsWhenBothOpponentsAreOut()
        {
            var m = Started(PlaceholderCards.DefaultConfig(GameMode.TwoVsTwo));
            var enemies = m.Players.Where(p => p.AllianceId == 1).ToList();
            m.DealDamage(enemies[0].HomeCard, 999);
            Assert.IsFalse(m.IsOver);
            m.DealDamage(enemies[1].HomeCard, 999);
            Assert.IsTrue(m.IsOver);
            Assert.AreEqual(0, m.WinningAllianceId);
        }

        private class CountingEffect : IEffect
        {
            public int Calls;
            public void Apply(EffectContext context) => Calls++;
        }
    }
}
