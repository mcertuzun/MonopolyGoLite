using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class HeistSystemTests
    {
        HeistSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new HeistSystem(42);
        }

        [Test]
        public void Resolve_ReturnsPositiveCoins()
        {
            var result = _system.Resolve(1, 1f);
            Assert.Greater(result.CoinsEarned, 0);
        }

        [Test]
        public void Resolve_MultiplierScalesReward()
        {
            var s1 = new HeistSystem(99);
            var s2 = new HeistSystem(99);
            var r1 = s1.Resolve(1, 1f);
            var r2 = s2.Resolve(5, 1f);
            Assert.AreEqual(r1.CoinsEarned * 5, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_BoardMultiplierScalesReward()
        {
            var s1 = new HeistSystem(99);
            var s2 = new HeistSystem(99);
            var r1 = s1.Resolve(1, 1f);
            var r2 = s2.Resolve(1, 2f);
            Assert.AreEqual(r1.CoinsEarned * 2, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_GridHas12Cells()
        {
            var result = _system.Resolve(1, 1f);
            Assert.IsNotNull(result.Grid);
            Assert.AreEqual(12, result.Grid.Length);
        }

        [Test]
        public void Resolve_MatchHas3MatchingSymbolsInGrid()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);
                if (!result.IsMatch) continue;

                int matchCount = 0;
                foreach (var cell in result.Grid)
                    if (cell == result.MatchedSymbol) matchCount++;

                Assert.GreaterOrEqual(matchCount, 3);
                return;
            }
            Assert.Fail("No match found in 100 seeds");
        }

        [Test]
        public void Resolve_MissGivesMinimumReward()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);
                if (result.IsMatch) continue;

                Assert.AreEqual(50, result.CoinsEarned);
                Assert.IsFalse(result.IsMatch);
                return;
            }
            Assert.Fail("No miss found in 200 seeds");
        }

        [Test]
        public void Resolve_DeterministicWithSameSeed()
        {
            var s1 = new HeistSystem(123);
            var s2 = new HeistSystem(123);
            var r1 = s1.Resolve(2, 1.5f);
            var r2 = s2.Resolve(2, 1.5f);
            Assert.AreEqual(r1.IsMatch, r2.IsMatch);
            Assert.AreEqual(r1.MatchedSymbol, r2.MatchedSymbol);
            Assert.AreEqual(r1.CoinsEarned, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_DistributionHasAllOutcomes()
        {
            bool hasCoinBag = false, hasGoldBar = false, hasDiamond = false, hasMiss = false;
            for (int seed = 0; seed < 500; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);
                if (!result.IsMatch) hasMiss = true;
                else if (result.MatchedSymbol == HeistSymbol.CoinBag) hasCoinBag = true;
                else if (result.MatchedSymbol == HeistSymbol.GoldBar) hasGoldBar = true;
                else if (result.MatchedSymbol == HeistSymbol.Diamond) hasDiamond = true;
            }
            Assert.IsTrue(hasCoinBag, "Never got CoinBag");
            Assert.IsTrue(hasGoldBar, "Never got GoldBar");
            Assert.IsTrue(hasDiamond, "Never got Diamond");
            Assert.IsTrue(hasMiss, "Never got Miss");
        }
    }
}
