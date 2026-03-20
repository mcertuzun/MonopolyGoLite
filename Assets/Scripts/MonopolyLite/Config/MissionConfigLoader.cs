using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public static class MissionConfigLoader
    {
        public static MissionDef[] CreateDefaultPool()
        {
            return new MissionDef[]
            {
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 5,    coinReward = 200,  diceReward = 10 },
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 10,   coinReward = 500,  diceReward = 20 },
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 20,   coinReward = 1000, diceReward = 40 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmark(s)",    target = 1,    coinReward = 300,  diceReward = 15 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmarks",      target = 3,    coinReward = 800,  diceReward = 30 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} Bank Heist",   target = 1,    coinReward = 400,  diceReward = 20 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} Bank Heists",  target = 3,    coinReward = 1000, diceReward = 40 },
                new MissionDef { type = MissionType.EarnCoins,     description = "Earn {0} coins",           target = 1000, coinReward = 300,  diceReward = 15 },
                new MissionDef { type = MissionType.EarnCoins,     description = "Earn {0} coins",           target = 5000, coinReward = 750,  diceReward = 30 },
            };
        }
    }
}
