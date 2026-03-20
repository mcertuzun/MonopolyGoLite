# Phase 3b: Persistence & Backend Abstraction — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add local save/load persistence so game state survives between sessions, with stats tracking and a backend abstraction layer ready for Firebase swap.

**Architecture:** `SaveData` DTO captures full game state in a JSON-serializable format. `SaveAdapter` converts between live `GameState` and `SaveData`. `ISaveService` interface abstracts persistence (local JSON now, Firebase later). `PlayerStats` tracks gameplay metrics. `GameController` auto-saves after every action and loads on startup.

**Tech Stack:** Unity 6.0.3, C# 9.0, NUnit, UnityEngine.JsonUtility

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md` (Phase 5.3 — Firebase Abstraction Layer, Offline Strategy)

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   └── MonopolyLite/
│       ├── Data/
│       │   ├── SaveData.cs                  # Serializable DTO: full game state snapshot
│       │   └── LandmarkSaveEntry.cs         # Struct: colorGroup + level for serialization
│       ├── State/
│       │   └── PlayerStats.cs               # Class: totalRolls, totalCoinsEarned, boardsCompleted, etc.
│       ├── Logic/
│       │   └── SaveAdapter.cs               # Static: ToSaveData(GameState) + ApplyToGameState(SaveData, GameState)
│       └── Config/
│           ├── ISaveService.cs              # Interface: HasSave, Load, Save, Delete
│           └── LocalSaveService.cs          # Local JSON file implementation
└── Tests/
    └── EditMode/
        ├── PlayerStatsTests.cs
        ├── SaveAdapterTests.cs
        └── LocalSaveServiceTests.cs
```

### Existing Files (Modify)

| File | Action | Changes |
|---|---|---|
| `Assets/Scripts/MonopolyLite/State/PlayerState.cs` | MODIFY | Add `SetCoins(int)`, `SetDice(int)` methods |
| `Assets/Scripts/MonopolyLite/State/ProgressionState.cs` | MODIFY | Add `LoadMilestones(int[])`, `LoadMultipliers(int[])` methods |
| `Assets/Scripts/MonopolyLite/State/GameState.cs` | MODIFY | Add `PlayerStats Stats` property |
| `Assets/Scripts/MonopolyLite/Core/GameController.cs` | MODIFY | Add save/load integration, stats tracking |

---

## Task 1: SaveData + LandmarkSaveEntry

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Data/LandmarkSaveEntry.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/SaveData.cs`

- [ ] **Step 1: Create LandmarkSaveEntry.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct LandmarkSaveEntry
    {
        public int colorGroup; // cast from ColorGroup enum
        public int level;
    }
}
```

- [ ] **Step 2: Create SaveData.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string lastSavedAt;

        // Player
        public int coins;
        public int dice;
        public int diceCap;
        public int position;
        public int shields;
        public int netWorth;
        public int multiplier;
        public int jailTurnsLeft;

        // Progression
        public int currentBoardIndex;
        public int loginStreak;
        public string lastLoginDate;
        public long lastRegenTicks;
        public int diceRegenSeconds;
        public int[] claimedMilestones;
        public int[] unlockedMultipliers;

        // Board
        public LandmarkSaveEntry[] landmarkLevels;
        public int chanceDrawIndex;
        public int communityChestDrawIndex;

        // Stats
        public int totalRolls;
        public int totalCoinsEarned;
        public int boardsCompleted;
        public int heistsCompleted;
        public int shutdownsDealt;
    }
}
```

- [ ] **Step 3: Create .meta files and commit**

```bash
git add Assets/Scripts/MonopolyLite/Data/LandmarkSaveEntry.cs Assets/Scripts/MonopolyLite/Data/SaveData.cs
git commit -m "feat(phase3b): add SaveData and LandmarkSaveEntry serializable DTOs"
```

---

## Task 2: PlayerStats + GameState Update

**Files:**
- Create: `Assets/Scripts/MonopolyLite/State/PlayerStats.cs`
- Create: `Assets/Tests/EditMode/PlayerStatsTests.cs`
- Modify: `Assets/Scripts/MonopolyLite/State/GameState.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class PlayerStatsTests
    {
        [Test]
        public void Constructor_AllZero()
        {
            var stats = new PlayerStats();

            Assert.AreEqual(0, stats.TotalRolls);
            Assert.AreEqual(0, stats.TotalCoinsEarned);
            Assert.AreEqual(0, stats.BoardsCompleted);
            Assert.AreEqual(0, stats.HeistsCompleted);
            Assert.AreEqual(0, stats.ShutdownsDealt);
        }

        [Test]
        public void Properties_Settable()
        {
            var stats = new PlayerStats();
            stats.TotalRolls = 50;
            stats.TotalCoinsEarned = 10000;
            stats.BoardsCompleted = 2;
            stats.HeistsCompleted = 15;
            stats.ShutdownsDealt = 8;

            Assert.AreEqual(50, stats.TotalRolls);
            Assert.AreEqual(10000, stats.TotalCoinsEarned);
            Assert.AreEqual(2, stats.BoardsCompleted);
            Assert.AreEqual(15, stats.HeistsCompleted);
            Assert.AreEqual(8, stats.ShutdownsDealt);
        }
    }
}
```

- [ ] **Step 2: Implement PlayerStats**

```csharp
namespace MonopolyLite.State
{
    public class PlayerStats
    {
        public int TotalRolls { get; set; }
        public int TotalCoinsEarned { get; set; }
        public int BoardsCompleted { get; set; }
        public int HeistsCompleted { get; set; }
        public int ShutdownsDealt { get; set; }
    }
}
```

- [ ] **Step 3: Update GameState to include PlayerStats**

In `Assets/Scripts/MonopolyLite/State/GameState.cs`, add `Stats` property and update constructor:

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
        public PlayerStats Stats { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap,
                         ProgressionState progression = null, PlayerStats stats = null)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
            Progression = progression;
            Stats = stats ?? new PlayerStats();
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

- [ ] **Step 4: Run all tests to verify no regressions**

Run: Unity Test Runner → EditMode → All
Expected: All tests PASS (existing + 2 new PlayerStats tests)

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/State/PlayerStats.cs Assets/Tests/EditMode/PlayerStatsTests.cs Assets/Scripts/MonopolyLite/State/GameState.cs
git commit -m "feat(phase3b): add PlayerStats tracking and include in GameState"
```

---

## Task 3: State Load Support

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/State/PlayerState.cs`
- Modify: `Assets/Scripts/MonopolyLite/State/ProgressionState.cs`
- Modify: `Assets/Tests/EditMode/GameStateTests.cs`

- [ ] **Step 1: Write failing tests**

Add to `GameStateTests.cs`:

```csharp
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
```

- [ ] **Step 2: Add SetCoins and SetDice to PlayerState**

In `Assets/Scripts/MonopolyLite/State/PlayerState.cs`, add after `SetDiceCap`:

```csharp
public void SetCoins(int coins)
{
    Coins = System.Math.Max(coins, 0);
}

public void SetDice(int dice)
{
    Dice = System.Math.Clamp(dice, 0, DiceCap);
}
```

- [ ] **Step 3: Add LoadMilestones and LoadMultipliers to ProgressionState**

In `Assets/Scripts/MonopolyLite/State/ProgressionState.cs`, add after `IsMultiplierUnlocked`:

```csharp
public void LoadMilestones(int[] milestones)
{
    ClaimedMilestones = new System.Collections.Generic.HashSet<int>(milestones);
}

public void LoadMultipliers(int[] multipliers)
{
    UnlockedMultipliers = new System.Collections.Generic.List<int>(multipliers);
}
```

- [ ] **Step 4: Run tests**

Run: Unity Test Runner → EditMode → GameStateTests
Expected: All tests PASS (existing + 5 new)

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/State/PlayerState.cs Assets/Scripts/MonopolyLite/State/ProgressionState.cs Assets/Tests/EditMode/GameStateTests.cs
git commit -m "feat(phase3b): add SetCoins/SetDice, LoadMilestones/LoadMultipliers for save support"
```

---

## Task 4: SaveAdapter + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/SaveAdapter.cs`
- Create: `Assets/Tests/EditMode/SaveAdapterTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
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

            // Create fresh state and apply
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
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL — `SaveAdapter` does not exist

- [ ] **Step 3: Implement SaveAdapter**

```csharp
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
                // Player
                coins = state.Player.Coins,
                dice = state.Player.Dice,
                diceCap = state.Player.DiceCap,
                position = state.Player.Position,
                shields = state.Player.Shields,
                netWorth = state.Player.NetWorth,
                multiplier = state.Player.Multiplier,
                jailTurnsLeft = state.Player.JailTurnsLeft,
                // Board
                chanceDrawIndex = state.Board.ChanceDrawIndex,
                communityChestDrawIndex = state.Board.CommunityChestDrawIndex,
            };

            // Landmarks
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

            // Progression
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

            // Stats
            if (state.Stats != null)
            {
                data.totalRolls = state.Stats.TotalRolls;
                data.totalCoinsEarned = state.Stats.TotalCoinsEarned;
                data.boardsCompleted = state.Stats.BoardsCompleted;
                data.heistsCompleted = state.Stats.HeistsCompleted;
                data.shutdownsDealt = state.Stats.ShutdownsDealt;
            }

            return data;
        }

        public static void ApplyToGameState(SaveData data, GameState state)
        {
            // Player
            state.Player.SetDiceCap(data.diceCap);
            state.Player.SetCoins(data.coins);
            state.Player.SetDice(data.dice);
            state.Player.Position = data.position;
            state.Player.Shields = data.shields;
            state.Player.NetWorth = data.netWorth;
            state.Player.Multiplier = data.multiplier;
            state.Player.JailTurnsLeft = data.jailTurnsLeft;

            // Board
            if (data.landmarkLevels != null)
            {
                foreach (var entry in data.landmarkLevels)
                    state.Board.SetLandmarkLevel((ColorGroup)entry.colorGroup, entry.level);
            }
            state.Board.ChanceDrawIndex = data.chanceDrawIndex;
            state.Board.CommunityChestDrawIndex = data.communityChestDrawIndex;

            // Progression
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

            // Stats
            if (state.Stats != null)
            {
                state.Stats.TotalRolls = data.totalRolls;
                state.Stats.TotalCoinsEarned = data.totalCoinsEarned;
                state.Stats.BoardsCompleted = data.boardsCompleted;
                state.Stats.HeistsCompleted = data.heistsCompleted;
                state.Stats.ShutdownsDealt = data.shutdownsDealt;
            }
        }
    }
}
```

**IMPORTANT:** `SetDiceCap` must be called before `SetDice` in `ApplyToGameState` because `SetDice` clamps to `DiceCap`.

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → SaveAdapterTests
Expected: All 8 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/SaveAdapter.cs Assets/Tests/EditMode/SaveAdapterTests.cs
git commit -m "feat(phase3b): add SaveAdapter — GameState to/from SaveData conversion"
```

---

## Task 5: ISaveService + LocalSaveService + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Config/ISaveService.cs`
- Create: `Assets/Scripts/MonopolyLite/Config/LocalSaveService.cs`
- Create: `Assets/Tests/EditMode/LocalSaveServiceTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Config;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class LocalSaveServiceTests
    {
        LocalSaveService _service;
        string _testPath;

        [SetUp]
        public void SetUp()
        {
            _testPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"monopoly_test_{System.Guid.NewGuid()}.json");
            _service = new LocalSaveService(_testPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (System.IO.File.Exists(_testPath))
                System.IO.File.Delete(_testPath);
        }

        [Test]
        public void HasSave_FalseWhenNoFile()
        {
            Assert.IsFalse(_service.HasSave());
        }

        [Test]
        public void Save_CreateFile()
        {
            var data = new SaveData { coins = 100 };

            _service.Save(data);

            Assert.IsTrue(_service.HasSave());
            Assert.IsTrue(System.IO.File.Exists(_testPath));
        }

        [Test]
        public void SaveAndLoad_RoundTrip()
        {
            var data = new SaveData
            {
                coins = 5000,
                dice = 80,
                netWorth = 1200,
                currentBoardIndex = 1,
                loginStreak = 3,
                claimedMilestones = new int[] { 0, 1 },
                unlockedMultipliers = new int[] { 1, 2 },
                landmarkLevels = new LandmarkSaveEntry[]
                {
                    new LandmarkSaveEntry { colorGroup = 1, level = 3 },
                },
                totalRolls = 42,
            };

            _service.Save(data);
            var loaded = _service.Load();

            Assert.AreEqual(5000, loaded.coins);
            Assert.AreEqual(80, loaded.dice);
            Assert.AreEqual(1200, loaded.netWorth);
            Assert.AreEqual(1, loaded.currentBoardIndex);
            Assert.AreEqual(3, loaded.loginStreak);
            Assert.AreEqual(42, loaded.totalRolls);
            Assert.AreEqual(2, loaded.claimedMilestones.Length);
            Assert.AreEqual(1, loaded.landmarkLevels.Length);
            Assert.AreEqual(3, loaded.landmarkLevels[0].level);
        }

        [Test]
        public void Delete_RemovesFile()
        {
            _service.Save(new SaveData { coins = 1 });
            Assert.IsTrue(_service.HasSave());

            _service.Delete();

            Assert.IsFalse(_service.HasSave());
        }

        [Test]
        public void Save_SetsLastSavedAt()
        {
            _service.Save(new SaveData());
            var loaded = _service.Load();

            Assert.IsNotNull(loaded.lastSavedAt);
            Assert.IsNotEmpty(loaded.lastSavedAt);
        }
    }
}
```

- [ ] **Step 2: Create ISaveService**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public interface ISaveService
    {
        bool HasSave();
        SaveData Load();
        void Save(SaveData data);
        void Delete();
    }
}
```

- [ ] **Step 3: Implement LocalSaveService**

```csharp
using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    public class LocalSaveService : ISaveService
    {
        readonly string _filePath;

        public LocalSaveService(string filePath = null)
        {
            _filePath = filePath ?? Application.persistentDataPath + "/save.json";
        }

        public bool HasSave()
        {
            return System.IO.File.Exists(_filePath);
        }

        public SaveData Load()
        {
            string json = System.IO.File.ReadAllText(_filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public void Save(SaveData data)
        {
            data.lastSavedAt = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(_filePath, json);
        }

        public void Delete()
        {
            if (System.IO.File.Exists(_filePath))
                System.IO.File.Delete(_filePath);
        }
    }
}
```

- [ ] **Step 4: Run tests**

Run: Unity Test Runner → EditMode → LocalSaveServiceTests
Expected: All 5 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Config/ISaveService.cs Assets/Scripts/MonopolyLite/Config/LocalSaveService.cs Assets/Tests/EditMode/LocalSaveServiceTests.cs
git commit -m "feat(phase3b): add ISaveService interface and LocalSaveService with JSON persistence"
```

---

## Task 6: GameController Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Read current GameController.cs fully**

- [ ] **Step 2: Add save service field**

After existing system fields:

```csharp
ISaveService _saveService;
```

- [ ] **Step 3: Update Initialize to load saved state**

Replace the Initialize method. Key changes:
- Create `_saveService = new LocalSaveService();`
- If save exists: load SaveData, determine board from saved index, create GameState, apply save
- If no save: create fresh GameState (current behavior)
- Create `PlayerStats` in GameState constructor

```csharp
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
```

- [ ] **Step 4: Add AutoSave helper method**

```csharp
void AutoSave()
{
    if (_saveService == null) return;
    var data = SaveAdapter.ToSaveData(State);
    _saveService.Save(data);
}
```

- [ ] **Step 5: Add stats tracking and auto-save calls to existing methods**

In `DoRoll()`, after a successful roll (after `OnTileResolved?.Invoke(resolve);` in non-jail path), add:

```csharp
State.Stats.TotalRolls++;
if (resolve.Type == TileResolveType.CoinsGained)
    State.Stats.TotalCoinsEarned += resolve.Amount;
AutoSave();
```

In the jail roll path (both exit-on-doubles and tick-jail-turn branches), add:

```csharp
State.Stats.TotalRolls++;
AutoSave();
```

In `DoUpgradeLandmark()`, after `OnLandmarkUpgraded?.Invoke(...)`, add:

```csharp
AutoSave();
```

In `HandleRailroadEvent()`, after `State.Player.AddCoins(result.CoinsEarned);` in the heist branch, add:

```csharp
State.Stats.HeistsCompleted++;
State.Stats.TotalCoinsEarned += result.CoinsEarned;
AutoSave();
```

In `DoShutdownAttack()`, after `State.Player.AddCoins(result.CoinsEarned);`, add:

```csharp
State.Stats.ShutdownsDealt++;
State.Stats.TotalCoinsEarned += result.CoinsEarned;
AutoSave();
```

In `TryTransitionToNextBoard()`, add before `OnBoardTransition?.Invoke(...)`:

```csharp
State.Stats.BoardsCompleted++;
```

- [ ] **Step 6: Add using directive**

Ensure at top of GameController.cs:

```csharp
using MonopolyLite.Logic; // for SaveAdapter (already present)
```

- [ ] **Step 7: Verify compilation**

Run: Confirm no compile errors in Unity.

- [ ] **Step 8: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs
git commit -m "feat(phase3b): add auto-save, load on init, stats tracking to GameController"
```

---

## Summary

| Task | Files | Tests | Description |
|---|---|---|---|
| 1 | 2 new | — | SaveData + LandmarkSaveEntry serializable DTOs |
| 2 | 2 new + 1 mod | 2 | PlayerStats class + GameState update |
| 3 | 2 mod | 5 | PlayerState.SetCoins/SetDice, ProgressionState.LoadMilestones/LoadMultipliers |
| 4 | 1 new | 8 | SaveAdapter — GameState ↔ SaveData conversion |
| 5 | 2 new | 5 | ISaveService + LocalSaveService (JSON file persistence) |
| 6 | 1 mod | — | GameController: save/load, auto-save, stats tracking |

**Total: 7 new files, 4 modified files, 20 new unit tests**
