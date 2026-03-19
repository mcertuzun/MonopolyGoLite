using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class ShutdownSystemTests
    {
        ShutdownSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new ShutdownSystem();
        }

        TargetProfile MakeTarget(int shields)
        {
            return new TargetProfile
            {
                displayName = "TestBot",
                netWorth = 1000,
                shields = shields,
                landmarks = new TargetLandmark[]
                {
                    new TargetLandmark { colorGroup = ColorGroup.Brown, name = "LM1", level = 3 },
                    new TargetLandmark { colorGroup = ColorGroup.Blue,  name = "LM2", level = 5 },
                },
            };
        }

        [Test]
        public void Resolve_Shielded_SmallReward()
        {
            var target = MakeTarget(shields: 2);
            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);
            Assert.IsTrue(result.Shielded);
            Assert.AreEqual(50, result.CoinsEarned);
        }

        [Test]
        public void Resolve_NoShield_LargeReward()
        {
            var target = MakeTarget(shields: 0);
            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);
            Assert.IsFalse(result.Shielded);
            Assert.AreEqual(500, result.CoinsEarned);
        }

        [Test]
        public void Resolve_MultiplierScales()
        {
            var target = MakeTarget(shields: 0);
            var result = _system.Resolve(target, ColorGroup.Blue, 5, 1f);
            Assert.AreEqual(2500, result.CoinsEarned);
        }

        [Test]
        public void Resolve_BoardMultiplierScales()
        {
            var target = MakeTarget(shields: 0);
            var result = _system.Resolve(target, ColorGroup.Blue, 1, 1.8f);
            Assert.AreEqual(900, result.CoinsEarned);
        }

        [Test]
        public void Resolve_ShieldedMultiplierScales()
        {
            var target = MakeTarget(shields: 1);
            var result = _system.Resolve(target, ColorGroup.Brown, 2, 1f);
            Assert.AreEqual(100, result.CoinsEarned);
        }

        [Test]
        public void Resolve_TracksTargetedLandmark()
        {
            var target = MakeTarget(shields: 0);
            var result = _system.Resolve(target, ColorGroup.Blue, 1, 1f);
            Assert.AreEqual(ColorGroup.Blue, result.TargetedLandmark);
        }

        [Test]
        public void Resolve_IncludesTargetName()
        {
            var target = MakeTarget(shields: 0);
            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);
            Assert.AreEqual("TestBot", result.TargetName);
        }
    }
}
