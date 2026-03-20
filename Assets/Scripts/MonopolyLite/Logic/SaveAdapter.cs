using System.Linq;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public static class SaveAdapter
    {
        public static SaveData ToSaveData(GameState state)
        {
            var data = new SaveData
            {
                saveVersion = 1,
                coins = state.Player.Coins,
                dice = state.Player.Dice,
                diceCap = state.Player.DiceCap,
                position = state.Player.Position,
                shields = state.Player.Shields,
                netWorth = state.Player.NetWorth,
                multiplier = state.Player.Multiplier,
                jailTurnsLeft = state.Player.JailTurnsLeft,
                chanceDrawIndex = state.Board.ChanceDrawIndex,
                communityChestDrawIndex = state.Board.CommunityChestDrawIndex,
            };

            var landmarks = new LandmarkSaveEntry[state.BoardDef.landmarks.Length];
            for (int i = 0; i < state.BoardDef.landmarks.Length; i++)
            {
                var lm = state.BoardDef.landmarks[i];
                landmarks[i] = new LandmarkSaveEntry
                {
                    colorGroup = (int)lm.colorGroup,
                    level = state.Board.GetLandmarkLevel(lm.colorGroup),
                };
            }
            data.landmarkLevels = landmarks;

            if (state.Progression != null)
            {
                data.currentBoardIndex = state.Progression.CurrentBoardIndex;
                data.loginStreak = state.Progression.LoginStreak;
                data.lastLoginDate = state.Progression.LastLoginDate;
                data.lastRegenTicks = state.Progression.LastRegenTicks;
                data.diceRegenSeconds = state.Progression.DiceRegenSeconds;
                data.claimedMilestones = state.Progression.ClaimedMilestones.ToArray();
                data.unlockedMultipliers = state.Progression.UnlockedMultipliers.ToArray();
            }

            if (state.Stats != null)
            {
                data.totalRolls = state.Stats.TotalRolls;
                data.totalCoinsEarned = state.Stats.TotalCoinsEarned;
                data.boardsCompleted = state.Stats.BoardsCompleted;
                data.heistsCompleted = state.Stats.HeistsCompleted;
                data.shutdownsDealt = state.Stats.ShutdownsDealt;
            }

            // Missions
            if (state.MissionState != null)
            {
                data.missionDate = state.MissionState.Date;
                data.missionBonusClaimed = state.MissionState.BonusClaimed;
                if (state.MissionState.Missions != null)
                {
                    data.missions = new MissionSaveEntry[state.MissionState.Missions.Length];
                    for (int i = 0; i < state.MissionState.Missions.Length; i++)
                    {
                        var m = state.MissionState.Missions[i];
                        data.missions[i] = new MissionSaveEntry
                        {
                            type = (int)m.Type,
                            description = m.Description,
                            target = m.Target,
                            progress = m.Progress,
                            coinReward = m.CoinReward,
                            diceReward = m.DiceReward,
                        };
                    }
                }
            }

            // Stickers
            if (state.StickerState != null)
            {
                data.duplicateStars = state.StickerState.DuplicateStars;
                var entries = new System.Collections.Generic.List<StickerSaveEntry>();
                foreach (var kvp in state.StickerState.OwnedStickers)
                {
                    entries.Add(new StickerSaveEntry { stickerId = kvp.Key, count = kvp.Value });
                }
                data.ownedStickers = entries.ToArray();
            }

            return data;
        }

        public static void ApplyToGameState(SaveData data, GameState state)
        {
            state.Player.SetDiceCap(data.diceCap);
            state.Player.SetCoins(data.coins);
            state.Player.SetDice(data.dice);
            state.Player.Position = data.position;
            state.Player.Shields = data.shields;
            state.Player.NetWorth = data.netWorth;
            state.Player.Multiplier = data.multiplier;
            state.Player.JailTurnsLeft = data.jailTurnsLeft;

            if (data.landmarkLevels != null)
            {
                foreach (var entry in data.landmarkLevels)
                    state.Board.SetLandmarkLevel((ColorGroup)entry.colorGroup, entry.level);
            }
            state.Board.ChanceDrawIndex = data.chanceDrawIndex;
            state.Board.CommunityChestDrawIndex = data.communityChestDrawIndex;

            if (state.Progression != null)
            {
                state.Progression.CurrentBoardIndex = data.currentBoardIndex;
                state.Progression.LoginStreak = data.loginStreak;
                state.Progression.LastLoginDate = data.lastLoginDate;
                state.Progression.LastRegenTicks = data.lastRegenTicks;
                state.Progression.DiceRegenSeconds = data.diceRegenSeconds;
                state.Progression.LoadMilestones(data.claimedMilestones ?? new int[0]);
                state.Progression.LoadMultipliers(data.unlockedMultipliers ?? new int[] { 1 });
            }

            if (state.Stats != null)
            {
                state.Stats.TotalRolls = data.totalRolls;
                state.Stats.TotalCoinsEarned = data.totalCoinsEarned;
                state.Stats.BoardsCompleted = data.boardsCompleted;
                state.Stats.HeistsCompleted = data.heistsCompleted;
                state.Stats.ShutdownsDealt = data.shutdownsDealt;
            }

            // Missions
            if (state.MissionState != null)
            {
                state.MissionState.Date = data.missionDate;
                state.MissionState.BonusClaimed = data.missionBonusClaimed;
                if (data.missions != null)
                {
                    state.MissionState.Missions = new MissionProgress[data.missions.Length];
                    for (int i = 0; i < data.missions.Length; i++)
                    {
                        var m = data.missions[i];
                        state.MissionState.Missions[i] = new MissionProgress
                        {
                            Type = (MissionType)m.type,
                            Description = m.description,
                            Target = m.target,
                            Progress = m.progress,
                            CoinReward = m.coinReward,
                            DiceReward = m.diceReward,
                        };
                    }
                }
            }

            // Stickers
            if (state.StickerState != null)
            {
                state.StickerState.DuplicateStars = data.duplicateStars;
                state.StickerState.LoadFromEntries(data.ownedStickers);
            }
        }
    }
}
