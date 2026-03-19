using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class DailyLoginSystemTests
    {
        DailyRewardDef[] _rewards;
        DailyLoginSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _rewards = new DailyRewardDef[]
            {
                new DailyRewardDef { day = 1, coins = 100,  dice = 20 },
                new DailyRewardDef { day = 2, coins = 200,  dice = 30 },
                new DailyRewardDef { day = 3, coins = 300,  dice = 40 },
                new DailyRewardDef { day = 4, coins = 500,  dice = 50 },
                new DailyRewardDef { day = 5, coins = 750,  dice = 75 },
                new DailyRewardDef { day = 6, coins = 1000, dice = 100 },
                new DailyRewardDef { day = 7, coins = 2000, dice = 200 },
            };
            _system = new DailyLoginSystem(_rewards);
            _player = new PlayerState(100, 1000);
            _progression = new ProgressionState();
        }

        [Test]
        public void Claim_FirstLogin_Day1Reward()
        {
            var reward = _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day);
            Assert.AreEqual(100, reward.Value.coins);
            Assert.AreEqual(1, _progression.LoginStreak);
            Assert.AreEqual("2026-03-19", _progression.LastLoginDate);
            Assert.AreEqual(100, _player.Coins);
            Assert.AreEqual(120, _player.Dice);
        }

        [Test]
        public void Claim_SameDay_ReturnsNull()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            var reward = _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsNull(reward);
        }

        [Test]
        public void Claim_ConsecutiveDay_StreakIncrements()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            var reward = _system.Claim(_player, _progression, "2026-03-20");

            Assert.IsNotNull(reward);
            Assert.AreEqual(2, reward.Value.day);
            Assert.AreEqual(2, _progression.LoginStreak);
        }

        [Test]
        public void Claim_GapDays_StreakResets()
        {
            _system.Claim(_player, _progression, "2026-03-19");
            _system.Claim(_player, _progression, "2026-03-20");

            var reward = _system.Claim(_player, _progression, "2026-03-23");

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day);
            Assert.AreEqual(1, _progression.LoginStreak);
        }

        [Test]
        public void Claim_Full7DayCycle()
        {
            for (int i = 0; i < 7; i++)
            {
                string date = $"2026-03-{19 + i:D2}";
                var reward = _system.Claim(_player, _progression, date);
                Assert.IsNotNull(reward);
                Assert.AreEqual(i + 1, reward.Value.day);
            }

            Assert.AreEqual(7, _progression.LoginStreak);
        }

        [Test]
        public void Claim_After7Days_CycleResets()
        {
            for (int i = 0; i < 7; i++)
                _system.Claim(_player, _progression, $"2026-03-{19 + i:D2}");

            var reward = _system.Claim(_player, _progression, "2026-03-26");

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day);
            Assert.AreEqual(1, _progression.LoginStreak);
        }

        [Test]
        public void CanClaim_True_ForNewDay()
        {
            Assert.IsTrue(_system.CanClaim(_progression, "2026-03-19"));
        }

        [Test]
        public void CanClaim_False_ForSameDay()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsFalse(_system.CanClaim(_progression, "2026-03-19"));
        }

        [Test]
        public void Claim_RewardsAccumulate()
        {
            _system.Claim(_player, _progression, "2026-03-19");
            _system.Claim(_player, _progression, "2026-03-20");

            Assert.AreEqual(300, _player.Coins);
            Assert.AreEqual(150, _player.Dice);
        }
    }
}
