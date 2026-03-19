using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MovementSystemTests
    {
        BoardDef _board;
        GameState _state;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardDef
            {
                tiles = new TileDef[32],
                goTileIndex = 0,
                goBonus = 200,
                landmarks = new LandmarkDef[0]
            };
            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
        }

        // 1. Move_AdvancesPosition
        [Test]
        public void Move_AdvancesPosition()
        {
            var system = new MovementSystem();
            _state.Player.Position = 5;

            var result = system.Move(_state, 4);

            Assert.AreEqual(9, _state.Player.Position);
            Assert.AreEqual(9, result.LandedTileIndex);
            Assert.IsFalse(result.PassedGo);
        }

        // 2. Move_WrapsAroundBoard
        [Test]
        public void Move_WrapsAroundBoard()
        {
            var system = new MovementSystem();
            _state.Player.Position = 30;

            var result = system.Move(_state, 5);

            Assert.AreEqual(3, _state.Player.Position);
            Assert.AreEqual(3, result.LandedTileIndex);
            Assert.IsTrue(result.PassedGo);
        }

        // 3. Move_GrantsGoBonusOnPass
        [Test]
        public void Move_GrantsGoBonusOnPass()
        {
            var system = new MovementSystem();
            _state.Player.Position = 28;

            system.Move(_state, 8);

            // Passed Go: board size 32, 28+8=36, 36%32=4. passedGo = (4 < 28) = true
            Assert.AreEqual(200, _state.Player.Coins);
        }

        // 4. Move_GoBonusScalesWithMultiplier
        [Test]
        public void Move_GoBonusScalesWithMultiplier()
        {
            var system = new MovementSystem();
            _state.Player.Position = 28;
            _state.Player.Multiplier = 3;

            system.Move(_state, 8);

            // goBonus 200 * multiplier 3 = 600
            Assert.AreEqual(600, _state.Player.Coins);
        }

        // 5. MoveToTile_DirectMovement_NoGoBonus
        [Test]
        public void MoveToTile_DirectMovement_NoGoBonus()
        {
            var system = new MovementSystem();
            _state.Player.Position = 10;

            system.MoveToTile(_state, 5, grantGoBonus: false);

            Assert.AreEqual(5, _state.Player.Position);
            Assert.AreEqual(0, _state.Player.Coins);
        }

        // 6. MoveToTile_Forward_WithGoBonus
        [Test]
        public void MoveToTile_Forward_WithGoBonus()
        {
            var system = new MovementSystem();
            _state.Player.Position = 20;

            // Moving to tile 5 from position 20 wraps around (5 < 20), so goBonus is granted
            system.MoveToTile(_state, 5, grantGoBonus: true);

            Assert.AreEqual(5, _state.Player.Position);
            Assert.AreEqual(200, _state.Player.Coins);
        }
    }
}
