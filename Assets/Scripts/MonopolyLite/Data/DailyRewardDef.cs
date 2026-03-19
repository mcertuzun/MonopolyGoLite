using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct DailyRewardDef
    {
        public int day;   // 1-7
        public int coins;
        public int dice;
    }
}
