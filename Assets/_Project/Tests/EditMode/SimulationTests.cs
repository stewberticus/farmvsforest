using System.Linq;
using FarmVsForest.Core;
using NUnit.Framework;

namespace FarmVsForest.Tests
{
    /// <summary>Plays whole matches with a naive bot to shake out crashes in the rules.</summary>
    public class SimulationTests
    {
        [Test]
        public void RandomBots_PlayFullMatches_WithoutErrors(
            [Values(GameMode.OneVsOne, GameMode.TwoVsTwo, GameMode.FreeForAll)] GameMode mode,
            [Values(1, 2, 3)] int seed)
        {
            var config = PlaceholderCards.DefaultConfig(mode, seed);
            config.roundLimit = 60;
            var m = new Match(config);
            m.Start();
            var rng = new GameRandom(seed * 31);

            int safety = 0;
            while (!m.IsOver && safety++ < 1000)
            {
                var p = m.CurrentPlayer;
                var options = p.Hand
                    .Where(c => p.Resources.CanAfford(c.Definition.cost))
                    .SelectMany(c => m.LegalSquares(p, c.Definition).Select(sq => (c, sq)))
                    .ToList();
                if (options.Count > 0)
                {
                    var (card, square) = options[rng.Range(0, options.Count)];
                    Assert.AreEqual(PlayResult.Ok, m.PlayCard(p.Id, card, square));
                }
                Assert.IsTrue(m.EndTurn(p.Id));
            }

            Assert.IsTrue(m.IsOver, "match should end by elimination or the round limit");
        }
    }
}
