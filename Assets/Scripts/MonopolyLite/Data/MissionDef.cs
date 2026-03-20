using System;
namespace MonopolyLite.Data
{
    [Serializable]
    public struct MissionDef
    {
        public MissionType type;
        public string description;
        public int target;
        public int coinReward;
        public int diceReward;
    }
}
