using NUnit.Framework;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class ProgressionStateTests
    {
        [Test]
        public void Constructor_DefaultValues()
        {
            var state = new ProgressionState();

            Assert.AreEqual(0, state.CurrentBoardIndex);
            Assert.AreEqual(0, state.LoginStreak);
            Assert.IsNull(state.LastLoginDate);
            Assert.AreEqual(0L, state.LastRegenTicks);
            Assert.AreEqual(300, state.DiceRegenSeconds);
            Assert.AreEqual(0, state.ClaimedMilestones.Count);
            Assert.AreEqual(1, state.UnlockedMultipliers.Count);
            Assert.IsTrue(state.UnlockedMultipliers.Contains(1));
        }

        [Test]
        public void Constructor_CustomRegenRate()
        {
            var state = new ProgressionState(diceRegenSeconds: 180);

            Assert.AreEqual(180, state.DiceRegenSeconds);
        }

        [Test]
        public void IsMultiplierUnlocked_TrueForDefault()
        {
            var state = new ProgressionState();

            Assert.IsTrue(state.IsMultiplierUnlocked(1));
        }

        [Test]
        public void IsMultiplierUnlocked_FalseForLocked()
        {
            var state = new ProgressionState();

            Assert.IsFalse(state.IsMultiplierUnlocked(2));
            Assert.IsFalse(state.IsMultiplierUnlocked(5));
            Assert.IsFalse(state.IsMultiplierUnlocked(10));
        }

        [Test]
        public void UnlockedMultipliers_AddNewTier()
        {
            var state = new ProgressionState();

            state.UnlockedMultipliers.Add(2);

            Assert.IsTrue(state.IsMultiplierUnlocked(2));
            Assert.AreEqual(2, state.UnlockedMultipliers.Count);
        }

        [Test]
        public void ClaimedMilestones_TrackIndices()
        {
            var state = new ProgressionState();

            state.ClaimedMilestones.Add(0);
            state.ClaimedMilestones.Add(2);

            Assert.IsTrue(state.ClaimedMilestones.Contains(0));
            Assert.IsFalse(state.ClaimedMilestones.Contains(1));
            Assert.IsTrue(state.ClaimedMilestones.Contains(2));
        }
    }
}
