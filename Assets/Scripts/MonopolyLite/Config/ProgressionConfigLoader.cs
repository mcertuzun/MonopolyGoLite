using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public static class ProgressionConfigLoader
    {
        public static ProgressionDef CreateDefault()
        {
            return new ProgressionDef
            {
                milestones = new MilestoneDef[]
                {
                    new MilestoneDef { nwThreshold = 0,     diceCap = 1000, diceRegenSeconds = 300, unlockedMultiplier = 1 },
                    new MilestoneDef { nwThreshold = 500,   diceCap = 0,    diceRegenSeconds = 0,   unlockedMultiplier = 2 },
                    new MilestoneDef { nwThreshold = 2000,  diceCap = 1500, diceRegenSeconds = 270, unlockedMultiplier = 5 },
                    new MilestoneDef { nwThreshold = 5000,  diceCap = 2000, diceRegenSeconds = 240, unlockedMultiplier = 10 },
                    new MilestoneDef { nwThreshold = 10000, diceCap = 3000, diceRegenSeconds = 210, unlockedMultiplier = 0 },
                    new MilestoneDef { nwThreshold = 25000, diceCap = 5000, diceRegenSeconds = 180, unlockedMultiplier = 0 },
                },
                dailyRewards = new DailyRewardDef[]
                {
                    new DailyRewardDef { day = 1, coins = 100,  dice = 20 },
                    new DailyRewardDef { day = 2, coins = 200,  dice = 30 },
                    new DailyRewardDef { day = 3, coins = 300,  dice = 40 },
                    new DailyRewardDef { day = 4, coins = 500,  dice = 50 },
                    new DailyRewardDef { day = 5, coins = 750,  dice = 75 },
                    new DailyRewardDef { day = 6, coins = 1000, dice = 100 },
                    new DailyRewardDef { day = 7, coins = 2000, dice = 200 },
                },
                boardOrder = new string[] { "board_01_istanbul", "board_02_paris" },
            };
        }
    }
}
