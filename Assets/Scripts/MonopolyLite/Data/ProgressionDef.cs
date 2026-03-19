using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class ProgressionDef
    {
        public MilestoneDef[] milestones;
        public DailyRewardDef[] dailyRewards;
        public string[] boardOrder;
    }
}
