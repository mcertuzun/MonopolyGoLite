using System.Collections.Generic;
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

        MilestoneSystem _milestoneSystem;
        DiceRegenSystem _diceRegenSystem;
        BoardProgressionSystem _boardProgressionSystem;
        DailyLoginSystem _dailyLoginSystem;
        ProgressionDef _progressionDef;

        HeistSystem _heistSystem;
        ShutdownSystem _shutdownSystem;
        ITargetProvider _targetProvider;
        RNG _railroadRng;
        TargetProfile _pendingShutdownTarget;
        bool _awaitingShutdownChoice;

        ISaveService _saveService;

        public event System.Action<RollResult, MoveResult> OnRollComplete;
        public event System.Action<TileResolveResult> OnTileResolved;
        public event System.Action<ColorGroup, int> OnLandmarkUpgraded;
        public event System.Action OnBoardComplete;

        public event System.Action<List<int>> OnMilestonesReached;
        public event System.Action<string> OnBoardTransition;
        public event System.Action<DailyRewardDef> OnDailyRewardClaimed;
        public event System.Action<int> OnDiceRegenerated;

        public event System.Action<HeistResult, TargetProfile> OnHeistResolved;
        public event System.Action<TargetProfile> OnShutdownStarted;
        public event System.Action<ShutdownResult> OnShutdownResolved;

        const int StartingDice = 100;
        const int DiceCap = 1000;
        const int JailDiceCost = 50;
        const int RngSeed = 12345;

        public void Initialize(string boardId = null)
        {
            _progressionDef = ProgressionConfigLoader.CreateDefault();
            _saveService = new LocalSaveService();
            bool isLoadedFromSave = _saveService.HasSave();

            var progression = new ProgressionState();
            var stats = new PlayerStats();

            if (isLoadedFromSave)
            {
                var save = _saveService.Load();

                if (boardId == null)
                    boardId = _progressionDef.boardOrder[save.currentBoardIndex];

                BoardDef = BoardConfigLoader.Load(boardId);
                State = new GameState(BoardDef, StartingDice, DiceCap, progression, stats);
                SaveAdapter.ApplyToGameState(save, State);
            }
            else
            {
                if (boardId == null)
                    boardId = _progressionDef.boardOrder[progression.CurrentBoardIndex];

                BoardDef = BoardConfigLoader.Load(boardId);
                State = new GameState(BoardDef, StartingDice, DiceCap, progression, stats);
            }

            _diceSystem = new DiceSystem(RngSeed);
            _movementSystem = new MovementSystem();
            _cardSystem = new CardSystem(RngSeed, _movementSystem);
            _jailSystem = new JailSystem(JailDiceCost);
            _landmarkSystem = new LandmarkSystem();
            _tileResolver = new TileResolver(_cardSystem, _jailSystem);

            _milestoneSystem = new MilestoneSystem(_progressionDef.milestones);
            _diceRegenSystem = new DiceRegenSystem();
            _boardProgressionSystem = new BoardProgressionSystem(_progressionDef.boardOrder);
            _dailyLoginSystem = new DailyLoginSystem(_progressionDef.dailyRewards);

            _heistSystem = new HeistSystem(RngSeed + 100);
            _shutdownSystem = new ShutdownSystem();
            _targetProvider = new MockTargetProvider(RngSeed + 200);
            _railroadRng = new RNG((uint)(RngSeed + 300));
            _awaitingShutdownChoice = false;

            // Only apply initial milestones for fresh games (not loaded)
            if (!isLoadedFromSave)
            {
                var initialMilestones = _milestoneSystem.CheckAndApply(State.Player, State.Progression);
                if (initialMilestones.Count > 0)
                    OnMilestonesReached?.Invoke(initialMilestones);
            }

            _diceRegenSystem.ApplyRegen(State.Player, State.Progression, System.DateTime.UtcNow.Ticks);

            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            var dailyReward = _dailyLoginSystem.Claim(State.Player, State.Progression, today);
            if (dailyReward.HasValue)
            {
                OnDailyRewardClaimed?.Invoke(dailyReward.Value);
                AutoSave();
            }
        }

        void AutoSave()
        {
            if (_saveService == null) return;
            var data = SaveAdapter.ToSaveData(State);
            _saveService.Save(data);
        }

        void Update()
        {
            if (State?.Progression == null) return;

            int regenDice = _diceRegenSystem.ApplyRegen(State.Player, State.Progression, System.DateTime.UtcNow.Ticks);
            if (regenDice > 0)
                OnDiceRegenerated?.Invoke(regenDice);
        }

        public void DoRoll()
        {
            if (_awaitingShutdownChoice) return;

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
                    State.Stats.TotalRolls++;
                    AutoSave();
                    if (tileResult.Type == TileResolveType.Railroad)
                        HandleRailroadEvent();
                }
                else
                {
                    _jailSystem.TickJailTurn(State);
                    OnRollComplete?.Invoke(jailRoll, default);
                    State.Stats.TotalRolls++;
                    AutoSave();
                }
                return;
            }

            var roll = _diceSystem.Roll(State.Player);
            if (!roll.Success) return;

            var move = _movementSystem.Move(State, roll.Total);
            OnRollComplete?.Invoke(roll, move);

            var resolve = _tileResolver.Resolve(State);
            OnTileResolved?.Invoke(resolve);
            State.Stats.TotalRolls++;
            if (resolve.Type == TileResolveType.CoinsGained)
                State.Stats.TotalCoinsEarned += resolve.Amount;
            AutoSave();
            if (resolve.Type == TileResolveType.Railroad)
                HandleRailroadEvent();
        }

        public void DoPayJailExit() => _jailSystem.PayToExit(State);

        public void DoUpgradeLandmark(ColorGroup group)
        {
            if (_landmarkSystem.Upgrade(State, group))
            {
                int level = State.Board.GetLandmarkLevel(group);
                OnLandmarkUpgraded?.Invoke(group, level);
                AutoSave();

                if (State.Progression != null)
                {
                    var milestones = _milestoneSystem.CheckAndApply(State.Player, State.Progression);
                    if (milestones.Count > 0)
                        OnMilestonesReached?.Invoke(milestones);
                }

                if (_landmarkSystem.IsBoardComplete(State))
                {
                    OnBoardComplete?.Invoke();
                    TryTransitionToNextBoard();
                }
            }
        }

        void TryTransitionToNextBoard()
        {
            if (State.Progression == null) return;
            if (!_boardProgressionSystem.HasNextBoard(State.Progression.CurrentBoardIndex)) return;

            string nextBoardId = _boardProgressionSystem.GetNextBoardId(State.Progression.CurrentBoardIndex);
            _boardProgressionSystem.AdvanceBoard(State.Progression);

            BoardDef = BoardConfigLoader.Load(nextBoardId);
            State.TransitionToBoard(BoardDef);

            int newSeed = RngSeed + State.Progression.CurrentBoardIndex * 1000;
            _cardSystem = new CardSystem(newSeed, _movementSystem);
            _tileResolver = new TileResolver(_cardSystem, _jailSystem);

            State.Stats.BoardsCompleted++;
            OnBoardTransition?.Invoke(nextBoardId);
        }

        public void SetMultiplier(int value)
        {
            if (State.Progression != null && !State.Progression.IsMultiplierUnlocked(value))
                return;
            State.Player.Multiplier = value;
        }

        public List<int> GetUnlockedMultipliers()
        {
            return State.Progression?.UnlockedMultipliers
                ?? new List<int> { 1, 2, 5, 10 };
        }

        public bool CanClaimDailyReward()
        {
            if (State.Progression == null) return false;
            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            return _dailyLoginSystem.CanClaim(State.Progression, today);
        }

        public bool CanUpgradeLandmark(ColorGroup group) => _landmarkSystem.CanUpgrade(State, group);
        public int GetUpgradeCost(ColorGroup group) => _landmarkSystem.GetUpgradeCost(State, group);

        void HandleRailroadEvent()
        {
            bool isHeist = _railroadRng.Next(0, 2) == 0;
            var target = _targetProvider.GetRandomTarget(State.Progression?.CurrentBoardIndex ?? 0);

            if (isHeist)
            {
                var result = _heistSystem.Resolve(State.Player.Multiplier, State.BoardDef.boardMultiplier);
                State.Player.AddCoins(result.CoinsEarned);
                State.Stats.HeistsCompleted++;
                State.Stats.TotalCoinsEarned += result.CoinsEarned;
                AutoSave();
                OnHeistResolved?.Invoke(result, target);
            }
            else
            {
                _pendingShutdownTarget = target;
                _awaitingShutdownChoice = true;
                OnShutdownStarted?.Invoke(target);
            }
        }

        public void DoShutdownAttack(ColorGroup chosenLandmark)
        {
            if (_pendingShutdownTarget == null) return;

            var result = _shutdownSystem.Resolve(
                _pendingShutdownTarget, chosenLandmark,
                State.Player.Multiplier, State.BoardDef.boardMultiplier);

            State.Player.AddCoins(result.CoinsEarned);
            State.Stats.ShutdownsDealt++;
            State.Stats.TotalCoinsEarned += result.CoinsEarned;
            AutoSave();
            _pendingShutdownTarget = null;
            _awaitingShutdownChoice = false;
            OnShutdownResolved?.Invoke(result);
        }

        public void DoSkipShutdown()
        {
            _pendingShutdownTarget = null;
            _awaitingShutdownChoice = false;
        }
    }
}
