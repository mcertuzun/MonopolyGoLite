# Phase 2: Economy & Progression — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add progression systems on top of the Phase 1 core loop — Net Worth milestones that unlock dice cap/regen/multiplier tiers, board-to-board progression, time-based dice regeneration, and daily login rewards.

**Architecture:** Extend existing pure C# logic layer with new systems. `ProgressionState` tracks cross-board state (login streak, regen timing, claimed milestones, unlocked multipliers). New logic systems (`MilestoneSystem`, `DiceRegenSystem`, `BoardProgressionSystem`, `DailyLoginSystem`) are pure C# and testable without Unity. `GameController` orchestrates integration.

**Tech Stack:** Unity 6.0.3, C# 9.0, UniTask, NUnit (Unity Test Framework), TextMeshPro

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md` (Phase 2 sections: 2.1–2.5)

**Phase 1 Plan:** `docs/superpowers/plans/2026-03-19-phase1-core-loop.md`

**Agent Team Roles for This Phase:**
- **Game Designer** — milestone thresholds, daily reward values, Paris board balance
- **Unity Architect** — state model extensions, system boundaries
- **Senior Developer** — implementation
- **Code Reviewer** — quality gate after each chunk

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   └── MonopolyLite/
│       ├── Data/
│       │   ├── MilestoneDef.cs              # Struct: nwThreshold, diceCap, diceRegenSeconds, unlockedMultiplier
│       │   ├── DailyRewardDef.cs            # Struct: day, coins, dice
│       │   └── ProgressionDef.cs            # Class: milestones[], dailyRewards[], boardOrder[]
│       ├── State/
│       │   └── ProgressionState.cs          # Class: currentBoardIndex, loginStreak, lastLoginDate, lastRegenTicks, diceRegenSeconds, claimedMilestones, unlockedMultipliers
│       ├── Logic/
│       │   ├── MilestoneSystem.cs           # Check NW vs milestones, apply effects (cap, regen rate, multiplier unlock)
│       │   ├── DiceRegenSystem.cs           # Time-based dice regeneration (online tick + offline catchup)
│       │   ├── BoardProgressionSystem.cs    # Board completion → load next board, reset board state
│       │   └── DailyLoginSystem.cs          # Login streak tracking (7-day cycle), reward claiming
│       └── Config/
│           └── ProgressionConfigLoader.cs   # Static defaults: milestones, daily rewards, board order
└── Tests/
    └── EditMode/
        ├── ProgressionStateTests.cs         # ProgressionState init, milestone tracking, multiplier list
        ├── MilestoneSystemTests.cs          # NW threshold checks, cap/regen/multiplier updates
        ├── DiceRegenSystemTests.cs          # Time-based regen, offline catchup, cap respect
        ├── BoardProgressionSystemTests.cs   # Board transition, state reset, edge cases
        └── DailyLoginSystemTests.cs         # Streak logic, reward claiming, day gaps
```

### Existing Files (Modify)

| File | Action | Changes |
|---|---|---|
| `Assets/Scripts/MonopolyLite/State/PlayerState.cs` | MODIFY | Add `SetDiceCap(int cap)` method |
| `Assets/Scripts/MonopolyLite/State/GameState.cs` | MODIFY | Add `ProgressionState Progression` property, `TransitionToBoard(BoardDef)` method |
| `Assets/Scripts/MonopolyLite/Config/BoardConfigLoader.cs` | MODIFY | Add `CreateParis()` default, boardId switch in fallback |
| `Assets/Scripts/MonopolyLite/Core/GameController.cs` | MODIFY | Integrate all new systems, add `Update()` for regen tick, board transition |
| `Assets/Scripts/MonopolyLite/View/HUDView.cs` | MODIFY | Gate multiplier by unlocked list, show regen timer |
| `Assets/Scripts/MonopolyLite/View/UIManager.cs` | MODIFY | Board transition flow, daily login popup |

---

## Task 1: Data Definitions

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Data/MilestoneDef.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/DailyRewardDef.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/ProgressionDef.cs`

- [ ] **Step 1: Create MilestoneDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct MilestoneDef
    {
        public int nwThreshold;        // Net Worth required to trigger
        public int diceCap;            // New dice cap (0 = no change)
        public int diceRegenSeconds;   // Seconds per dice regen (0 = no change)
        public int unlockedMultiplier; // New multiplier tier unlocked (0 = none)
    }
}
```

- [ ] **Step 2: Create DailyRewardDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct DailyRewardDef
    {
        public int day;   // 1-7
        public int coins;
        public int dice;
    }
}
```

- [ ] **Step 3: Create ProgressionDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class ProgressionDef
    {
        public MilestoneDef[] milestones;
        public DailyRewardDef[] dailyRewards;
        public string[] boardOrder;
    }
}
```

- [ ] **Step 4: Create .meta files and verify compilation**

Run: Open Unity Editor or run `dotnet build` equivalent — confirm no compile errors.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Data/MilestoneDef.cs Assets/Scripts/MonopolyLite/Data/DailyRewardDef.cs Assets/Scripts/MonopolyLite/Data/ProgressionDef.cs
git commit -m "feat(phase2): add MilestoneDef, DailyRewardDef, ProgressionDef data structs"
```

---

## Task 2: ProgressionState + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/State/ProgressionState.cs`
- Create: `Assets/Tests/EditMode/ProgressionStateTests.cs`

- [ ] **Step 1: Write failing tests for ProgressionState**

```csharp
using NUnit.Framework;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class ProgressionStateTests
    {
        [Test]
        public void Constructor_DefaultValues()
        {
            var state = new ProgressionState();

            Assert.AreEqual(0, state.CurrentBoardIndex);
            Assert.AreEqual(0, state.LoginStreak);
            Assert.IsNull(state.LastLoginDate);
            Assert.AreEqual(0L, state.LastRegenTicks);
            Assert.AreEqual(300, state.DiceRegenSeconds);
            Assert.AreEqual(0, state.ClaimedMilestones.Count);
            Assert.AreEqual(1, state.UnlockedMultipliers.Count);
            Assert.IsTrue(state.UnlockedMultipliers.Contains(1));
        }

        [Test]
        public void Constructor_CustomRegenRate()
        {
            var state = new ProgressionState(diceRegenSeconds: 180);

            Assert.AreEqual(180, state.DiceRegenSeconds);
        }

        [Test]
        public void IsMultiplierUnlocked_TrueForDefault()
        {
            var state = new ProgressionState();

            Assert.IsTrue(state.IsMultiplierUnlocked(1));
        }

        [Test]
        public void IsMultiplierUnlocked_FalseForLocked()
        {
            var state = new ProgressionState();

            Assert.IsFalse(state.IsMultiplierUnlocked(2));
            Assert.IsFalse(state.IsMultiplierUnlocked(5));
            Assert.IsFalse(state.IsMultiplierUnlocked(10));
        }

        [Test]
        public void UnlockedMultipliers_AddNewTier()
        {
            var state = new ProgressionState();

            state.UnlockedMultipliers.Add(2);

            Assert.IsTrue(state.IsMultiplierUnlocked(2));
            Assert.AreEqual(2, state.UnlockedMultipliers.Count);
        }

        [Test]
        public void ClaimedMilestones_TrackIndices()
        {
            var state = new ProgressionState();

            state.ClaimedMilestones.Add(0);
            state.ClaimedMilestones.Add(2);

            Assert.IsTrue(state.ClaimedMilestones.Contains(0));
            Assert.IsFalse(state.ClaimedMilestones.Contains(1));
            Assert.IsTrue(state.ClaimedMilestones.Contains(2));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → ProgressionStateTests
Expected: FAIL — `ProgressionState` class does not exist

- [ ] **Step 3: Implement ProgressionState**

```csharp
using System.Collections.Generic;

namespace MonopolyLite.State
{
    public class ProgressionState
    {
        public int CurrentBoardIndex { get; set; }
        public int LoginStreak { get; set; }
        public string LastLoginDate { get; set; }
        public long LastRegenTicks { get; set; }
        public int DiceRegenSeconds { get; set; }
        public HashSet<int> ClaimedMilestones { get; private set; }
        public List<int> UnlockedMultipliers { get; private set; }

        public ProgressionState(int diceRegenSeconds = 300)
        {
            CurrentBoardIndex = 0;
            LoginStreak = 0;
            LastLoginDate = null;
            LastRegenTicks = 0;
            DiceRegenSeconds = diceRegenSeconds;
            ClaimedMilestones = new HashSet<int>();
            UnlockedMultipliers = new List<int> { 1 };
        }

        public bool IsMultiplierUnlocked(int multiplier)
        {
            return UnlockedMultipliers.Contains(multiplier);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → ProgressionStateTests
Expected: All 6 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/State/ProgressionState.cs Assets/Tests/EditMode/ProgressionStateTests.cs
git commit -m "feat(phase2): add ProgressionState with milestone/multiplier tracking"
```

---

## Task 3: State Modifications (PlayerState + GameState)

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/State/PlayerState.cs`
- Modify: `Assets/Scripts/MonopolyLite/State/GameState.cs`
- Modify: `Assets/Tests/EditMode/GameStateTests.cs`

- [ ] **Step 1: Write failing tests for new state methods**

Add these tests to `GameStateTests.cs`. Add `using MonopolyLite.Config;` to the using directives if not already present:

```csharp
// Add at the end of the existing test class

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
public void GameState_TransitionToBoard_ResetsBoardKeepsPlayer()
{
    var board1 = BoardConfigLoader.CreateDefault();
    var progression = new ProgressionState();
    var state = new GameState(board1, startingDice: 100, diceCap: 1000, progression: progression);

    state.Player.AddCoins(5000);
    state.Player.NetWorth = 1500;
    state.Player.Position = 15;
    state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);

    // Create a second board (different landmarks for distinct verification)
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → GameStateTests
Expected: FAIL — `SetDiceCap`, `Progression`, `TransitionToBoard` not defined

- [ ] **Step 3: Add SetDiceCap to PlayerState**

In `Assets/Scripts/MonopolyLite/State/PlayerState.cs`, add after `AddShield`:

```csharp
public void SetDiceCap(int cap)
{
    DiceCap = cap;
}
```

- [ ] **Step 4: Update GameState with Progression and TransitionToBoard**

Replace `Assets/Scripts/MonopolyLite/State/GameState.cs` contents:

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class GameState
    {
        public PlayerState Player { get; }
        public BoardState Board { get; private set; }
        public BoardDef BoardDef { get; private set; }
        public ProgressionState Progression { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap,
                         ProgressionState progression = null)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
            Progression = progression;
        }

        public void TransitionToBoard(BoardDef newBoard)
        {
            BoardDef = newBoard;
            Board = new BoardState(newBoard.landmarks);
            Player.Position = 0;
        }
    }
}
```

- [ ] **Step 5: Run all tests to verify they pass (including existing)**

Run: Unity Test Runner → EditMode → All tests
Expected: All tests PASS (new + existing GameStateTests + all other test suites)

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/MonopolyLite/State/PlayerState.cs Assets/Scripts/MonopolyLite/State/GameState.cs Assets/Tests/EditMode/GameStateTests.cs
git commit -m "feat(phase2): add PlayerState.SetDiceCap, GameState.Progression + TransitionToBoard"
```

---

## Task 4: ProgressionConfigLoader

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Config/ProgressionConfigLoader.cs`

- [ ] **Step 1: Create ProgressionConfigLoader with defaults**

```csharp
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
```

- [ ] **Step 2: Verify compilation**

Run: Confirm no compile errors in Unity.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Config/ProgressionConfigLoader.cs
git commit -m "feat(phase2): add ProgressionConfigLoader with default milestones and daily rewards"
```

---

## Task 5: MilestoneSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/MilestoneSystem.cs`
- Create: `Assets/Tests/EditMode/MilestoneSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MilestoneSystemTests
    {
        MilestoneDef[] _milestones;
        MilestoneSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _milestones = new MilestoneDef[]
            {
                new MilestoneDef { nwThreshold = 0,    diceCap = 1000, diceRegenSeconds = 300, unlockedMultiplier = 1 },
                new MilestoneDef { nwThreshold = 500,  diceCap = 0,    diceRegenSeconds = 0,   unlockedMultiplier = 2 },
                new MilestoneDef { nwThreshold = 2000, diceCap = 1500, diceRegenSeconds = 270, unlockedMultiplier = 5 },
            };
            _system = new MilestoneSystem(_milestones);
            _player = new PlayerState(100, 1000);
            _progression = new ProgressionState();
        }

        // 1. Initial milestone (NW=0) applied on first check
        [Test]
        public void CheckAndApply_InitialMilestone_Applied()
        {
            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1, applied.Count);
            Assert.AreEqual(0, applied[0]);
            Assert.IsTrue(_progression.ClaimedMilestones.Contains(0));
            Assert.AreEqual(300, _progression.DiceRegenSeconds);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(1));
        }

        // 2. No re-claim on second check at same NW
        [Test]
        public void CheckAndApply_NoDuplicateClaim()
        {
            _system.CheckAndApply(_player, _progression);
            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(0, applied.Count);
        }

        // 3. Second milestone triggers at NW >= 500
        [Test]
        public void CheckAndApply_SecondMilestone_UnlocksMultiplier()
        {
            _system.CheckAndApply(_player, _progression); // claim initial
            _player.NetWorth = 500;

            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1, applied.Count);
            Assert.AreEqual(1, applied[0]);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(2));
        }

        // 4. DiceCap=0 means no change
        [Test]
        public void CheckAndApply_ZeroDiceCap_NoChange()
        {
            _system.CheckAndApply(_player, _progression); // sets cap to 1000
            _player.NetWorth = 500;

            _system.CheckAndApply(_player, _progression); // milestone 1: diceCap=0

            Assert.AreEqual(1000, _player.DiceCap); // unchanged
        }

        // 5. Multiple milestones applied in single call
        [Test]
        public void CheckAndApply_MultipleAtOnce()
        {
            _player.NetWorth = 2500; // qualifies for all 3 milestones

            List<int> applied = _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(3, applied.Count);
            Assert.AreEqual(1500, _player.DiceCap);
            Assert.AreEqual(270, _progression.DiceRegenSeconds);
            Assert.IsTrue(_progression.IsMultiplierUnlocked(1));
            Assert.IsTrue(_progression.IsMultiplierUnlocked(2));
            Assert.IsTrue(_progression.IsMultiplierUnlocked(5));
        }

        // 6. DiceCap update propagates to PlayerState
        [Test]
        public void CheckAndApply_DiceCapUpdated()
        {
            _player.NetWorth = 2000;

            _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(1500, _player.DiceCap);
        }

        // 7. DiceRegenSeconds update propagates to ProgressionState
        [Test]
        public void CheckAndApply_RegenRateUpdated()
        {
            _player.NetWorth = 2000;

            _system.CheckAndApply(_player, _progression);

            Assert.AreEqual(270, _progression.DiceRegenSeconds);
        }

        // 8. UnlockedMultiplier=0 means no multiplier unlock
        [Test]
        public void CheckAndApply_ZeroMultiplier_NoUnlock()
        {
            var milestones = new MilestoneDef[]
            {
                new MilestoneDef { nwThreshold = 0, diceCap = 1000, diceRegenSeconds = 300, unlockedMultiplier = 0 },
            };
            var system = new MilestoneSystem(milestones);
            var progression = new ProgressionState();
            var player = new PlayerState(100, 1000);

            system.CheckAndApply(player, progression);

            // Only default multiplier (1) remains
            Assert.AreEqual(1, progression.UnlockedMultipliers.Count);
            Assert.IsTrue(progression.IsMultiplierUnlocked(1));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → MilestoneSystemTests
Expected: FAIL — `MilestoneSystem` class does not exist

- [ ] **Step 3: Implement MilestoneSystem**

```csharp
using System.Collections.Generic;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class MilestoneSystem
    {
        readonly MilestoneDef[] _milestones;

        public MilestoneSystem(MilestoneDef[] milestones)
        {
            _milestones = milestones;
        }

        public List<int> CheckAndApply(PlayerState player, ProgressionState progression)
        {
            var applied = new List<int>();

            for (int i = 0; i < _milestones.Length; i++)
            {
                if (progression.ClaimedMilestones.Contains(i))
                    continue;

                var m = _milestones[i];
                if (player.NetWorth < m.nwThreshold)
                    continue;

                progression.ClaimedMilestones.Add(i);
                applied.Add(i);

                if (m.diceCap > 0)
                    player.SetDiceCap(m.diceCap);

                if (m.diceRegenSeconds > 0)
                    progression.DiceRegenSeconds = m.diceRegenSeconds;

                if (m.unlockedMultiplier > 0 && !progression.IsMultiplierUnlocked(m.unlockedMultiplier))
                    progression.UnlockedMultipliers.Add(m.unlockedMultiplier);
            }

            return applied;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → MilestoneSystemTests
Expected: All 8 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/MilestoneSystem.cs Assets/Tests/EditMode/MilestoneSystemTests.cs
git commit -m "feat(phase2): add MilestoneSystem — NW-gated dice cap, regen rate, multiplier unlocks"
```

---

## Task 6: DiceRegenSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/DiceRegenSystem.cs`
- Create: `Assets/Tests/EditMode/DiceRegenSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class DiceRegenSystemTests
    {
        DiceRegenSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        const long TicksPerSecond = 10_000_000L; // TimeSpan.TicksPerSecond

        [SetUp]
        public void SetUp()
        {
            _system = new DiceRegenSystem();
            _player = new PlayerState(50, 1000);
            _progression = new ProgressionState(diceRegenSeconds: 300); // 5 min per dice
        }

        // 1. First call initializes lastRegenTicks, grants nothing
        [Test]
        public void ApplyRegen_FirstCall_InitializesTime()
        {
            long now = 1000L * TicksPerSecond;

            int granted = _system.ApplyRegen(_player, _progression, now);

            Assert.AreEqual(0, granted);
            Assert.AreEqual(now, _progression.LastRegenTicks);
            Assert.AreEqual(50, _player.Dice);
        }

        // 2. Before one interval elapses, no dice granted
        [Test]
        public void ApplyRegen_BeforeInterval_NoDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 299 * TicksPerSecond);

            Assert.AreEqual(0, granted);
            Assert.AreEqual(50, _player.Dice);
        }

        // 3. Exactly one interval grants 1 dice
        [Test]
        public void ApplyRegen_OneInterval_OneDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 300 * TicksPerSecond);

            Assert.AreEqual(1, granted);
            Assert.AreEqual(51, _player.Dice);
        }

        // 4. Fractional time preserved — 450s = 1 dice, 150s carried forward
        [Test]
        public void ApplyRegen_FractionalTime_Preserved()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            _system.ApplyRegen(_player, _progression, start + 450 * TicksPerSecond);

            Assert.AreEqual(51, _player.Dice);
            // lastRegenTicks advanced by 1 interval (300s), not to current time
            Assert.AreEqual(start + 300 * TicksPerSecond, _progression.LastRegenTicks);
        }

        // 5. Offline catchup — 1800s (30 min) = 6 dice
        [Test]
        public void ApplyRegen_OfflineCatchup_MultipleDice()
        {
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 1800 * TicksPerSecond);

            Assert.AreEqual(6, granted);
            Assert.AreEqual(56, _player.Dice);
        }

        // 6. Respects dice cap
        [Test]
        public void ApplyRegen_RespectsDiceCap()
        {
            _player = new PlayerState(998, 1000); // only 2 space
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 1800 * TicksPerSecond);

            Assert.AreEqual(2, granted);
            Assert.AreEqual(1000, _player.Dice);
        }

        // 7. Already at cap grants nothing
        [Test]
        public void ApplyRegen_AtCap_ZeroDice()
        {
            _player = new PlayerState(1000, 1000);
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 600 * TicksPerSecond);

            Assert.AreEqual(0, granted);
        }

        // 8. Different regen rate (180s) works correctly
        [Test]
        public void ApplyRegen_FasterRate()
        {
            _progression.DiceRegenSeconds = 180;
            long start = 1000L * TicksPerSecond;
            _progression.LastRegenTicks = start;

            int granted = _system.ApplyRegen(_player, _progression, start + 900 * TicksPerSecond);

            Assert.AreEqual(5, granted); // 900 / 180 = 5
            Assert.AreEqual(55, _player.Dice);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → DiceRegenSystemTests
Expected: FAIL — `DiceRegenSystem` class does not exist

- [ ] **Step 3: Implement DiceRegenSystem**

```csharp
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class DiceRegenSystem
    {
        const long TicksPerSecond = 10_000_000L;

        public int ApplyRegen(PlayerState player, ProgressionState progression, long currentTicks)
        {
            if (progression.LastRegenTicks == 0)
            {
                progression.LastRegenTicks = currentTicks;
                return 0;
            }

            long elapsed = currentTicks - progression.LastRegenTicks;
            long intervalTicks = (long)progression.DiceRegenSeconds * TicksPerSecond;
            int diceToGrant = (int)(elapsed / intervalTicks);

            if (diceToGrant <= 0)
                return 0;

            int space = player.DiceCap - player.Dice;
            int granted = System.Math.Min(diceToGrant, space);
            granted = System.Math.Max(granted, 0);

            if (granted > 0)
                player.AddDice(granted);

            // Advance by consumed intervals to preserve fractional time
            progression.LastRegenTicks += diceToGrant * intervalTicks;

            return granted;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → DiceRegenSystemTests
Expected: All 8 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/DiceRegenSystem.cs Assets/Tests/EditMode/DiceRegenSystemTests.cs
git commit -m "feat(phase2): add DiceRegenSystem — time-based dice regen with offline catchup"
```

---

## Task 7: DailyLoginSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/DailyLoginSystem.cs`
- Create: `Assets/Tests/EditMode/DailyLoginSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class DailyLoginSystemTests
    {
        DailyRewardDef[] _rewards;
        DailyLoginSystem _system;
        PlayerState _player;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _rewards = new DailyRewardDef[]
            {
                new DailyRewardDef { day = 1, coins = 100,  dice = 20 },
                new DailyRewardDef { day = 2, coins = 200,  dice = 30 },
                new DailyRewardDef { day = 3, coins = 300,  dice = 40 },
                new DailyRewardDef { day = 4, coins = 500,  dice = 50 },
                new DailyRewardDef { day = 5, coins = 750,  dice = 75 },
                new DailyRewardDef { day = 6, coins = 1000, dice = 100 },
                new DailyRewardDef { day = 7, coins = 2000, dice = 200 },
            };
            _system = new DailyLoginSystem(_rewards);
            _player = new PlayerState(100, 1000);
            _progression = new ProgressionState();
        }

        // 1. First login — streak starts at 1, day 1 reward
        [Test]
        public void Claim_FirstLogin_Day1Reward()
        {
            var reward = _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day);
            Assert.AreEqual(100, reward.Value.coins);
            Assert.AreEqual(1, _progression.LoginStreak);
            Assert.AreEqual("2026-03-19", _progression.LastLoginDate);
            Assert.AreEqual(100, _player.Coins);
            Assert.AreEqual(120, _player.Dice); // 100 + 20
        }

        // 2. Same day claim returns null (already claimed)
        [Test]
        public void Claim_SameDay_ReturnsNull()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            var reward = _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsNull(reward);
        }

        // 3. Consecutive day — streak increments
        [Test]
        public void Claim_ConsecutiveDay_StreakIncrements()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            var reward = _system.Claim(_player, _progression, "2026-03-20");

            Assert.IsNotNull(reward);
            Assert.AreEqual(2, reward.Value.day);
            Assert.AreEqual(2, _progression.LoginStreak);
        }

        // 4. Gap of 2+ days — streak resets to 1
        [Test]
        public void Claim_GapDays_StreakResets()
        {
            _system.Claim(_player, _progression, "2026-03-19");
            _system.Claim(_player, _progression, "2026-03-20"); // streak = 2

            var reward = _system.Claim(_player, _progression, "2026-03-23"); // 3-day gap

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day); // reset to day 1
            Assert.AreEqual(1, _progression.LoginStreak);
        }

        // 5. Full 7-day cycle
        [Test]
        public void Claim_Full7DayCycle()
        {
            for (int i = 0; i < 7; i++)
            {
                string date = $"2026-03-{19 + i:D2}";
                var reward = _system.Claim(_player, _progression, date);
                Assert.IsNotNull(reward);
                Assert.AreEqual(i + 1, reward.Value.day);
            }

            Assert.AreEqual(7, _progression.LoginStreak);
        }

        // 6. Day 8 after full cycle — streak resets to 1
        [Test]
        public void Claim_After7Days_CycleResets()
        {
            for (int i = 0; i < 7; i++)
                _system.Claim(_player, _progression, $"2026-03-{19 + i:D2}");

            var reward = _system.Claim(_player, _progression, "2026-03-26");

            Assert.IsNotNull(reward);
            Assert.AreEqual(1, reward.Value.day);
            Assert.AreEqual(1, _progression.LoginStreak);
        }

        // 7. CanClaim returns true for new day
        [Test]
        public void CanClaim_True_ForNewDay()
        {
            Assert.IsTrue(_system.CanClaim(_progression, "2026-03-19"));
        }

        // 8. CanClaim returns false for same day
        [Test]
        public void CanClaim_False_ForSameDay()
        {
            _system.Claim(_player, _progression, "2026-03-19");

            Assert.IsFalse(_system.CanClaim(_progression, "2026-03-19"));
        }

        // 9. Rewards accumulate on player
        [Test]
        public void Claim_RewardsAccumulate()
        {
            _system.Claim(_player, _progression, "2026-03-19"); // +100 coins, +20 dice
            _system.Claim(_player, _progression, "2026-03-20"); // +200 coins, +30 dice

            Assert.AreEqual(300, _player.Coins);
            Assert.AreEqual(150, _player.Dice); // 100 + 20 + 30
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → DailyLoginSystemTests
Expected: FAIL — `DailyLoginSystem` class does not exist

- [ ] **Step 3: Implement DailyLoginSystem**

```csharp
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class DailyLoginSystem
    {
        readonly DailyRewardDef[] _rewards;

        public DailyLoginSystem(DailyRewardDef[] rewards)
        {
            _rewards = rewards;
        }

        public bool CanClaim(ProgressionState progression, string today)
        {
            return progression.LastLoginDate != today;
        }

        public DailyRewardDef? Claim(PlayerState player, ProgressionState progression, string today)
        {
            if (!CanClaim(progression, today))
                return null;

            if (IsConsecutiveDay(progression.LastLoginDate, today))
            {
                progression.LoginStreak++;
                if (progression.LoginStreak > _rewards.Length)
                    progression.LoginStreak = 1;
            }
            else
            {
                progression.LoginStreak = 1;
            }

            progression.LastLoginDate = today;

            int dayIndex = progression.LoginStreak - 1;
            var reward = _rewards[dayIndex];

            player.AddCoins(reward.coins);
            player.AddDice(reward.dice);

            return reward;
        }

        static bool IsConsecutiveDay(string lastDate, string today)
        {
            if (string.IsNullOrEmpty(lastDate))
                return false;

            if (!System.DateTime.TryParseExact(lastDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var last))
                return false;

            if (!System.DateTime.TryParseExact(today, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var current))
                return false;

            return (current - last).Days == 1;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → DailyLoginSystemTests
Expected: All 9 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/DailyLoginSystem.cs Assets/Tests/EditMode/DailyLoginSystemTests.cs
git commit -m "feat(phase2): add DailyLoginSystem — 7-day streak cycle with escalating rewards"
```

---

## Task 8: BoardProgressionSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/BoardProgressionSystem.cs`
- Create: `Assets/Tests/EditMode/BoardProgressionSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class BoardProgressionSystemTests
    {
        BoardProgressionSystem _system;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _system = new BoardProgressionSystem(
                new string[] { "board_01_istanbul", "board_02_paris", "board_03_tokyo" }
            );
            _progression = new ProgressionState();
        }

        // 1. HasNextBoard true when not at last board
        [Test]
        public void HasNextBoard_True_WhenNotLast()
        {
            Assert.IsTrue(_system.HasNextBoard(_progression.CurrentBoardIndex));
        }

        // 2. HasNextBoard false when at last board
        [Test]
        public void HasNextBoard_False_WhenLast()
        {
            _progression.CurrentBoardIndex = 2;

            Assert.IsFalse(_system.HasNextBoard(_progression.CurrentBoardIndex));
        }

        // 3. GetNextBoardId returns correct board
        [Test]
        public void GetNextBoardId_ReturnsNext()
        {
            Assert.AreEqual("board_02_paris", _system.GetNextBoardId(_progression.CurrentBoardIndex));
        }

        // 4. GetCurrentBoardId returns current board
        [Test]
        public void GetCurrentBoardId_ReturnsCurrent()
        {
            Assert.AreEqual("board_01_istanbul", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));
        }

        // 5. AdvanceBoard increments index
        [Test]
        public void AdvanceBoard_IncrementsIndex()
        {
            _system.AdvanceBoard(_progression);

            Assert.AreEqual(1, _progression.CurrentBoardIndex);
        }

        // 6. AdvanceBoard does not go past last board
        [Test]
        public void AdvanceBoard_StopsAtLast()
        {
            _progression.CurrentBoardIndex = 2;

            bool advanced = _system.AdvanceBoard(_progression);

            Assert.IsFalse(advanced);
            Assert.AreEqual(2, _progression.CurrentBoardIndex);
        }

        // 7. Sequential progression through all boards
        [Test]
        public void AdvanceBoard_SequentialProgression()
        {
            Assert.IsTrue(_system.AdvanceBoard(_progression));
            Assert.AreEqual("board_02_paris", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));

            Assert.IsTrue(_system.AdvanceBoard(_progression));
            Assert.AreEqual("board_03_tokyo", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));

            Assert.IsFalse(_system.AdvanceBoard(_progression)); // no more boards
        }

        // 8. Single board order — no next board available
        [Test]
        public void SingleBoard_NoNext()
        {
            var system = new BoardProgressionSystem(new string[] { "board_01_istanbul" });
            var progression = new ProgressionState();

            Assert.IsFalse(system.HasNextBoard(progression.CurrentBoardIndex));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → BoardProgressionSystemTests
Expected: FAIL — `BoardProgressionSystem` class does not exist

- [ ] **Step 3: Implement BoardProgressionSystem**

```csharp
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class BoardProgressionSystem
    {
        readonly string[] _boardOrder;

        public BoardProgressionSystem(string[] boardOrder)
        {
            _boardOrder = boardOrder;
        }

        public bool HasNextBoard(int currentBoardIndex)
        {
            return currentBoardIndex + 1 < _boardOrder.Length;
        }

        public string GetCurrentBoardId(int currentBoardIndex)
        {
            return _boardOrder[currentBoardIndex];
        }

        public string GetNextBoardId(int currentBoardIndex)
        {
            return _boardOrder[currentBoardIndex + 1];
        }

        public bool AdvanceBoard(ProgressionState progression)
        {
            if (!HasNextBoard(progression.CurrentBoardIndex))
                return false;

            progression.CurrentBoardIndex++;
            return true;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → BoardProgressionSystemTests
Expected: All 8 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/BoardProgressionSystem.cs Assets/Tests/EditMode/BoardProgressionSystemTests.cs
git commit -m "feat(phase2): add BoardProgressionSystem — board-to-board progression"
```

---

## Task 9: Board 02 Paris + BoardConfigLoader Update

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Config/BoardConfigLoader.cs`

- [ ] **Step 1: Read current BoardConfigLoader.cs fully**

Read the complete file to understand `CreateDefault()` structure.

- [ ] **Step 2: Add Paris board default and boardId routing**

Add `CreateParis()` method to `BoardConfigLoader` and update `Load()` fallback to route by boardId:

Update the fallback section in `Load()`:

```csharp
// In the fallback section of Load(), replace:
//   return CreateDefault();
// with:
return CreateDefaultForBoard(boardId);
```

Add these methods:

```csharp
static BoardDef CreateDefaultForBoard(string boardId)
{
    return boardId switch
    {
        "board_02_paris" => CreateParis(),
        _ => CreateDefault(),
    };
}

public static BoardDef CreateParis()
{
    return new BoardDef
    {
        id = "board_02_paris",
        theme = "Paris",
        sideLength = 9f,
        tileSize = 1f,
        jailTileIndex = 8,
        goTileIndex = 0,
        goBonus = 360,  // 200 * 1.8 board multiplier
        boardMultiplier = 1.8f,
        tiles = new TileDef[]
        {
            // Bottom row (left to right)
            new TileDef { name = "GO",               type = TileType.Go,             colorGroup = ColorGroup.None },
            new TileDef { name = "Montmartre",        type = TileType.Property,       colorGroup = ColorGroup.Brown,     baseReward = 90 },
            new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None },
            new TileDef { name = "Le Marais",         type = TileType.Property,       colorGroup = ColorGroup.Brown,     baseReward = 108 },
            new TileDef { name = "Income Tax",        type = TileType.Tax,            colorGroup = ColorGroup.None,      taxAmount = 270 },
            new TileDef { name = "Gare du Nord",      type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 180 },
            new TileDef { name = "Belleville",        type = TileType.Property,       colorGroup = ColorGroup.LightBlue, baseReward = 126 },
            new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None },
            // Left column (bottom to top)
            new TileDef { name = "JAIL",              type = TileType.Jail,           colorGroup = ColorGroup.None },
            new TileDef { name = "Bastille",          type = TileType.Property,       colorGroup = ColorGroup.LightBlue, baseReward = 144 },
            new TileDef { name = "Saint-Germain",     type = TileType.Property,       colorGroup = ColorGroup.Pink,      baseReward = 162 },
            new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None },
            new TileDef { name = "Trocadero",         type = TileType.Property,       colorGroup = ColorGroup.Pink,      baseReward = 180 },
            new TileDef { name = "Gare de Lyon",      type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 180 },
            new TileDef { name = "Pigalle",           type = TileType.Property,       colorGroup = ColorGroup.Orange,    baseReward = 198 },
            new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None },
            // Top row (right to left)
            new TileDef { name = "FREE PARKING",      type = TileType.FreeParking,    colorGroup = ColorGroup.None },
            new TileDef { name = "Opera",             type = TileType.Property,       colorGroup = ColorGroup.Orange,    baseReward = 216 },
            new TileDef { name = "La Defense",        type = TileType.Property,       colorGroup = ColorGroup.Red,       baseReward = 234 },
            new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None },
            new TileDef { name = "Invalides",         type = TileType.Property,       colorGroup = ColorGroup.Red,       baseReward = 252 },
            new TileDef { name = "Gare Montparnasse", type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 180 },
            new TileDef { name = "Latin Quarter",     type = TileType.Property,       colorGroup = ColorGroup.Yellow,    baseReward = 270 },
            new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None },
            // Right column (top to bottom)
            new TileDef { name = "GO TO JAIL",        type = TileType.GoToJail,       colorGroup = ColorGroup.None },
            new TileDef { name = "Ile de la Cite",    type = TileType.Property,       colorGroup = ColorGroup.Yellow,    baseReward = 288 },
            new TileDef { name = "Seine River",       type = TileType.Property,       colorGroup = ColorGroup.Green,     baseReward = 306 },
            new TileDef { name = "Luxury Tax",        type = TileType.Tax,            colorGroup = ColorGroup.None,      taxAmount = 360 },
            new TileDef { name = "Champs-Elysees",    type = TileType.Property,       colorGroup = ColorGroup.Green,     baseReward = 324 },
            new TileDef { name = "Gare Saint-Lazare", type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 180 },
            new TileDef { name = "Louvre",            type = TileType.Property,       colorGroup = ColorGroup.Blue,      baseReward = 342 },
            new TileDef { name = "Eiffel Tower",      type = TileType.Property,       colorGroup = ColorGroup.Blue,      baseReward = 360 },
        },
        landmarks = new LandmarkDef[]
        {
            new LandmarkDef { colorGroup = ColorGroup.Brown,     name = "Sacre-Coeur",        costs = new int[] { 180, 360, 540, 720, 900 },       nwPoints = new int[] { 90, 216, 360, 522, 720 } },
            new LandmarkDef { colorGroup = ColorGroup.LightBlue, name = "Place des Vosges",   costs = new int[] { 270, 540, 810, 1080, 1350 },     nwPoints = new int[] { 135, 324, 540, 783, 1080 } },
            new LandmarkDef { colorGroup = ColorGroup.Pink,      name = "Musee d'Orsay",      costs = new int[] { 360, 720, 1080, 1440, 1800 },    nwPoints = new int[] { 180, 432, 720, 1044, 1440 } },
            new LandmarkDef { colorGroup = ColorGroup.Orange,    name = "Palais Garnier",     costs = new int[] { 450, 900, 1350, 1800, 2250 },    nwPoints = new int[] { 225, 540, 900, 1305, 1800 } },
            new LandmarkDef { colorGroup = ColorGroup.Red,       name = "Notre-Dame",         costs = new int[] { 540, 1080, 1620, 2160, 2700 },   nwPoints = new int[] { 270, 648, 1080, 1566, 2160 } },
            new LandmarkDef { colorGroup = ColorGroup.Yellow,    name = "Arc de Triomphe",    costs = new int[] { 630, 1260, 1890, 2520, 3150 },   nwPoints = new int[] { 315, 756, 1260, 1827, 2520 } },
            new LandmarkDef { colorGroup = ColorGroup.Green,     name = "Versailles",         costs = new int[] { 720, 1440, 2160, 2880, 3600 },   nwPoints = new int[] { 360, 864, 1440, 2088, 2880 } },
            new LandmarkDef { colorGroup = ColorGroup.Blue,      name = "Eiffel Tower",       costs = new int[] { 900, 1800, 2700, 3600, 4500 },   nwPoints = new int[] { 450, 1080, 1800, 2610, 3600 } },
        },
        chanceCards = new CardDef[]
        {
            new CardDef { type = CardType.GainCoins, description = "Cafe tips!",            amount = 270 },
            new CardDef { type = CardType.GainCoins, description = "Street art sale",        amount = 450 },
            new CardDef { type = CardType.LoseCoins, description = "Parking fine",           amount = 180 },
            new CardDef { type = CardType.GainDice,  description = "Metro pass found",       amount = 10 },
            new CardDef { type = CardType.GoToJail,  description = "Caught jaywalking!",     amount = 0, tileIndex = 8 },
            new CardDef { type = CardType.GoToTile,  description = "Advance to GO",          amount = 0, tileIndex = 0 },
            new CardDef { type = CardType.GainShield, description = "Beret of protection",   amount = 1 },
            new CardDef { type = CardType.GainCoins, description = "Boulangerie bonus",      amount = 360 },
            new CardDef { type = CardType.LoseCoins, description = "Restaurant bill",        amount = 270 },
            new CardDef { type = CardType.GainDice,  description = "Bicycle rental refund",  amount = 15 },
        },
        communityChestCards = new CardDef[]
        {
            new CardDef { type = CardType.GainCoins, description = "Wine festival prize",    amount = 360 },
            new CardDef { type = CardType.GainCoins, description = "Art auction earnings",   amount = 540 },
            new CardDef { type = CardType.LoseCoins, description = "Museum donation",        amount = 180 },
            new CardDef { type = CardType.GainDice,  description = "Seine cruise tickets",   amount = 8 },
            new CardDef { type = CardType.GainCoins, description = "Fashion week bonus",     amount = 270 },
            new CardDef { type = CardType.LoseCoins, description = "Champagne expense",      amount = 135 },
            new CardDef { type = CardType.GainShield, description = "Croissant shield",      amount = 1 },
            new CardDef { type = CardType.GoToTile,  description = "Go to Eiffel Tower",     amount = 0, tileIndex = 31 },
            new CardDef { type = CardType.GainDice,  description = "Louvre VIP pass",        amount = 12 },
            new CardDef { type = CardType.GainCoins, description = "Perfume sales",          amount = 180 },
        },
    };
}
```

- [ ] **Step 3: Verify both boards load correctly**

Run: Unity Test Runner → all existing tests still pass. Manually test `BoardConfigLoader.Load("board_02_paris")` returns Paris board.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Config/BoardConfigLoader.cs
git commit -m "feat(phase2): add Paris board (board_02) and boardId-based fallback routing"
```

---

## Task 10: GameController Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Read current GameController.cs**

Read the complete file to understand current structure.

- [ ] **Step 2: Add new system fields and events**

Add the following fields and events to `GameController`:

```csharp
// New system fields (add after existing system fields)
MilestoneSystem _milestoneSystem;
DiceRegenSystem _diceRegenSystem;
BoardProgressionSystem _boardProgressionSystem;
DailyLoginSystem _dailyLoginSystem;
ProgressionDef _progressionDef;

// New events (add after existing events)
public event System.Action<System.Collections.Generic.List<int>> OnMilestonesReached;
public event System.Action<string> OnBoardTransition;  // new boardId
public event System.Action<DailyRewardDef> OnDailyRewardClaimed;
public event System.Action<int> OnDiceRegenerated; // dice granted
```

- [ ] **Step 3: Update Initialize method**

Replace the `Initialize` method:

```csharp
public void Initialize(string boardId = null)
{
    _progressionDef = ProgressionConfigLoader.CreateDefault();

    // Create progression state for new game
    var progression = new ProgressionState();

    if (boardId == null)
        boardId = _progressionDef.boardOrder[progression.CurrentBoardIndex];

    BoardDef = BoardConfigLoader.Load(boardId);
    State = new GameState(BoardDef, StartingDice, DiceCap, progression);

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

    // Apply initial milestones (NW=0)
    var initialMilestones = _milestoneSystem.CheckAndApply(State.Player, State.Progression);
    if (initialMilestones.Count > 0)
        OnMilestonesReached?.Invoke(initialMilestones);

    // Initialize regen timer
    _diceRegenSystem.ApplyRegen(State.Player, State.Progression, System.DateTime.UtcNow.Ticks);

    // Check daily login
    string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
    var dailyReward = _dailyLoginSystem.Claim(State.Player, State.Progression, today);
    if (dailyReward.HasValue)
        OnDailyRewardClaimed?.Invoke(dailyReward.Value);
}
```

- [ ] **Step 4: Add Update method for dice regen**

```csharp
void Update()
{
    if (State?.Progression == null) return;

    int regenDice = _diceRegenSystem.ApplyRegen(State.Player, State.Progression, System.DateTime.UtcNow.Ticks);
    if (regenDice > 0)
        OnDiceRegenerated?.Invoke(regenDice);
}
```

- [ ] **Step 5: Update DoUpgradeLandmark to check milestones and board transition**

Replace the `DoUpgradeLandmark` method:

```csharp
public void DoUpgradeLandmark(ColorGroup group)
{
    if (_landmarkSystem.Upgrade(State, group))
    {
        int level = State.Board.GetLandmarkLevel(group);
        OnLandmarkUpgraded?.Invoke(group, level);

        // Check milestones after NW change
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
```

- [ ] **Step 6: Add board transition method**

```csharp
void TryTransitionToNextBoard()
{
    if (State.Progression == null) return;
    if (!_boardProgressionSystem.HasNextBoard(State.Progression.CurrentBoardIndex)) return;

    string nextBoardId = _boardProgressionSystem.GetNextBoardId(State.Progression.CurrentBoardIndex);
    _boardProgressionSystem.AdvanceBoard(State.Progression);

    BoardDef = BoardConfigLoader.Load(nextBoardId);
    State.TransitionToBoard(BoardDef);

    // Re-create card system with new seed for new board's decks
    int newSeed = RngSeed + State.Progression.CurrentBoardIndex * 1000;
    _cardSystem = new CardSystem(newSeed, _movementSystem);
    _tileResolver = new TileResolver(_cardSystem, _jailSystem);

    OnBoardTransition?.Invoke(nextBoardId);
}
```

- [ ] **Step 7: Update SetMultiplier to validate against unlocked list**

Replace the `SetMultiplier` method:

```csharp
public void SetMultiplier(int value)
{
    if (State.Progression != null && !State.Progression.IsMultiplierUnlocked(value))
        return;
    State.Player.Multiplier = value;
}
```

- [ ] **Step 8: Add convenience accessors for view layer**

```csharp
public System.Collections.Generic.List<int> GetUnlockedMultipliers()
{
    return State.Progression?.UnlockedMultipliers
        ?? new System.Collections.Generic.List<int> { 1, 2, 5, 10 };
}

public bool CanClaimDailyReward()
{
    if (State.Progression == null) return false;
    string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
    return _dailyLoginSystem.CanClaim(State.Progression, today);
}
```

- [ ] **Step 9: Add required using directives**

Add at top of file:

```csharp
using System.Collections.Generic;
using MonopolyLite.Config;  // already present
```

Verify `ProgressionConfigLoader` is accessible (same assembly).

- [ ] **Step 10: Verify compilation and run all existing tests**

Run: Unity Test Runner → EditMode → All tests
Expected: All existing tests PASS (GameController is tested manually, not in edit mode tests)

- [ ] **Step 11: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs
git commit -m "feat(phase2): integrate milestone, regen, progression, daily login into GameController"
```

---

## Task 11: View Updates (HUDView + UIManager)

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/View/HUDView.cs`
- Modify: `Assets/Scripts/MonopolyLite/View/UIManager.cs`

- [ ] **Step 1: Read current HUDView.cs and UIManager.cs**

Read both files completely.

- [ ] **Step 2: Update HUDView multiplier cycling to use unlocked list**

Replace the static `MultiplierCycle` array and `CycleMultiplier` method:

```csharp
// Remove:
// static readonly int[] MultiplierCycle = { 1, 2, 5, 10 };
// int _multiplierIndex = 0;

// Add:
int _multiplierIndex = 0;
```

Replace `CycleMultiplier`:

```csharp
void CycleMultiplier()
{
    var unlocked = _controller.GetUnlockedMultipliers();
    if (unlocked.Count == 0) return;

    _multiplierIndex = (_multiplierIndex + 1) % unlocked.Count;
    int value = unlocked[_multiplierIndex];
    _controller.SetMultiplier(value);
    _multiplierLabel.text = $"{value}x";
}
```

- [ ] **Step 3: Add regen timer and board info to stats panel**

Add a new label field:

```csharp
TextMeshProUGUI _regenLabel;
TextMeshProUGUI _boardLabel;
```

In `BuildStatsPanel`, add after the `_networthLabel` line:

```csharp
_regenLabel = CreateLabel(panel, "RegenLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -138f), new Vector2(240f, 28f));
_boardLabel = CreateLabel(panel, "BoardLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -170f), new Vector2(240f, 28f));
```

Increase the stats panel height from 140 to 210:

```csharp
// Change sizeDelta from (260, 140) to:
new Vector2(260f, 210f)
```

- [ ] **Step 4: Update RefreshStats to show regen and board info**

Replace `RefreshStats`:

```csharp
public void RefreshStats()
{
    if (_controller?.State == null) return;
    var p = _controller.State.Player;
    _diceLabel.text     = $"Dice: {p.Dice} / {p.DiceCap}";
    _coinsLabel.text    = $"Coins: {p.Coins}";
    _shieldsLabel.text  = $"Shields: {p.Shields}/3";
    _networthLabel.text = $"Net Worth: {p.NetWorth}";

    var prog = _controller.State.Progression;
    if (prog != null)
    {
        int regenSec = prog.DiceRegenSeconds;
        _regenLabel.text = $"Regen: 1 / {regenSec / 60}m{regenSec % 60:D2}s";
        _boardLabel.text = $"Board: {_controller.BoardDef.theme ?? "Unknown"}";
    }
    else
    {
        _regenLabel.text = "";
        _boardLabel.text = "";
    }
}
```

- [ ] **Step 5: Subscribe to new events in Initialize**

Add in `Initialize`, after existing subscriptions:

```csharp
controller.OnMilestonesReached += HandleMilestonesReached;
controller.OnDiceRegenerated += HandleDiceRegenerated;
controller.OnBoardTransition += HandleBoardTransition;
controller.OnDailyRewardClaimed += HandleDailyReward;
```

- [ ] **Step 6: Add event handlers to HUDView**

```csharp
void HandleMilestonesReached(System.Collections.Generic.List<int> milestoneIndices)
{
    _statusLabel.text = $"Milestone reached! ({milestoneIndices.Count} new)";
    // Reset multiplier index when new multipliers unlocked
    _multiplierIndex = 0;
    var unlocked = _controller.GetUnlockedMultipliers();
    _multiplierLabel.text = $"{unlocked[0]}x";
    _controller.SetMultiplier(unlocked[0]);
    RefreshStats();
}

void HandleDiceRegenerated(int amount)
{
    RefreshStats();
}

void HandleBoardTransition(string newBoardId)
{
    _statusLabel.text = $"New board: {_controller.BoardDef.theme}!";
    RefreshStats();
}

void HandleDailyReward(DailyRewardDef reward)
{
    _statusLabel.text = $"Daily reward! +{reward.coins} coins, +{reward.dice} dice (Day {reward.day})";
    RefreshStats();
}
```

- [ ] **Step 7: Update UIManager to handle board transition re-render**

Read `UIManager.cs` and add board transition handling. In `Initialize`, subscribe to the new event:

```csharp
controller.OnBoardTransition += HandleBoardTransition;
```

Add the handler (UIManager needs references to BoardRenderer and TokenRenderer — read existing code to see how they're accessed, then implement accordingly):

```csharp
void HandleBoardTransition(string newBoardId)
{
    // Board re-rendering is handled in Bootstrap.cs or via a dedicated method.
    // UIManager refreshes UI state.
    Debug.Log($"[UIManager] Board transition to: {newBoardId}");
}
```

> **Note to implementer:** Full board re-rendering (destroying old tiles, re-rendering new board) requires access to `BoardRenderer` and `TokenRenderer`. These are created in `Bootstrap.cs`. For Phase 2, add a `HandleBoardTransition` in Bootstrap that re-renders. Read `Bootstrap.cs` to understand the exact references needed.

- [ ] **Step 8: Verify in Unity Editor**

Run: Open Unity, enter play mode, verify:
- Stats panel shows regen rate and board name
- Multiplier button only cycles through unlocked multipliers (initially just 1x)
- Daily reward message shows on startup
- Dice count increases over time (regen)

- [ ] **Step 9: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/HUDView.cs Assets/Scripts/MonopolyLite/View/UIManager.cs
git commit -m "feat(phase2): update HUDView with milestone-gated multipliers, regen display, board info"
```

---

## Summary

| Task | Files | Tests | Description |
|---|---|---|---|
| 1 | 3 new | — | MilestoneDef, DailyRewardDef, ProgressionDef data structs |
| 2 | 1 new | 6 | ProgressionState with milestone/multiplier tracking |
| 3 | 2 mod | 5 | PlayerState.SetDiceCap, GameState.Progression + TransitionToBoard |
| 4 | 1 new | — | ProgressionConfigLoader with default milestones/rewards |
| 5 | 1 new | 8 | MilestoneSystem — NW-gated unlocks |
| 6 | 1 new | 8 | DiceRegenSystem — time-based regen with offline catchup |
| 7 | 1 new | 9 | DailyLoginSystem — 7-day streak cycle |
| 8 | 1 new | 8 | BoardProgressionSystem — board-to-board progression |
| 9 | 1 mod | — | Paris board (board_02) + loader routing |
| 10 | 1 mod | — | GameController integration of all systems |
| 11 | 2 mod | — | HUDView/UIManager view updates |

**Total: 8 new files, 5 modified files, 44 new unit tests**
