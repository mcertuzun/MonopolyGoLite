using MonopolyLite.Config;
using MonopolyLite.Data;
using MonopolyLite.Logic;
using MonopolyLite.State;
using UnityEngine;

namespace MonopolyLite.Core
{
    public class GameController : MonoBehaviour
    {
        public GameState State { get; private set; }
        public BoardDef BoardDef { get; private set; }

        DiceSystem _diceSystem;
        MovementSystem _movementSystem;
        TileResolver _tileResolver;
        CardSystem _cardSystem;
        JailSystem _jailSystem;
        LandmarkSystem _landmarkSystem;

        public event System.Action<RollResult, MoveResult> OnRollComplete;
        public event System.Action<TileResolveResult> OnTileResolved;
        public event System.Action<ColorGroup, int> OnLandmarkUpgraded;
        public event System.Action OnBoardComplete;

        const int StartingDice = 100;
        const int DiceCap = 1000;
        const int JailDiceCost = 50;
        const int RngSeed = 12345;

        public void Initialize(string boardId = "board_01_istanbul")
        {
            BoardDef = BoardConfigLoader.Load(boardId);
            State = new GameState(BoardDef, StartingDice, DiceCap);

            _diceSystem = new DiceSystem(RngSeed);
            _movementSystem = new MovementSystem();
            _cardSystem = new CardSystem(RngSeed, _movementSystem);
            _jailSystem = new JailSystem(JailDiceCost);
            _landmarkSystem = new LandmarkSystem();
            _tileResolver = new TileResolver(_cardSystem, _jailSystem);
        }

        public void DoRoll()
        {
            if (_jailSystem.IsInJail(State))
            {
                var jailRoll = _diceSystem.Roll(State.Player);
                if (!jailRoll.Success) return;

                if (_jailSystem.TryExitOnDoubles(State, jailRoll.IsDoubles))
                {
                    var moveResult = _movementSystem.Move(State, jailRoll.Total);
                    var tileResult = _tileResolver.Resolve(State);
                    OnRollComplete?.Invoke(jailRoll, moveResult);
                    OnTileResolved?.Invoke(tileResult);
                }
                else
                {
                    _jailSystem.TickJailTurn(State);
                    OnRollComplete?.Invoke(jailRoll, default);
                }
                return;
            }

            var roll = _diceSystem.Roll(State.Player);
            if (!roll.Success) return;

            var move = _movementSystem.Move(State, roll.Total);
            OnRollComplete?.Invoke(roll, move);

            var resolve = _tileResolver.Resolve(State);
            OnTileResolved?.Invoke(resolve);
        }

        public void DoPayJailExit() => _jailSystem.PayToExit(State);

        public void DoUpgradeLandmark(ColorGroup group)
        {
            if (_landmarkSystem.Upgrade(State, group))
            {
                int level = State.Board.GetLandmarkLevel(group);
                OnLandmarkUpgraded?.Invoke(group, level);
                if (_landmarkSystem.IsBoardComplete(State))
                    OnBoardComplete?.Invoke();
            }
        }

        public void SetMultiplier(int value) => State.Player.Multiplier = value;
        public bool CanUpgradeLandmark(ColorGroup group) => _landmarkSystem.CanUpgrade(State, group);
        public int GetUpgradeCost(ColorGroup group) => _landmarkSystem.GetUpgradeCost(State, group);
    }
}
