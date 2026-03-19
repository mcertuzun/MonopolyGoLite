using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string lastSavedAt;

        public int coins;
        public int dice;
        public int diceCap;
        public int position;
        public int shields;
        public int netWorth;
        public int multiplier;
        public int jailTurnsLeft;

        public int currentBoardIndex;
        public int loginStreak;
        public string lastLoginDate;
        public long lastRegenTicks;
        public int diceRegenSeconds;
        public int[] claimedMilestones;
        public int[] unlockedMultipliers;

        public LandmarkSaveEntry[] landmarkLevels;
        public int chanceDrawIndex;
        public int communityChestDrawIndex;

        public int totalRolls;
        public int totalCoinsEarned;
        public int boardsCompleted;
        public int heistsCompleted;
        public int shutdownsDealt;
    }
}
