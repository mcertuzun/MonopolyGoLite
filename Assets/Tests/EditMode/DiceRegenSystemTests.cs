using NUnit.Framework;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class DiceRegenSystemTests
    {
        DiceRegenSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        const long TicksPerSecond = 10_000_000L;

        [SetUp]
        public void SetUp()
        {
            _system = new DiceRegenSystem();
            _player = new PlayerState(50, 1000);
            _progression = new ProgressionState(diceRegenSeconds: 300);
        }

        [Test]
        public void ApplyRegen_FirstCall_InitializesTime()
        {
            long now = 1000L * TicksPerSecond;

            int granted = _system.ApplyRegen(_player, _progression, now);

            Assert.AreEqual(0, granted);
            Assert.AreEqual(now, _progression.LastRegenTicks);
            Assert.AreEqual(50, _player.Dice);
        }

        [Test]
        public void ApplyRegen_BeforeInterval_NoDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 299 * TicksPerSecond);

            Assert.AreEqual(0, granted);
            Assert.AreEqual(50, _player.Dice);
        }

        [Test]
        public void ApplyRegen_OneInterval_OneDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 300 * TicksPerSecond);

            Assert.AreEqual(1, granted);
            Assert.AreEqual(51, _player.Dice);
        }

        [Test]
        public void ApplyRegen_FractionalTime_Preserved()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            _system.ApplyRegen(_player, _progression, start + 450 * TicksPerSecond);

            Assert.AreEqual(51, _player.Dice);
            Assert.AreEqual(start + 300 * TicksPerSecond, _progression.LastRegenTicks);
        }

        [Test]
        public void ApplyRegen_OfflineCatchup_MultipleDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 1800 * TicksPerSecond);

            Assert.AreEqual(6, granted);
            Assert.AreEqual(56, _player.Dice);
        }

        [Test]
        public void ApplyRegen_RespectsDiceCap()
        {
            _player = new PlayerState(998, 1000);
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 1800 * TicksPerSecond);

            Assert.AreEqual(2, granted);
            Assert.AreEqual(1000, _player.Dice);
        }

        [Test]
        public void ApplyRegen_AtCap_ZeroDice()
        {
            _player = new PlayerState(1000, 1000);
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 600 * TicksPerSecond);

            Assert.AreEqual(0, granted);
        }

        [Test]
        public void ApplyRegen_FasterRate()
        {
            _progression.DiceRegenSeconds = 180;
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 900 * TicksPerSecond);

            Assert.AreEqual(5, granted);
            Assert.AreEqual(55, _player.Dice);
        }
    }
}
