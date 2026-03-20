# Phase 3a: Social Mechanics (Local) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Railroad placeholder with Bank Heist and Shutdown minigames — fully playable locally with mock bot targets, no Firebase dependency.

**Architecture:** New pure C# logic systems (`HeistSystem`, `ShutdownSystem`) resolve minigame outcomes via RNG. `ITargetProvider` interface abstracts target selection (mock bots now, Firebase later in Phase 3b). `TileResolver` Railroad case delegates to `GameController` which coordinates the minigame flow. Shutdown is async (player picks a landmark), Heist is immediate (pre-determined outcome). New View panels display results.

**Tech Stack:** Unity 6.0.3, C# 9.0, NUnit, TextMeshPro

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md` (Phase 3 sections: 3.1–3.3)

**Agent Team Roles:**
- **Game Designer** — reward values, probabilities, bot name pool
- **Unity Architect** — ITargetProvider interface, async Shutdown flow
- **Senior Developer** — implementation
- **Code Reviewer** — quality gate

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   └── MonopolyLite/
│       ├── Data/
│       │   ├── HeistSymbol.cs               # Enum: CoinBag, GoldBar, Diamond
│       │   ├── RailroadEventType.cs         # Enum: Heist, Shutdown
│       │   ├── HeistResult.cs               # Struct: isMatch, matchedSymbol, coinsEarned, grid[12]
│       │   ├── ShutdownResult.cs            # Struct: shielded, coinsEarned, targetedLandmark, target
│       │   ├── TargetLandmark.cs            # Struct: colorGroup, name, level
│       │   └── TargetProfile.cs             # Class: displayName, netWorth, shields, landmarks[]
│       ├── Logic/
│       │   ├── ITargetProvider.cs           # Interface: GetRandomTarget(int boardIndex)
│       │   ├── MockTargetProvider.cs        # Bot profile generator with RNG
│       │   ├── HeistSystem.cs               # Heist resolution: symbol probability, reward calc, grid gen
│       │   └── ShutdownSystem.cs            # Shutdown resolution: shield check, reward calc
│       └── View/
│           ├── HeistPanelView.cs            # 3x4 grid display, result animation
│           └── ShutdownPanelView.cs         # Target landmarks display, player choice, result
└── Tests/
    └── EditMode/
        ├── MockTargetProviderTests.cs
        ├── HeistSystemTests.cs
        └── ShutdownSystemTests.cs
```

### Existing Files (Modify)

| File | Action | Changes |
|---|---|---|
| `Assets/Scripts/MonopolyLite/Logic/TileResolver.cs` | MODIFY | Railroad case: return type only, no coins |
| `Assets/Tests/EditMode/TileResolverTests.cs` | MODIFY | Update Railroad test expectations |
| `Assets/Scripts/MonopolyLite/Core/GameController.cs` | MODIFY | Add Heist/Shutdown handling, new events, pending state |
| `Assets/Scripts/MonopolyLite/View/HUDView.cs` | MODIFY | Subscribe to new events, status messages |
| `Assets/Scripts/MonopolyLite/View/UIManager.cs` | MODIFY | Create and wire HeistPanelView, ShutdownPanelView |

---

## Task 1: Data Definitions

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Data/HeistSymbol.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/RailroadEventType.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/HeistResult.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/TargetLandmark.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/TargetProfile.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/ShutdownResult.cs`

- [ ] **Step 1: Create HeistSymbol.cs**

```csharp
namespace MonopolyLite.Data
{
    public enum HeistSymbol { CoinBag, GoldBar, Diamond }
}
```

- [ ] **Step 2: Create RailroadEventType.cs**

```csharp
namespace MonopolyLite.Data
{
    public enum RailroadEventType { Heist, Shutdown }
}
```

- [ ] **Step 3: Create TargetLandmark.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct TargetLandmark
    {
        public ColorGroup colorGroup;
        public string name;
        public int level;
    }
}
```

- [ ] **Step 4: Create TargetProfile.cs**

```csharp
namespace MonopolyLite.Data
{
    public class TargetProfile
    {
        public string displayName;
        public int netWorth;
        public int shields;
        public TargetLandmark[] landmarks;
    }
}
```

- [ ] **Step 5: Create HeistResult.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct HeistResult
    {
        public bool IsMatch;
        public HeistSymbol MatchedSymbol;
        public int CoinsEarned;
        public HeistSymbol[] Grid; // 12 cells (3x4) for display
    }
}
```

- [ ] **Step 6: Create ShutdownResult.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct ShutdownResult
    {
        public bool Shielded;
        public int CoinsEarned;
        public ColorGroup TargetedLandmark;
        public string TargetName;
    }
}
```

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Data/HeistSymbol.cs Assets/Scripts/MonopolyLite/Data/RailroadEventType.cs Assets/Scripts/MonopolyLite/Data/HeistResult.cs Assets/Scripts/MonopolyLite/Data/TargetLandmark.cs Assets/Scripts/MonopolyLite/Data/TargetProfile.cs Assets/Scripts/MonopolyLite/Data/ShutdownResult.cs
git commit -m "feat(phase3a): add Heist/Shutdown data types, TargetProfile, enums"
```

---

## Task 2: ITargetProvider + MockTargetProvider + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/ITargetProvider.cs`
- Create: `Assets/Scripts/MonopolyLite/Logic/MockTargetProvider.cs`
- Create: `Assets/Tests/EditMode/MockTargetProviderTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MockTargetProviderTests
    {
        MockTargetProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _provider = new MockTargetProvider(42);
        }

        [Test]
        public void GetRandomTarget_ReturnsNonNull()
        {
            var target = _provider.GetRandomTarget(0);

            Assert.IsNotNull(target);
        }

        [Test]
        public void GetRandomTarget_HasDisplayName()
        {
            var target = _provider.GetRandomTarget(0);

            Assert.IsNotNull(target.displayName);
            Assert.IsNotEmpty(target.displayName);
        }

        [Test]
        public void GetRandomTarget_ShieldsInRange()
        {
            for (int i = 0; i < 20; i++)
            {
                var target = _provider.GetRandomTarget(0);
                Assert.GreaterOrEqual(target.shields, 0);
                Assert.LessOrEqual(target.shields, 3);
            }
        }

        [Test]
        public void GetRandomTarget_HasLandmarks()
        {
            var target = _provider.GetRandomTarget(0);

            Assert.IsNotNull(target.landmarks);
            Assert.Greater(target.landmarks.Length, 0);
        }

        [Test]
        public void GetRandomTarget_LandmarkLevelsInRange()
        {
            var target = _provider.GetRandomTarget(0);

            foreach (var lm in target.landmarks)
            {
                Assert.GreaterOrEqual(lm.level, 0);
                Assert.LessOrEqual(lm.level, 5);
            }
        }

        [Test]
        public void GetRandomTarget_NetWorthPositive()
        {
            var target = _provider.GetRandomTarget(0);

            Assert.GreaterOrEqual(target.netWorth, 0);
        }

        [Test]
        public void GetRandomTarget_DeterministicWithSameSeed()
        {
            var p1 = new MockTargetProvider(99);
            var p2 = new MockTargetProvider(99);

            var t1 = p1.GetRandomTarget(0);
            var t2 = p2.GetRandomTarget(0);

            Assert.AreEqual(t1.displayName, t2.displayName);
            Assert.AreEqual(t1.shields, t2.shields);
            Assert.AreEqual(t1.netWorth, t2.netWorth);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → MockTargetProviderTests
Expected: FAIL — types do not exist

- [ ] **Step 3: Create ITargetProvider.cs**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public interface ITargetProvider
    {
        TargetProfile GetRandomTarget(int boardIndex);
    }
}
```

- [ ] **Step 4: Implement MockTargetProvider.cs**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class MockTargetProvider : ITargetProvider
    {
        static readonly string[] BotNames =
        {
            "Ali", "Ayse", "Mehmet", "Fatma", "Emre", "Zeynep",
            "Burak", "Elif", "Cem", "Deniz", "Gul", "Hakan"
        };

        static readonly ColorGroup[] LandmarkGroups =
        {
            ColorGroup.Brown, ColorGroup.LightBlue, ColorGroup.Pink, ColorGroup.Orange,
            ColorGroup.Red, ColorGroup.Yellow, ColorGroup.Green, ColorGroup.Blue
        };

        static readonly string[] LandmarkNames =
        {
            "Monument A", "Monument B", "Monument C", "Monument D",
            "Monument E", "Monument F", "Monument G", "Monument H"
        };

        RNG _rng;

        public MockTargetProvider(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public TargetProfile GetRandomTarget(int boardIndex)
        {
            string name = BotNames[_rng.Next(0, BotNames.Length)];
            int shields = _rng.Next(0, 4); // 0-3
            int landmarkCount = _rng.Next(4, LandmarkGroups.Length + 1); // 4-8 landmarks

            var landmarks = new TargetLandmark[landmarkCount];
            int totalNW = 0;

            for (int i = 0; i < landmarkCount; i++)
            {
                int level = _rng.Next(1, 6); // 1-5
                int nw = level * 100 * (boardIndex + 1);
                totalNW += nw;
                landmarks[i] = new TargetLandmark
                {
                    colorGroup = LandmarkGroups[i],
                    name = LandmarkNames[i],
                    level = level,
                };
            }

            return new TargetProfile
            {
                displayName = name,
                shields = shields,
                netWorth = totalNW,
                landmarks = landmarks,
            };
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → MockTargetProviderTests
Expected: All 7 tests PASS

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/ITargetProvider.cs Assets/Scripts/MonopolyLite/Logic/MockTargetProvider.cs Assets/Tests/EditMode/MockTargetProviderTests.cs
git commit -m "feat(phase3a): add ITargetProvider interface and MockTargetProvider with bot profiles"
```

---

## Task 3: HeistSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/HeistSystem.cs`
- Create: `Assets/Tests/EditMode/HeistSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class HeistSystemTests
    {
        HeistSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new HeistSystem(42);
        }

        [Test]
        public void Resolve_ReturnsPositiveCoins()
        {
            var result = _system.Resolve(1, 1f);

            Assert.Greater(result.CoinsEarned, 0);
        }

        [Test]
        public void Resolve_MultiplierScalesReward()
        {
            var s1 = new HeistSystem(99);
            var s2 = new HeistSystem(99);

            var r1 = s1.Resolve(1, 1f);
            var r2 = s2.Resolve(5, 1f);

            Assert.AreEqual(r1.CoinsEarned * 5, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_BoardMultiplierScalesReward()
        {
            var s1 = new HeistSystem(99);
            var s2 = new HeistSystem(99);

            var r1 = s1.Resolve(1, 1f);
            var r2 = s2.Resolve(1, 2f);

            Assert.AreEqual(r1.CoinsEarned * 2, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_GridHas12Cells()
        {
            var result = _system.Resolve(1, 1f);

            Assert.IsNotNull(result.Grid);
            Assert.AreEqual(12, result.Grid.Length);
        }

        [Test]
        public void Resolve_MatchHas3MatchingSymbolsInGrid()
        {
            // Run many times until we get a match
            for (int seed = 0; seed < 100; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);
                if (!result.IsMatch) continue;

                int matchCount = 0;
                foreach (var cell in result.Grid)
                    if (cell == result.MatchedSymbol) matchCount++;

                Assert.GreaterOrEqual(matchCount, 3);
                return;
            }
            Assert.Fail("No match found in 100 seeds");
        }

        [Test]
        public void Resolve_MissGivesMinimumReward()
        {
            // Find a miss result
            for (int seed = 0; seed < 200; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);
                if (result.IsMatch) continue;

                Assert.AreEqual(50, result.CoinsEarned);
                Assert.IsFalse(result.IsMatch);
                return;
            }
            Assert.Fail("No miss found in 200 seeds");
        }

        [Test]
        public void Resolve_DeterministicWithSameSeed()
        {
            var s1 = new HeistSystem(123);
            var s2 = new HeistSystem(123);

            var r1 = s1.Resolve(2, 1.5f);
            var r2 = s2.Resolve(2, 1.5f);

            Assert.AreEqual(r1.IsMatch, r2.IsMatch);
            Assert.AreEqual(r1.MatchedSymbol, r2.MatchedSymbol);
            Assert.AreEqual(r1.CoinsEarned, r2.CoinsEarned);
        }

        [Test]
        public void Resolve_DistributionHasAllOutcomes()
        {
            bool hasCoinBag = false, hasGoldBar = false, hasDiamond = false, hasMiss = false;

            for (int seed = 0; seed < 500; seed++)
            {
                var system = new HeistSystem(seed);
                var result = system.Resolve(1, 1f);

                if (!result.IsMatch) hasMiss = true;
                else if (result.MatchedSymbol == HeistSymbol.CoinBag) hasCoinBag = true;
                else if (result.MatchedSymbol == HeistSymbol.GoldBar) hasGoldBar = true;
                else if (result.MatchedSymbol == HeistSymbol.Diamond) hasDiamond = true;
            }

            Assert.IsTrue(hasCoinBag, "Never got CoinBag");
            Assert.IsTrue(hasGoldBar, "Never got GoldBar");
            Assert.IsTrue(hasDiamond, "Never got Diamond");
            Assert.IsTrue(hasMiss, "Never got Miss");
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → HeistSystemTests
Expected: FAIL — `HeistSystem` does not exist

- [ ] **Step 3: Implement HeistSystem**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class HeistSystem
    {
        RNG _rng;

        const int CoinBagReward = 100;
        const int GoldBarReward = 300;
        const int DiamondReward = 1000;
        const int MissReward = 50;

        public HeistSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public HeistResult Resolve(int multiplier, float boardMultiplier)
        {
            int roll = _rng.Next(0, 100);

            bool isMatch;
            HeistSymbol symbol;
            int baseReward;

            if (roll < 40)      { isMatch = true;  symbol = HeistSymbol.CoinBag; baseReward = CoinBagReward; }
            else if (roll < 70) { isMatch = true;  symbol = HeistSymbol.GoldBar; baseReward = GoldBarReward; }
            else if (roll < 80) { isMatch = true;  symbol = HeistSymbol.Diamond; baseReward = DiamondReward; }
            else                { isMatch = false; symbol = HeistSymbol.CoinBag; baseReward = MissReward; }

            int coinsEarned = (int)(baseReward * multiplier * boardMultiplier);
            var grid = GenerateGrid(isMatch, symbol);

            return new HeistResult
            {
                IsMatch = isMatch,
                MatchedSymbol = symbol,
                CoinsEarned = coinsEarned,
                Grid = grid,
            };
        }

        HeistSymbol[] GenerateGrid(bool isMatch, HeistSymbol matched)
        {
            var grid = new HeistSymbol[12];
            var allSymbols = new[] { HeistSymbol.CoinBag, HeistSymbol.GoldBar, HeistSymbol.Diamond };

            if (isMatch)
            {
                // Place 3 of the matched symbol at random positions
                var positions = new bool[12];
                int placed = 0;
                while (placed < 3)
                {
                    int pos = _rng.Next(0, 12);
                    if (!positions[pos])
                    {
                        positions[pos] = true;
                        grid[pos] = matched;
                        placed++;
                    }
                }

                // Fill remaining with other symbols
                for (int i = 0; i < 12; i++)
                {
                    if (!positions[i])
                        grid[i] = allSymbols[_rng.Next(0, allSymbols.Length)];
                }
            }
            else
            {
                // Fill randomly for miss display
                for (int i = 0; i < 12; i++)
                    grid[i] = allSymbols[_rng.Next(0, allSymbols.Length)];
            }

            return grid;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → HeistSystemTests
Expected: All 8 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/HeistSystem.cs Assets/Tests/EditMode/HeistSystemTests.cs
git commit -m "feat(phase3a): add HeistSystem — 3x4 grid heist with symbol matching and reward tiers"
```

---

## Task 4: ShutdownSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/ShutdownSystem.cs`
- Create: `Assets/Tests/EditMode/ShutdownSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class ShutdownSystemTests
    {
        ShutdownSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new ShutdownSystem();
        }

        TargetProfile MakeTarget(int shields)
        {
            return new TargetProfile
            {
                displayName = "TestBot",
                netWorth = 1000,
                shields = shields,
                landmarks = new TargetLandmark[]
                {
                    new TargetLandmark { colorGroup = ColorGroup.Brown, name = "LM1", level = 3 },
                    new TargetLandmark { colorGroup = ColorGroup.Blue,  name = "LM2", level = 5 },
                },
            };
        }

        [Test]
        public void Resolve_Shielded_SmallReward()
        {
            var target = MakeTarget(shields: 2);

            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);

            Assert.IsTrue(result.Shielded);
            Assert.AreEqual(50, result.CoinsEarned);
        }

        [Test]
        public void Resolve_NoShield_LargeReward()
        {
            var target = MakeTarget(shields: 0);

            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);

            Assert.IsFalse(result.Shielded);
            Assert.AreEqual(500, result.CoinsEarned);
        }

        [Test]
        public void Resolve_MultiplierScales()
        {
            var target = MakeTarget(shields: 0);

            var result = _system.Resolve(target, ColorGroup.Blue, 5, 1f);

            Assert.AreEqual(2500, result.CoinsEarned);
        }

        [Test]
        public void Resolve_BoardMultiplierScales()
        {
            var target = MakeTarget(shields: 0);

            var result = _system.Resolve(target, ColorGroup.Blue, 1, 1.8f);

            Assert.AreEqual(900, result.CoinsEarned); // 500 * 1 * 1.8 = 900
        }

        [Test]
        public void Resolve_ShieldedMultiplierScales()
        {
            var target = MakeTarget(shields: 1);

            var result = _system.Resolve(target, ColorGroup.Brown, 2, 1f);

            Assert.AreEqual(100, result.CoinsEarned); // 50 * 2
        }

        [Test]
        public void Resolve_TracksTargetedLandmark()
        {
            var target = MakeTarget(shields: 0);

            var result = _system.Resolve(target, ColorGroup.Blue, 1, 1f);

            Assert.AreEqual(ColorGroup.Blue, result.TargetedLandmark);
        }

        [Test]
        public void Resolve_IncludesTargetName()
        {
            var target = MakeTarget(shields: 0);

            var result = _system.Resolve(target, ColorGroup.Brown, 1, 1f);

            Assert.AreEqual("TestBot", result.TargetName);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Unity Test Runner → EditMode → ShutdownSystemTests
Expected: FAIL — `ShutdownSystem` does not exist

- [ ] **Step 3: Implement ShutdownSystem**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class ShutdownSystem
    {
        const int ShieldedReward = 50;
        const int UnshieldedReward = 500;

        public ShutdownResult Resolve(TargetProfile target, ColorGroup chosenLandmark,
                                      int multiplier, float boardMultiplier)
        {
            bool shielded = target.shields > 0;
            int baseReward = shielded ? ShieldedReward : UnshieldedReward;
            int coinsEarned = (int)(baseReward * multiplier * boardMultiplier);

            return new ShutdownResult
            {
                Shielded = shielded,
                CoinsEarned = coinsEarned,
                TargetedLandmark = chosenLandmark,
                TargetName = target.displayName,
            };
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → ShutdownSystemTests
Expected: All 7 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/ShutdownSystem.cs Assets/Tests/EditMode/ShutdownSystemTests.cs
git commit -m "feat(phase3a): add ShutdownSystem — shield check and reward calculation"
```

---

## Task 5: TileResolver Update + Test Updates

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Logic/TileResolver.cs`
- Modify: `Assets/Tests/EditMode/TileResolverTests.cs`

- [ ] **Step 1: Read current TileResolverTests.cs**

Read the file to find the Railroad test(s). Look for tests that assert Railroad grants coins.

- [ ] **Step 2: Update Railroad test to expect no coins**

Find the Railroad test and update it. The Railroad case should now return `TileResolveType.Railroad` with `Amount = 0` and NOT add coins to player:

```csharp
// Replace the existing Railroad test with:
[Test]
public void Railroad_ReturnsRailroadType_NoCoinGrant()
{
    // Set player on a Railroad tile
    // (Read the test file to identify how tiles are set up, then position player on a Railroad tile)
    // Assert TileResolveType.Railroad
    // Assert Amount == 0
    // Assert player coins unchanged
}
```

> **Note to implementer:** Read the actual test file structure — identify how tiles are arranged in the test setup, which tile index is Railroad, and update accordingly. The key assertion changes: Amount should be 0, and player.Coins should NOT increase.

- [ ] **Step 3: Update TileResolver Railroad case**

In `Assets/Scripts/MonopolyLite/Logic/TileResolver.cs`, replace the Railroad case:

```csharp
case TileType.Railroad:
{
    // Heist/Shutdown handled by GameController after resolve
    return new TileResolveResult { Type = TileResolveType.Railroad, Amount = 0 };
}
```

- [ ] **Step 4: Run all TileResolverTests to verify they pass**

Run: Unity Test Runner → EditMode → TileResolverTests
Expected: All tests PASS (updated Railroad test passes, others unchanged)

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/TileResolver.cs Assets/Tests/EditMode/TileResolverTests.cs
git commit -m "refactor(phase3a): Railroad tile no longer grants coins — delegated to Heist/Shutdown"
```

---

## Task 6: GameController Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Read current GameController.cs fully**

Understand the current structure, especially `DoRoll()` and the existing event pattern.

- [ ] **Step 2: Add new fields**

After existing system fields:

```csharp
HeistSystem _heistSystem;
ShutdownSystem _shutdownSystem;
ITargetProvider _targetProvider;
RNG _railroadRng;
TargetProfile _pendingShutdownTarget;
bool _awaitingShutdownChoice;
```

- [ ] **Step 3: Add new events**

After existing events:

```csharp
public event System.Action<HeistResult, TargetProfile> OnHeistResolved;
public event System.Action<TargetProfile> OnShutdownStarted;
public event System.Action<ShutdownResult> OnShutdownResolved;
```

- [ ] **Step 4: Initialize new systems in Initialize()**

Add after `_dailyLoginSystem` initialization:

```csharp
_heistSystem = new HeistSystem(RngSeed + 100);
_shutdownSystem = new ShutdownSystem();
_targetProvider = new MockTargetProvider(RngSeed + 200);
_railroadRng = new RNG((uint)(RngSeed + 300));
_awaitingShutdownChoice = false;
```

- [ ] **Step 5: Add railroad guard to DoRoll()**

At the start of `DoRoll()`, add:

```csharp
if (_awaitingShutdownChoice) return;
```

Then after `OnTileResolved?.Invoke(resolve);` in the non-jail path, add:

```csharp
if (resolve.Type == TileResolveType.Railroad)
    HandleRailroadEvent();
```

Also add the same after the jail-exit tile resolve (where `_jailSystem.TryExitOnDoubles` succeeds):

```csharp
if (tileResult.Type == TileResolveType.Railroad)
    HandleRailroadEvent();
```

- [ ] **Step 6: Add HandleRailroadEvent method**

```csharp
void HandleRailroadEvent()
{
    bool isHeist = _railroadRng.Next(0, 2) == 0;
    var target = _targetProvider.GetRandomTarget(State.Progression?.CurrentBoardIndex ?? 0);

    if (isHeist)
    {
        var result = _heistSystem.Resolve(State.Player.Multiplier, State.BoardDef.boardMultiplier);
        State.Player.AddCoins(result.CoinsEarned);
        OnHeistResolved?.Invoke(result, target);
    }
    else
    {
        _pendingShutdownTarget = target;
        _awaitingShutdownChoice = true;
        OnShutdownStarted?.Invoke(target);
    }
}
```

- [ ] **Step 7: Add DoShutdownAttack method**

```csharp
public void DoShutdownAttack(ColorGroup chosenLandmark)
{
    if (_pendingShutdownTarget == null) return;

    var result = _shutdownSystem.Resolve(
        _pendingShutdownTarget, chosenLandmark,
        State.Player.Multiplier, State.BoardDef.boardMultiplier);

    State.Player.AddCoins(result.CoinsEarned);
    _pendingShutdownTarget = null;
    _awaitingShutdownChoice = false;
    OnShutdownResolved?.Invoke(result);
}
```

- [ ] **Step 8: Add DoSkipShutdown for cancellation**

```csharp
public void DoSkipShutdown()
{
    _pendingShutdownTarget = null;
    _awaitingShutdownChoice = false;
}
```

- [ ] **Step 9: Verify compilation**

Run: Confirm no compile errors in Unity.

- [ ] **Step 10: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs
git commit -m "feat(phase3a): integrate Heist/Shutdown into GameController with railroad event handling"
```

---

## Task 7: HeistPanelView

**Files:**
- Create: `Assets/Scripts/MonopolyLite/View/HeistPanelView.cs`

- [ ] **Step 1: Read existing view files for UI patterns**

Read `HUDView.cs` and `LandmarkPanelView.cs` to understand the UI factory helpers (`CreatePanel`, `CreateLabel`) and canvas anchoring conventions.

- [ ] **Step 2: Implement HeistPanelView**

```csharp
using MonopolyLite.Core;
using MonopolyLite.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class HeistPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI _resultLabel;
        Image[] _gridCells;
        TextMeshProUGUI[] _gridLabels;
        bool _visible;

        static readonly Color CoinBagColor = new Color(0.9f, 0.75f, 0.2f);
        static readonly Color GoldBarColor = new Color(1f, 0.85f, 0f);
        static readonly Color DiamondColor = new Color(0.4f, 0.8f, 1f);
        static readonly Color PanelBg = new Color(0f, 0f, 0f, 0.85f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            Hide();

            controller.OnHeistResolved += HandleHeistResolved;
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("HeistPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0.5f, 0.5f);
            _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.anchoredPosition = Vector2.zero;
            _panel.sizeDelta = new Vector2(500f, 450f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -20f);
            titleRt.sizeDelta = new Vector2(0f, 40f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.text = "BANK HEIST";
            _titleLabel.fontSize = 28f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            // Grid (3x4 = 12 cells)
            _gridCells = new Image[12];
            _gridLabels = new TextMeshProUGUI[12];
            float cellSize = 90f;
            float gridStartX = -180f;
            float gridStartY = -80f;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int idx = row * 4 + col;
                    float x = gridStartX + col * (cellSize + 10f);
                    float y = gridStartY - row * (cellSize + 10f);

                    var cellGo = new GameObject($"Cell_{idx}", typeof(RectTransform));
                    cellGo.transform.SetParent(_panel, false);
                    var cellRt = cellGo.GetComponent<RectTransform>();
                    cellRt.anchorMin = new Vector2(0.5f, 1f);
                    cellRt.anchorMax = new Vector2(0.5f, 1f);
                    cellRt.anchoredPosition = new Vector2(x, y);
                    cellRt.sizeDelta = new Vector2(cellSize, cellSize);

                    _gridCells[idx] = cellGo.AddComponent<Image>();
                    _gridCells[idx].color = Color.gray;

                    var labelGo = new GameObject("Label", typeof(RectTransform));
                    labelGo.transform.SetParent(cellRt, false);
                    var labelRt = labelGo.GetComponent<RectTransform>();
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;

                    _gridLabels[idx] = labelGo.AddComponent<TextMeshProUGUI>();
                    _gridLabels[idx].fontSize = 16f;
                    _gridLabels[idx].alignment = TextAlignmentOptions.Center;
                    _gridLabels[idx].color = Color.white;
                }
            }

            // Result label
            var resultGo = new GameObject("Result", typeof(RectTransform));
            resultGo.transform.SetParent(_panel, false);
            var resultRt = resultGo.GetComponent<RectTransform>();
            resultRt.anchorMin = new Vector2(0f, 0f);
            resultRt.anchorMax = new Vector2(1f, 0f);
            resultRt.anchoredPosition = new Vector2(0f, 40f);
            resultRt.sizeDelta = new Vector2(0f, 50f);
            _resultLabel = resultGo.AddComponent<TextMeshProUGUI>();
            _resultLabel.fontSize = 22f;
            _resultLabel.alignment = TextAlignmentOptions.Center;
            _resultLabel.color = Color.yellow;
        }

        void HandleHeistResolved(HeistResult result, TargetProfile target)
        {
            _titleLabel.text = $"BANK HEIST vs {target.displayName}";

            for (int i = 0; i < 12; i++)
            {
                var symbol = result.Grid[i];
                _gridCells[i].color = GetSymbolColor(symbol);
                _gridLabels[i].text = GetSymbolText(symbol);
            }

            if (result.IsMatch)
                _resultLabel.text = $"Matched {result.MatchedSymbol}! +{result.CoinsEarned} coins";
            else
                _resultLabel.text = $"No match. +{result.CoinsEarned} coins";

            Show();
            Invoke(nameof(Hide), 3f);
        }

        static Color GetSymbolColor(HeistSymbol symbol) => symbol switch
        {
            HeistSymbol.CoinBag => CoinBagColor,
            HeistSymbol.GoldBar => GoldBarColor,
            HeistSymbol.Diamond => DiamondColor,
            _ => Color.gray,
        };

        static string GetSymbolText(HeistSymbol symbol) => symbol switch
        {
            HeistSymbol.CoinBag => "COIN",
            HeistSymbol.GoldBar => "GOLD",
            HeistSymbol.Diamond => "GEM",
            _ => "?",
        };

        void Show() { _panel.gameObject.SetActive(true); _visible = true; }
        public void Hide() { _panel.gameObject.SetActive(false); _visible = false; }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/HeistPanelView.cs
git commit -m "feat(phase3a): add HeistPanelView — 3x4 grid display with symbol colors and result"
```

---

## Task 8: ShutdownPanelView

**Files:**
- Create: `Assets/Scripts/MonopolyLite/View/ShutdownPanelView.cs`

- [ ] **Step 1: Implement ShutdownPanelView**

```csharp
using MonopolyLite.Core;
using MonopolyLite.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class ShutdownPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI _shieldLabel;
        TextMeshProUGUI _resultLabel;
        RectTransform _landmarkContainer;
        bool _visible;
        bool _showingResult;

        static readonly Color PanelBg = new Color(0.15f, 0f, 0f, 0.9f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            Hide();

            controller.OnShutdownStarted += HandleShutdownStarted;
            controller.OnShutdownResolved += HandleShutdownResolved;
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("ShutdownPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0.5f, 0.5f);
            _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.anchoredPosition = Vector2.zero;
            _panel.sizeDelta = new Vector2(500f, 500f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -20f);
            titleRt.sizeDelta = new Vector2(0f, 40f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.fontSize = 26f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            // Shield info
            var shieldGo = new GameObject("ShieldInfo", typeof(RectTransform));
            shieldGo.transform.SetParent(_panel, false);
            var shieldRt = shieldGo.GetComponent<RectTransform>();
            shieldRt.anchorMin = new Vector2(0f, 1f);
            shieldRt.anchorMax = new Vector2(1f, 1f);
            shieldRt.anchoredPosition = new Vector2(0f, -55f);
            shieldRt.sizeDelta = new Vector2(0f, 28f);
            _shieldLabel = shieldGo.AddComponent<TextMeshProUGUI>();
            _shieldLabel.fontSize = 20f;
            _shieldLabel.alignment = TextAlignmentOptions.Center;
            _shieldLabel.color = new Color(0.7f, 0.7f, 0.7f);

            // Landmark container
            var containerGo = new GameObject("LandmarkContainer", typeof(RectTransform));
            containerGo.transform.SetParent(_panel, false);
            _landmarkContainer = containerGo.GetComponent<RectTransform>();
            _landmarkContainer.anchorMin = new Vector2(0f, 0.15f);
            _landmarkContainer.anchorMax = new Vector2(1f, 0.85f);
            _landmarkContainer.offsetMin = new Vector2(20f, 0f);
            _landmarkContainer.offsetMax = new Vector2(-20f, -80f);

            // Result label (hidden initially)
            var resultGo = new GameObject("Result", typeof(RectTransform));
            resultGo.transform.SetParent(_panel, false);
            var resultRt = resultGo.GetComponent<RectTransform>();
            resultRt.anchorMin = new Vector2(0f, 0f);
            resultRt.anchorMax = new Vector2(1f, 0f);
            resultRt.anchoredPosition = new Vector2(0f, 30f);
            resultRt.sizeDelta = new Vector2(0f, 50f);
            _resultLabel = resultGo.AddComponent<TextMeshProUGUI>();
            _resultLabel.fontSize = 22f;
            _resultLabel.alignment = TextAlignmentOptions.Center;
            _resultLabel.color = Color.red;
            _resultLabel.text = "";
        }

        void HandleShutdownStarted(TargetProfile target)
        {
            _showingResult = false;
            _titleLabel.text = $"SHUTDOWN: {target.displayName}";
            _shieldLabel.text = $"Shields: {target.shields}/3 | NW: {target.netWorth}";
            _resultLabel.text = "Pick a landmark to attack!";
            _resultLabel.color = Color.white;

            // Clear old landmarks
            foreach (Transform child in _landmarkContainer)
                Destroy(child.gameObject);

            // Build landmark buttons
            float yPos = 0f;
            foreach (var lm in target.landmarks)
            {
                BuildLandmarkButton(lm, yPos);
                yPos -= 48f;
            }

            Show();
        }

        void BuildLandmarkButton(TargetLandmark lm, float yPos)
        {
            var go = new GameObject($"LM_{lm.colorGroup}", typeof(RectTransform));
            go.transform.SetParent(_landmarkContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta = new Vector2(0f, 44f);

            var btnBg = go.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.1f, 0.1f);

            var btn = go.AddComponent<Button>();
            var group = lm.colorGroup;
            btn.onClick.AddListener(() =>
            {
                if (_showingResult) return;
                _controller.DoShutdownAttack(group);
            });

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rt, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10f, 0f);
            labelRt.offsetMax = new Vector2(-10f, 0f);

            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = $"{lm.name} (L{lm.level}/5) — {lm.colorGroup}";
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Left;
            label.color = Color.white;
        }

        void HandleShutdownResolved(ShutdownResult result)
        {
            _showingResult = true;

            if (result.Shielded)
            {
                _resultLabel.color = Color.cyan;
                _resultLabel.text = $"Blocked by shield! +{result.CoinsEarned} coins";
            }
            else
            {
                _resultLabel.color = Color.red;
                _resultLabel.text = $"SHUTDOWN! +{result.CoinsEarned} coins";
            }

            Invoke(nameof(Hide), 3f);
        }

        void Show() { _panel.gameObject.SetActive(true); _visible = true; }
        public void Hide() { _panel.gameObject.SetActive(false); _visible = false; }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/ShutdownPanelView.cs
git commit -m "feat(phase3a): add ShutdownPanelView — landmark selection and shutdown result display"
```

---

## Task 9: UIManager + HUDView Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/View/UIManager.cs`
- Modify: `Assets/Scripts/MonopolyLite/View/HUDView.cs`

- [ ] **Step 1: Read current UIManager.cs and HUDView.cs**

- [ ] **Step 2: Add HeistPanelView and ShutdownPanelView to UIManager**

In `Initialize()`, after creating `LandmarkPanelView`, add:

```csharp
var heistGo = new GameObject("HeistPanelView");
heistGo.transform.SetParent(transform, false);
var heistPanel = heistGo.AddComponent<HeistPanelView>();
heistPanel.Initialize(controller, canvasRect);

var shutdownGo = new GameObject("ShutdownPanelView");
shutdownGo.transform.SetParent(transform, false);
var shutdownPanel = shutdownGo.AddComponent<ShutdownPanelView>();
shutdownPanel.Initialize(controller, canvasRect);
```

- [ ] **Step 3: Add heist/shutdown status messages to HUDView**

Subscribe to new events in `Initialize()`:

```csharp
controller.OnHeistResolved += HandleHeistResolved;
controller.OnShutdownStarted += HandleShutdownStarted;
controller.OnShutdownResolved += HandleShutdownResolved;
```

Add handlers:

```csharp
void HandleHeistResolved(HeistResult result, TargetProfile target)
{
    if (result.IsMatch)
        _statusLabel.text = $"Heist vs {target.displayName}: {result.MatchedSymbol}! +{result.CoinsEarned}";
    else
        _statusLabel.text = $"Heist vs {target.displayName}: Miss! +{result.CoinsEarned}";
    RefreshStats();
}

void HandleShutdownStarted(TargetProfile target)
{
    _statusLabel.text = $"Shutdown! Choose a landmark on {target.displayName}'s board...";
}

void HandleShutdownResolved(ShutdownResult result)
{
    if (result.Shielded)
        _statusLabel.text = $"Shutdown blocked by shield! +{result.CoinsEarned}";
    else
        _statusLabel.text = $"SHUTDOWN on {result.TargetName}! +{result.CoinsEarned}";
    RefreshStats();
}
```

Add required using at top of HUDView.cs:

```csharp
using MonopolyLite.Data;
```

> **Note to implementer:** Check if `using MonopolyLite.Data;` is already present or if HUDView uses fully-qualified names. Add the import if missing.

- [ ] **Step 4: Verify in Unity Editor**

Run: Open Unity, enter play mode. Land on a Railroad tile. Verify either Heist panel (3x4 grid) or Shutdown panel (landmark list) appears. Verify coins are granted. Verify roll button is disabled during Shutdown choice.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/UIManager.cs Assets/Scripts/MonopolyLite/View/HUDView.cs
git commit -m "feat(phase3a): wire HeistPanel and ShutdownPanel into UIManager, add status messages"
```

---

## Summary

| Task | Files | Tests | Description |
|---|---|---|---|
| 1 | 6 new | — | HeistSymbol, RailroadEventType, HeistResult, ShutdownResult, TargetLandmark, TargetProfile |
| 2 | 2 new | 7 | ITargetProvider interface + MockTargetProvider with bot profiles |
| 3 | 1 new | 8 | HeistSystem — symbol matching, reward tiers, grid generation |
| 4 | 1 new | 7 | ShutdownSystem — shield check, reward calculation |
| 5 | 2 mod | — | TileResolver Railroad update (no coins) + test fix |
| 6 | 1 mod | — | GameController: railroad events, Heist/Shutdown flow, pending state |
| 7 | 1 new | — | HeistPanelView — 3x4 grid display |
| 8 | 1 new | — | ShutdownPanelView — landmark selection UI |
| 9 | 2 mod | — | UIManager + HUDView integration |

**Total: 12 new files, 5 modified files, 22 new unit tests**
