using NUnit.Framework;
using MonopolyLite.State;
using MonopolyLite.Data;
using MonopolyLite.Config;

namespace MonopolyLite.Tests
{
    public class GameStateTests
    {
        static BoardDef MakeBoardDef(LandmarkDef[] landmarks = null)
        {
            return new BoardDef
            {
                id = "test",
                landmarks = landmarks ?? new LandmarkDef[0]
            };
        }

        static LandmarkDef MakeLandmark(ColorGroup group)
        {
            return new LandmarkDef
            {
                colorGroup = group,
                name = group.ToString(),
                costs = new[] { 10, 20, 30, 40, 50 },
                nwPoints = new[] { 1, 2, 3, 4, 5 }
            };
        }

        // 1. NewGameState_HasCorrectDefaults
        [Test]
        public void NewGameState_HasCorrectDefaults()
        {
            var boardDef = MakeBoardDef();
            var state = new GameState(boardDef, startingDice: 30, diceCap: 999);

            Assert.AreEqual(0, state.Player.Coins);
            Assert.AreEqual(30, state.Player.Dice);
            Assert.AreEqual(999, state.Player.DiceCap);
            Assert.AreEqual(0, state.Player.Position);
            Assert.AreEqual(0, state.Player.Shields);
            Assert.AreEqual(1, state.Player.Multiplier);
            Assert.AreEqual(0, state.Player.JailTurnsLeft);
            Assert.AreSame(boardDef, state.BoardDef);
            Assert.IsNotNull(state.Board);
        }

        // 2. PlayerState_AddCoins_IncreasesCoins
        [Test]
        public void PlayerState_AddCoins_IncreasesCoins()
        {
            var player = new PlayerState(30, 999);
            player.AddCoins(100);
            Assert.AreEqual(100, player.Coins);
            player.AddCoins(50);
            Assert.AreEqual(150, player.Coins);
        }

        // 3. PlayerState_SpendCoins_DecreasesCoins
        [Test]
        public void PlayerState_SpendCoins_DecreasesCoins()
        {
            var player = new PlayerState(30, 999);
            player.AddCoins(200);
            bool result = player.SpendCoins(75);
            Assert.IsTrue(result);
            Assert.AreEqual(125, player.Coins);
        }

        // 4. PlayerState_SpendCoins_FailsWhenInsufficient
        [Test]
        public void PlayerState_SpendCoins_FailsWhenInsufficient()
        {
            var player = new PlayerState(30, 999);
            player.AddCoins(50);
            bool result = player.SpendCoins(100);
            Assert.IsFalse(result);
            Assert.AreEqual(50, player.Coins);
        }

        // 5. PlayerState_ConsumeDice_RespectsMultiplier
        [Test]
        public void PlayerState_ConsumeDice_RespectsMultiplier()
        {
            var player = new PlayerState(30, 999);
            player.Multiplier = 3;
            bool result = player.ConsumeDice();
            Assert.IsTrue(result);
            Assert.AreEqual(27, player.Dice);
        }

        // 6. PlayerState_ConsumeDice_FailsWhenInsufficient
        [Test]
        public void PlayerState_ConsumeDice_FailsWhenInsufficient()
        {
            var player = new PlayerState(2, 999);
            player.Multiplier = 5;
            bool result = player.ConsumeDice();
            Assert.IsFalse(result);
            Assert.AreEqual(2, player.Dice);
        }

        // 7. PlayerState_AddDice_RespectsCapFromCap
        [Test]
        public void PlayerState_AddDice_RespectsCapFromCap()
        {
            var player = new PlayerState(990, 1000);
            player.AddDice(50);
            Assert.AreEqual(1000, player.Dice);
        }

        // 8. BoardState_GetSetLandmarkLevel
        [Test]
        public void BoardState_GetSetLandmarkLevel()
        {
            var landmarks = new[] { MakeLandmark(ColorGroup.Brown), MakeLandmark(ColorGroup.Red) };
            var board = new BoardState(landmarks);

            Assert.AreEqual(0, board.GetLandmarkLevel(ColorGroup.Brown));
            board.SetLandmarkLevel(ColorGroup.Brown, 3);
            Assert.AreEqual(3, board.GetLandmarkLevel(ColorGroup.Brown));
            Assert.AreEqual(0, board.GetLandmarkLevel(ColorGroup.Red));
        }

        // 9. BoardState_IsComplete_WhenAllLandmarksMaxLevel
        [Test]
        public void BoardState_IsComplete_WhenAllLandmarksMaxLevel()
        {
            var landmarks = new[] { MakeLandmark(ColorGroup.Brown), MakeLandmark(ColorGroup.Blue) };
            var board = new BoardState(landmarks);

            Assert.IsFalse(board.IsComplete());

            board.SetLandmarkLevel(ColorGroup.Brown, 5);
            Assert.IsFalse(board.IsComplete());

            board.SetLandmarkLevel(ColorGroup.Blue, 5);
            Assert.IsTrue(board.IsComplete());
        }

        [Test]
        public void PlayerState_SetDiceCap_UpdatesCap()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 50, diceCap: 500);

            state.Player.SetDiceCap(2000);

            Assert.AreEqual(2000, state.Player.DiceCap);
        }

        [Test]
        public void PlayerState_AddDice_RespectsNewCap()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 50, diceCap: 100);

            state.Player.SetDiceCap(200);
            state.Player.AddDice(180);

            Assert.AreEqual(200, state.Player.Dice); // 50 + 180 = 230, capped at 200
        }

        [Test]
        public void GameState_Progression_NullByDefault()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 100, diceCap: 1000);

            Assert.IsNull(state.Progression);
        }

        [Test]
        public void GameState_Progression_SetViaConstructor()
        {
            var board = BoardConfigLoader.CreateDefault();
            var progression = new ProgressionState();
            var state = new GameState(board, startingDice: 100, diceCap: 1000, progression: progression);

            Assert.IsNotNull(state.Progression);
            Assert.AreEqual(0, state.Progression.CurrentBoardIndex);
        }

        [Test]
        public void PlayerState_SetCoins_SetsValue()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 100, diceCap: 1000);
            state.Player.SetCoins(5000);
            Assert.AreEqual(5000, state.Player.Coins);
        }

        [Test]
        public void PlayerState_SetDice_SetsValue()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 100, diceCap: 1000);
            state.Player.SetDice(750);
            Assert.AreEqual(750, state.Player.Dice);
        }

        [Test]
        public void PlayerState_SetDice_ClampsToCap()
        {
            var board = BoardConfigLoader.CreateDefault();
            var state = new GameState(board, startingDice: 100, diceCap: 500);
            state.Player.SetDice(999);
            Assert.AreEqual(500, state.Player.Dice);
        }

        [Test]
        public void ProgressionState_LoadMilestones_SetsFromArray()
        {
            var progression = new ProgressionState();
            progression.LoadMilestones(new int[] { 0, 2, 4 });
            Assert.IsTrue(progression.ClaimedMilestones.Contains(0));
            Assert.IsTrue(progression.ClaimedMilestones.Contains(2));
            Assert.IsTrue(progression.ClaimedMilestones.Contains(4));
            Assert.IsFalse(progression.ClaimedMilestones.Contains(1));
            Assert.AreEqual(3, progression.ClaimedMilestones.Count);
        }

        [Test]
        public void ProgressionState_LoadMultipliers_SetsFromArray()
        {
            var progression = new ProgressionState();
            progression.LoadMultipliers(new int[] { 1, 2, 5 });
            Assert.IsTrue(progression.IsMultiplierUnlocked(1));
            Assert.IsTrue(progression.IsMultiplierUnlocked(2));
            Assert.IsTrue(progression.IsMultiplierUnlocked(5));
            Assert.IsFalse(progression.IsMultiplierUnlocked(10));
        }

        [Test]
        public void GameState_TransitionToBoard_ResetsBoardKeepsPlayer()
        {
            var board1 = BoardConfigLoader.CreateDefault();
            var progression = new ProgressionState();
            var state = new GameState(board1, startingDice: 100, diceCap: 1000, progression: progression);

            state.Player.AddCoins(5000);
            state.Player.NetWorth = 1500;
            state.Player.Position = 15;
            state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);

            var board2 = new BoardDef
            {
                tiles = new TileDef[9],
                jailTileIndex = 6,
                goTileIndex = 0,
                goBonus = 300,
                chanceCards = new CardDef[0],
                communityChestCards = new CardDef[0],
                landmarks = new LandmarkDef[]
                {
                    new LandmarkDef
                    {
                        colorGroup = ColorGroup.Pink,
                        name = "Test Landmark",
                        costs = new int[] { 200, 400, 600, 800, 1000 },
                        nwPoints = new int[] { 50, 100, 200, 400, 800 },
                    },
                },
            };

            state.TransitionToBoard(board2);

            // Player state carries over
            Assert.AreEqual(5000, state.Player.Coins);
            Assert.AreEqual(1500, state.Player.NetWorth);
            Assert.AreEqual(0, state.Player.Position); // Reset to 0
            // Board state is fresh
            Assert.AreEqual(0, state.Board.GetLandmarkLevel(ColorGroup.Pink));
            Assert.AreEqual(300, state.BoardDef.goBonus); // New board config
        }
    }
}
