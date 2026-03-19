using System.Collections.Generic;
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MilestoneSystemTests
    {
        MilestoneDef[] _milestones;
        MilestoneSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _milestones = new MilestoneDef[]
            {
                new MilestoneDef { nwThreshold = 0,    diceCap = 1000, diceRegenSeconds = 300, unlockedMultiplier = 1 },
                new MilestoneDef { nwThreshold = 500,  diceCap = 0,    diceRegenSeconds = 0,   unlockedMultiplier = 2 },
                new MilestoneDef { nwThreshold = 2000, diceCap = 1500, diceRegenSeconds = 270, unlockedMultiplier = 5 },
            };
            _system = new MilestoneSystem(_milestones);
            _player = new PlayerState(100, 1000);
            _progression = new ProgressionState();
        }

        [Test]
        public void CheckAndApply_InitialMilestone_Applied()
        {
            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1, applied.Count);
            Assert.AreEqual(0, applied[0]);
            Assert.IsTrue(_progression.ClaimedMilestones.Contains(0));
            Assert.AreEqual(300, _progression.DiceRegenSeconds);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(1));
        }

        [Test]
        public void CheckAndApply_NoDuplicateClaim()
        {
            _system.CheckAndApply(_player, _progression);
            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(0, applied.Count);
        }

        [Test]
        public void CheckAndApply_SecondMilestone_UnlocksMultiplier()
        {
            _system.CheckAndApply(_player, _progression);
            _player.NetWorth = 500;

            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1, applied.Count);
            Assert.AreEqual(1, applied[0]);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(2));
        }

        [Test]
        public void CheckAndApply_ZeroDiceCap_NoChange()
        {
            _system.CheckAndApply(_player, _progression);
            _player.NetWorth = 500;

            _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1000, _player.DiceCap);
        }

        [Test]
        public void CheckAndApply_MultipleAtOnce()
        {
            _player.NetWorth = 2500;

            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(3, applied.Count);
            Assert.AreEqual(1500, _player.DiceCap);
            Assert.AreEqual(270, _progression.DiceRegenSeconds);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(1));
            Assert.IsTrue(_progression.IsMultiplierUnlocked(2));
            Assert.IsTrue(_progression.IsMultiplierUnlocked(5));
        }

        [Test]
        public void CheckAndApply_DiceCapUpdated()
        {
            _player.NetWorth = 2000;

            _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1500, _player.DiceCap);
        }

        [Test]
        public void CheckAndApply_RegenRateUpdated()
        {
            _player.NetWorth = 2000;

            _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(270, _progression.DiceRegenSeconds);
        }

        [Test]
        public void CheckAndApply_ZeroMultiplier_NoUnlock()
        {
            var milestones = new MilestoneDef[]
            {
                new MilestoneDef { nwThreshold = 0, diceCap = 1000, diceRegenSeconds = 300, unlockedMultiplier = 0 },
            };
            var system = new MilestoneSystem(milestones);
            var progression = new ProgressionState();
            var player = new PlayerState(100, 1000);

            system.CheckAndApply(player, progression);

            Assert.AreEqual(1, progression.UnlockedMultipliers.Count);
            Assert.IsTrue(progression.IsMultiplierUnlocked(1));
        }
    }
}
