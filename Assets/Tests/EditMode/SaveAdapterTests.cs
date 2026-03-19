using NUnit.Framework;
using MonopolyLite.Config;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class SaveAdapterTests
    {
        GameState _state;

        [SetUp]
        public void SetUp()
        {
            var board = BoardConfigLoader.CreateDefault();
            var progression = new ProgressionState();
            _state = new GameState(board, startingDice: 100, diceCap: 1000, progression: progression);
        }

        [Test]
        public void ToSaveData_CapturesPlayerState()
        {
            _state.Player.AddCoins(5000);
            _state.Player.Shields = 2;
            _state.Player.NetWorth = 1500;
            _state.Player.Position = 10;
            _state.Player.Multiplier = 5;
            _state.Player.JailTurnsLeft = 2;

            var save = SaveAdapter.ToSaveData(_state);

            Assert.AreEqual(5000, save.coins);
            Assert.AreEqual(100, save.dice);
            Assert.AreEqual(1000, save.diceCap);
            Assert.AreEqual(2, save.shields);
            Assert.AreEqual(1500, save.netWorth);
            Assert.AreEqual(10, save.position);
            Assert.AreEqual(5, save.multiplier);
            Assert.AreEqual(2, save.jailTurnsLeft);
        }

        [Test]
        public void ToSaveData_CapturesProgression()
        {
            _state.Progression.CurrentBoardIndex = 1;
            _state.Progression.LoginStreak = 3;
            _state.Progression.LastLoginDate = "2026-03-19";
            _state.Progression.DiceRegenSeconds = 270;
            _state.Progression.ClaimedMilestones.Add(0);
            _state.Progression.ClaimedMilestones.Add(1);
            _state.Progression.UnlockedMultipliers.Add(2);

            var save = SaveAdapter.ToSaveData(_state);

            Assert.AreEqual(1, save.currentBoardIndex);
            Assert.AreEqual(3, save.loginStreak);
            Assert.AreEqual("2026-03-19", save.lastLoginDate);
            Assert.AreEqual(270, save.diceRegenSeconds);
            Assert.IsTrue(System.Array.IndexOf(save.claimedMilestones, 0) >= 0);
            Assert.IsTrue(System.Array.IndexOf(save.claimedMilestones, 1) >= 0);
            Assert.IsTrue(System.Array.IndexOf(save.unlockedMultipliers, 2) >= 0);
        }

        [Test]
        public void ToSaveData_CapturesLandmarks()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 3);
            _state.Board.SetLandmarkLevel(ColorGroup.Blue, 5);

            var save = SaveAdapter.ToSaveData(_state);

            Assert.IsNotNull(save.landmarkLevels);
            Assert.Greater(save.landmarkLevels.Length, 0);

            bool foundBrown = false, foundBlue = false;
            foreach (var entry in save.landmarkLevels)
            {
                if (entry.colorGroup == (int)ColorGroup.Brown) { Assert.AreEqual(3, entry.level); foundBrown = true; }
                if (entry.colorGroup == (int)ColorGroup.Blue) { Assert.AreEqual(5, entry.level); foundBlue = true; }
            }
            Assert.IsTrue(foundBrown);
            Assert.IsTrue(foundBlue);
        }

        [Test]
        public void ToSaveData_CapturesStats()
        {
            _state.Stats.TotalRolls = 50;
            _state.Stats.TotalCoinsEarned = 10000;
            _state.Stats.BoardsCompleted = 1;

            var save = SaveAdapter.ToSaveData(_state);

            Assert.AreEqual(50, save.totalRolls);
            Assert.AreEqual(10000, save.totalCoinsEarned);
            Assert.AreEqual(1, save.boardsCompleted);
        }

        [Test]
        public void ApplyToGameState_RestoresPlayerState()
        {
            var save = new SaveData
            {
                coins = 3000, dice = 80, diceCap = 1500,
                position = 7, shields = 1, netWorth = 800,
                multiplier = 2, jailTurnsLeft = 1,
                claimedMilestones = new int[0],
                unlockedMultipliers = new int[] { 1 },
                landmarkLevels = new LandmarkSaveEntry[0],
            };

            SaveAdapter.ApplyToGameState(save, _state);

            Assert.AreEqual(3000, _state.Player.Coins);
            Assert.AreEqual(80, _state.Player.Dice);
            Assert.AreEqual(1500, _state.Player.DiceCap);
            Assert.AreEqual(7, _state.Player.Position);
            Assert.AreEqual(1, _state.Player.Shields);
            Assert.AreEqual(800, _state.Player.NetWorth);
            Assert.AreEqual(2, _state.Player.Multiplier);
            Assert.AreEqual(1, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void ApplyToGameState_RestoresProgression()
        {
            var save = new SaveData
            {
                currentBoardIndex = 1, loginStreak = 5,
                lastLoginDate = "2026-03-18",
                diceRegenSeconds = 240, lastRegenTicks = 999L,
                claimedMilestones = new int[] { 0, 1, 2 },
                unlockedMultipliers = new int[] { 1, 2, 5 },
                landmarkLevels = new LandmarkSaveEntry[0],
            };

            SaveAdapter.ApplyToGameState(save, _state);

            Assert.AreEqual(1, _state.Progression.CurrentBoardIndex);
            Assert.AreEqual(5, _state.Progression.LoginStreak);
            Assert.AreEqual("2026-03-18", _state.Progression.LastLoginDate);
            Assert.AreEqual(240, _state.Progression.DiceRegenSeconds);
            Assert.IsTrue(_state.Progression.ClaimedMilestones.Contains(2));
            Assert.IsTrue(_state.Progression.IsMultiplierUnlocked(5));
        }

        [Test]
        public void ApplyToGameState_RestoresLandmarks()
        {
            var save = new SaveData
            {
                claimedMilestones = new int[0],
                unlockedMultipliers = new int[] { 1 },
                landmarkLevels = new LandmarkSaveEntry[]
                {
                    new LandmarkSaveEntry { colorGroup = (int)ColorGroup.Brown, level = 4 },
                    new LandmarkSaveEntry { colorGroup = (int)ColorGroup.Blue, level = 2 },
                },
            };

            SaveAdapter.ApplyToGameState(save, _state);

            Assert.AreEqual(4, _state.Board.GetLandmarkLevel(ColorGroup.Brown));
            Assert.AreEqual(2, _state.Board.GetLandmarkLevel(ColorGroup.Blue));
        }

        [Test]
        public void RoundTrip_SaveThenLoad_PreservesState()
        {
            _state.Player.AddCoins(7777);
            _state.Player.Shields = 3;
            _state.Player.NetWorth = 2500;
            _state.Progression.CurrentBoardIndex = 1;
            _state.Progression.LoginStreak = 6;
            _state.Progression.ClaimedMilestones.Add(0);
            _state.Progression.ClaimedMilestones.Add(1);
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            _state.Stats.TotalRolls = 100;

            var save = SaveAdapter.ToSaveData(_state);

            var board = BoardConfigLoader.CreateDefault();
            var freshState = new GameState(board, 100, 1000, new ProgressionState());
            SaveAdapter.ApplyToGameState(save, freshState);

            Assert.AreEqual(7777, freshState.Player.Coins);
            Assert.AreEqual(3, freshState.Player.Shields);
            Assert.AreEqual(2500, freshState.Player.NetWorth);
            Assert.AreEqual(1, freshState.Progression.CurrentBoardIndex);
            Assert.AreEqual(6, freshState.Progression.LoginStreak);
            Assert.IsTrue(freshState.Progression.ClaimedMilestones.Contains(1));
            Assert.AreEqual(5, freshState.Board.GetLandmarkLevel(ColorGroup.Brown));
            Assert.AreEqual(100, freshState.Stats.TotalRolls);
        }
    }
}
