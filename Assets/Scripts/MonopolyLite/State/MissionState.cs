using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class MissionProgress
    {
        public MissionType Type { get; set; }
        public string Description { get; set; }
        public int Target { get; set; }
        public int Progress { get; set; }
        public int CoinReward { get; set; }
        public int DiceReward { get; set; }
        public bool Completed => Progress >= Target;
    }

    public class MissionState
    {
        public string Date { get; set; }
        public MissionProgress[] Missions { get; set; }
        public bool BonusClaimed { get; set; }

        public MissionState()
        {
            Date = null;
            Missions = new MissionProgress[0];
            BonusClaimed = false;
        }
    }
}
