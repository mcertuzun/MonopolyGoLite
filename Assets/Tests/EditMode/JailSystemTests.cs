using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class JailSystemTests
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
                jailTileIndex = 8,
                landmarks = new LandmarkDef[0],
                chanceCards = new CardDef[0],
                communityChestCards = new CardDef[0],
            };
            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
        }

        // 1. SendToJail_SetsPositionAndTurns
        [Test]
        public void SendToJail_SetsPositionAndTurns()
        {
            var system = new JailSystem(jailDiceCost: 50);

            system.SendToJail(_state);

            Assert.AreEqual(_board.jailTileIndex, _state.Player.Position);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        // 2. IsInJail_TrueWhenTurnsRemain
        [Test]
        public void IsInJail_TrueWhenTurnsRemain()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 2;

            Assert.IsTrue(system.IsInJail(_state));
        }

        // 3. TickJailTurn_DecrementsTurns
        [Test]
        public void TickJailTurn_DecrementsTurns()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 3;

            system.TickJailTurn(_state);

            Assert.AreEqual(2, _state.Player.JailTurnsLeft);
        }

        // 4. TickJailTurn_FreesAfterThreeTurns
        [Test]
        public void TickJailTurn_FreesAfterThreeTurns()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 3;

            system.TickJailTurn(_state);
            system.TickJailTurn(_state);
            system.TickJailTurn(_state);

            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
            Assert.IsFalse(system.IsInJail(_state));
        }

        // 5. PayToExit_ConsumeDiceAndFrees
        [Test]
        public void PayToExit_ConsumeDiceAndFrees()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 3;
            // Player starts with 100 dice

            bool result = system.PayToExit(_state);

            Assert.IsTrue(result);
            Assert.AreEqual(50, _state.Player.Dice);
            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
        }

        // 6. PayToExit_FailsWhenNotEnoughDice
        [Test]
        public void PayToExit_FailsWhenNotEnoughDice()
        {
            var board = new BoardDef
            {
                tiles = new TileDef[32],
                jailTileIndex = 8,
                landmarks = new LandmarkDef[0],
                chanceCards = new CardDef[0],
                communityChestCards = new CardDef[0],
            };
            var state = new GameState(board, startingDice: 30, diceCap: 1000);
            var system = new JailSystem(jailDiceCost: 50);
            state.Player.JailTurnsLeft = 3;

            bool result = system.PayToExit(state);

            Assert.IsFalse(result);
            Assert.AreEqual(30, state.Player.Dice);
            Assert.AreEqual(3, state.Player.JailTurnsLeft);
        }

        // 7. ExitOnDoubles_FreesPlayer
        [Test]
        public void ExitOnDoubles_FreesPlayer()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 2;

            bool result = system.TryExitOnDoubles(_state, isDoubles: true);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
        }

        // 8. ExitOnDoubles_DoesNotFreeOnNonDoubles
        [Test]
        public void ExitOnDoubles_DoesNotFreeOnNonDoubles()
        {
            var system = new JailSystem(jailDiceCost: 50);
            _state.Player.JailTurnsLeft = 2;

            bool result = system.TryExitOnDoubles(_state, isDoubles: false);

            Assert.IsFalse(result);
            Assert.AreEqual(2, _state.Player.JailTurnsLeft);
        }
    }
}
