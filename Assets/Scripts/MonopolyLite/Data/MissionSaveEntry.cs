using System;
namespace MonopolyLite.Data
{
    [Serializable]
    public struct MissionSaveEntry
    {
        public int type;
        public string description;
        public int target;
        public int progress;
        public int coinReward;
        public int diceReward;
    }
}
