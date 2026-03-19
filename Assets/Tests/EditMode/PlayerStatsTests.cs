using NUnit.Framework;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class PlayerStatsTests
    {
        [Test]
        public void Constructor_AllZero()
        {
            var stats = new PlayerStats();
            Assert.AreEqual(0, stats.TotalRolls);
            Assert.AreEqual(0, stats.TotalCoinsEarned);
            Assert.AreEqual(0, stats.BoardsCompleted);
            Assert.AreEqual(0, stats.HeistsCompleted);
            Assert.AreEqual(0, stats.ShutdownsDealt);
        }

        [Test]
        public void Properties_Settable()
        {
            var stats = new PlayerStats();
            stats.TotalRolls = 50;
            stats.TotalCoinsEarned = 10000;
            stats.BoardsCompleted = 2;
            stats.HeistsCompleted = 15;
            stats.ShutdownsDealt = 8;
            Assert.AreEqual(50, stats.TotalRolls);
            Assert.AreEqual(10000, stats.TotalCoinsEarned);
            Assert.AreEqual(2, stats.BoardsCompleted);
            Assert.AreEqual(15, stats.HeistsCompleted);
            Assert.AreEqual(8, stats.ShutdownsDealt);
        }
    }
}
