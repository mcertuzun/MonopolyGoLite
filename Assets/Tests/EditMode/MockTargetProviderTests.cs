using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MockTargetProviderTests
    {
        MockTargetProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _provider = new MockTargetProvider(42);
        }

        [Test]
        public void GetRandomTarget_ReturnsNonNull()
        {
            var target = _provider.GetRandomTarget(0);
            Assert.IsNotNull(target);
        }

        [Test]
        public void GetRandomTarget_HasDisplayName()
        {
            var target = _provider.GetRandomTarget(0);
            Assert.IsNotNull(target.displayName);
            Assert.IsNotEmpty(target.displayName);
        }

        [Test]
        public void GetRandomTarget_ShieldsInRange()
        {
            for (int i = 0; i < 20; i++)
            {
                var target = _provider.GetRandomTarget(0);
                Assert.GreaterOrEqual(target.shields, 0);
                Assert.LessOrEqual(target.shields, 3);
            }
        }

        [Test]
        public void GetRandomTarget_HasLandmarks()
        {
            var target = _provider.GetRandomTarget(0);
            Assert.IsNotNull(target.landmarks);
            Assert.Greater(target.landmarks.Length, 0);
        }

        [Test]
        public void GetRandomTarget_LandmarkLevelsInRange()
        {
            var target = _provider.GetRandomTarget(0);
            foreach (var lm in target.landmarks)
            {
                Assert.GreaterOrEqual(lm.level, 0);
                Assert.LessOrEqual(lm.level, 5);
            }
        }

        [Test]
        public void GetRandomTarget_NetWorthPositive()
        {
            var target = _provider.GetRandomTarget(0);
            Assert.GreaterOrEqual(target.netWorth, 0);
        }

        [Test]
        public void GetRandomTarget_DeterministicWithSameSeed()
        {
            var p1 = new MockTargetProvider(99);
            var p2 = new MockTargetProvider(99);
            var t1 = p1.GetRandomTarget(0);
            var t2 = p2.GetRandomTarget(0);
            Assert.AreEqual(t1.displayName, t2.displayName);
            Assert.AreEqual(t1.shields, t2.shields);
            Assert.AreEqual(t1.netWorth, t2.netWorth);
        }
    }
}
