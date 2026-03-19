using System.Collections.Generic;

namespace MonopolyLite.State
{
    public class ProgressionState
    {
        public int CurrentBoardIndex { get; set; }
        public int LoginStreak { get; set; }
        public string LastLoginDate { get; set; }
        public long LastRegenTicks { get; set; }
        public int DiceRegenSeconds { get; set; }
        public HashSet<int> ClaimedMilestones { get; private set; }
        public List<int> UnlockedMultipliers { get; private set; }

        public ProgressionState(int diceRegenSeconds = 300)
        {
            CurrentBoardIndex = 0;
            LoginStreak = 0;
            LastLoginDate = null;
            LastRegenTicks = 0;
            DiceRegenSeconds = diceRegenSeconds;
            ClaimedMilestones = new HashSet<int>();
            UnlockedMultipliers = new List<int> { 1 };
        }

        public bool IsMultiplierUnlocked(int multiplier)
        {
            return UnlockedMultipliers.Contains(multiplier);
        }

        public void LoadMilestones(int[] milestones)
        {
            ClaimedMilestones = new System.Collections.Generic.HashSet<int>(milestones);
        }

        public void LoadMultipliers(int[] multipliers)
        {
            UnlockedMultipliers = new System.Collections.Generic.List<int>(multipliers);
        }
    }
}
