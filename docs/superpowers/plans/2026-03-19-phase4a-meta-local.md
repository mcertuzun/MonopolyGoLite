# Phase 4a: Meta Systems (Local) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Daily Missions (3-5 per day with progress tracking and bonus chest) and Sticker Album (collectible stickers with set/album completion rewards) — fully local, no Firebase dependency.

**Architecture:** `MissionSystem` generates daily missions from a config pool and tracks progress via `MissionType` matching. `StickerSystem` manages sticker collection with rarity-based duplicates. Both systems integrate into `GameController` via existing action hooks. State persists via `SaveData`/`SaveAdapter` extensions.

**Tech Stack:** Unity 6.0.3, C# 9.0, NUnit, TextMeshPro

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md` (Phase 4 sections: 4.1, 4.4)

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   └── MonopolyLite/
│       ├── Data/
│       │   ├── MissionType.cs               # Enum: RollDice, BuildLandmark, CompleteHeist, EarnCoins
│       │   ├── MissionDef.cs                # Struct: type, description, target, coinReward, diceReward
│       │   ├── MissionSaveEntry.cs          # Struct: type, target, progress (for serialization)
│       │   ├── StickerRarity.cs             # Enum: Star1=1, Star2, Star3, Star4, Star5
│       │   ├── StickerDef.cs                # Struct: id, name, setIndex, rarity
│       │   ├── StickerSetDef.cs             # Struct: name, stickerCount, coinReward, diceReward
│       │   ├── AlbumDef.cs                  # Class: name, sets[], stickers[]
│       │   └── StickerSaveEntry.cs          # Struct: stickerId, count (for serialization)
│       ├── State/
│       │   ├── MissionState.cs              # Class: date, missions[], bonusClaimed
│       │   └── StickerState.cs              # Class: ownedStickers dict, duplicateStars
│       ├── Logic/
│       │   ├── MissionSystem.cs             # Generate daily, track progress, check completion
│       │   └── StickerSystem.cs             # Grant sticker, check set/album completion
│       ├── Config/
│       │   ├── MissionConfigLoader.cs       # Default mission pool
│       │   └── StickerConfigLoader.cs       # Default album (Istanbul Collection)
│       └── View/
│           └── MissionPanelView.cs          # Mission list with progress bars
└── Tests/
    └── EditMode/
        ├── MissionSystemTests.cs
        └── StickerSystemTests.cs
```

### Existing Files (Modify)

| File | Action | Changes |
|---|---|---|
| `Assets/Scripts/MonopolyLite/State/GameState.cs` | MODIFY | Add `MissionState`, `StickerState` properties |
| `Assets/Scripts/MonopolyLite/Data/SaveData.cs` | MODIFY | Add mission + sticker save fields |
| `Assets/Scripts/MonopolyLite/Logic/SaveAdapter.cs` | MODIFY | Serialize/deserialize missions + stickers |
| `Assets/Scripts/MonopolyLite/Core/GameController.cs` | MODIFY | Mission tracking, sticker grants, daily mission reset |
| `Assets/Scripts/MonopolyLite/View/UIManager.cs` | MODIFY | Wire MissionPanelView |
| `Assets/Scripts/MonopolyLite/View/HUDView.cs` | MODIFY | Mission/sticker status messages |

---

## Task 1: Data Definitions

**Files:** Create 8 new files in `Assets/Scripts/MonopolyLite/Data/`

- [ ] **Step 1: Create MissionType.cs**

```csharp
namespace MonopolyLite.Data
{
    public enum MissionType { RollDice, BuildLandmark, CompleteHeist, EarnCoins }
}
```

- [ ] **Step 2: Create MissionDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct MissionDef
    {
        public MissionType type;
        public string description;
        public int target;
        public int coinReward;
        public int diceReward;
    }
}
```

- [ ] **Step 3: Create MissionSaveEntry.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct MissionSaveEntry
    {
        public int type;
        public string description;
        public int target;
        public int progress;
        public int coinReward;
        public int diceReward;
    }
}
```

- [ ] **Step 4: Create StickerRarity.cs**

```csharp
namespace MonopolyLite.Data
{
    public enum StickerRarity { Star1 = 1, Star2 = 2, Star3 = 3, Star4 = 4, Star5 = 5 }
}
```

- [ ] **Step 5: Create StickerDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct StickerDef
    {
        public int id;
        public string name;
        public int setIndex;
        public StickerRarity rarity;
    }
}
```

- [ ] **Step 6: Create StickerSetDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct StickerSetDef
    {
        public string name;
        public int stickerCount;
        public int coinReward;
        public int diceReward;
    }
}
```

- [ ] **Step 7: Create AlbumDef.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class AlbumDef
    {
        public string name;
        public StickerSetDef[] sets;
        public StickerDef[] stickers;
    }
}
```

- [ ] **Step 8: Create StickerSaveEntry.cs**

```csharp
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct StickerSaveEntry
    {
        public int stickerId;
        public int count;
    }
}
```

- [ ] **Step 9: Create .meta files and commit**

```bash
git add Assets/Scripts/MonopolyLite/Data/MissionType.cs Assets/Scripts/MonopolyLite/Data/MissionDef.cs Assets/Scripts/MonopolyLite/Data/MissionSaveEntry.cs Assets/Scripts/MonopolyLite/Data/StickerRarity.cs Assets/Scripts/MonopolyLite/Data/StickerDef.cs Assets/Scripts/MonopolyLite/Data/StickerSetDef.cs Assets/Scripts/MonopolyLite/Data/AlbumDef.cs Assets/Scripts/MonopolyLite/Data/StickerSaveEntry.cs
git commit -m "feat(phase4a): add Mission and Sticker data types"
```

---

## Task 2: MissionState + StickerState + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/State/MissionState.cs`
- Create: `Assets/Scripts/MonopolyLite/State/StickerState.cs`

- [ ] **Step 1: Create MissionState.cs**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class MissionProgress
    {
        public MissionType Type { get; set; }
        public string Description { get; set; }
        public int Target { get; set; }
        public int Progress { get; set; }
        public int CoinReward { get; set; }
        public int DiceReward { get; set; }
        public bool Completed => Progress >= Target;
    }

    public class MissionState
    {
        public string Date { get; set; }
        public MissionProgress[] Missions { get; set; }
        public bool BonusClaimed { get; set; }

        public MissionState()
        {
            Date = null;
            Missions = new MissionProgress[0];
            BonusClaimed = false;
        }
    }
}
```

- [ ] **Step 2: Create StickerState.cs**

```csharp
using System.Collections.Generic;
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class StickerState
    {
        public Dictionary<int, int> OwnedStickers { get; private set; }
        public int DuplicateStars { get; set; }

        public StickerState()
        {
            OwnedStickers = new Dictionary<int, int>();
            DuplicateStars = 0;
        }

        public void AddSticker(int stickerId, StickerRarity rarity)
        {
            if (OwnedStickers.ContainsKey(stickerId))
            {
                OwnedStickers[stickerId]++;
                DuplicateStars += (int)rarity;
            }
            else
            {
                OwnedStickers[stickerId] = 1;
            }
        }

        public int GetStickerCount(int stickerId)
        {
            return OwnedStickers.TryGetValue(stickerId, out int count) ? count : 0;
        }

        public bool HasSticker(int stickerId)
        {
            return OwnedStickers.ContainsKey(stickerId);
        }

        public void LoadFromEntries(StickerSaveEntry[] entries)
        {
            OwnedStickers = new Dictionary<int, int>();
            if (entries == null) return;
            foreach (var e in entries)
                OwnedStickers[e.stickerId] = e.count;
        }
    }
}
```

- [ ] **Step 3: Create .meta files and commit**

```bash
git add Assets/Scripts/MonopolyLite/State/MissionState.cs Assets/Scripts/MonopolyLite/State/StickerState.cs
git commit -m "feat(phase4a): add MissionState and StickerState"
```

---

## Task 3: MissionSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/MissionSystem.cs`
- Create: `Assets/Tests/EditMode/MissionSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MissionSystemTests
    {
        MissionDef[] _pool;
        MissionSystem _system;

        [SetUp]
        public void SetUp()
        {
            _pool = new MissionDef[]
            {
                new MissionDef { type = MissionType.RollDice, description = "Roll {0} times", target = 5, coinReward = 200, diceReward = 10 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmark", target = 1, coinReward = 300, diceReward = 15 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} heist", target = 2, coinReward = 400, diceReward = 20 },
                new MissionDef { type = MissionType.EarnCoins, description = "Earn {0} coins", target = 1000, coinReward = 500, diceReward = 25 },
            };
            _system = new MissionSystem(_pool, 42);
        }

        [Test]
        public void GenerateDaily_ReturnsRequestedCount()
        {
            var missions = _system.GenerateDaily(3);
            Assert.AreEqual(3, missions.Length);
        }

        [Test]
        public void GenerateDaily_MissionsStartAtZeroProgress()
        {
            var missions = _system.GenerateDaily(3);
            foreach (var m in missions)
                Assert.AreEqual(0, m.Progress);
        }

        [Test]
        public void GenerateDaily_MissionsHaveTargets()
        {
            var missions = _system.GenerateDaily(3);
            foreach (var m in missions)
                Assert.Greater(m.Target, 0);
        }

        [Test]
        public void TrackProgress_IncrementsMatchingMissions()
        {
            var missions = _system.GenerateDaily(4);

            var rollMission = System.Array.Find(missions, m => m.Type == MissionType.RollDice);
            if (rollMission == null) return; // skip if not generated

            _system.TrackProgress(missions, MissionType.RollDice, 3);

            Assert.AreEqual(3, rollMission.Progress);
        }

        [Test]
        public void TrackProgress_DoesNotIncrementOtherTypes()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 0 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 0 },
            };

            _system.TrackProgress(missions, MissionType.RollDice, 2);

            Assert.AreEqual(2, missions[0].Progress);
            Assert.AreEqual(0, missions[1].Progress);
        }

        [Test]
        public void Completed_TrueWhenProgressReachesTarget()
        {
            var mission = new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 };
            Assert.IsTrue(mission.Completed);
        }

        [Test]
        public void AllCompleted_TrueWhenAllDone()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 1 },
            };

            Assert.IsTrue(_system.AllCompleted(missions));
        }

        [Test]
        public void AllCompleted_FalseWhenAnyIncomplete()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 0 },
            };

            Assert.IsFalse(_system.AllCompleted(missions));
        }

        [Test]
        public void GenerateDaily_DeterministicWithSameSeed()
        {
            var s1 = new MissionSystem(_pool, 99);
            var s2 = new MissionSystem(_pool, 99);

            var m1 = s1.GenerateDaily(3);
            var m2 = s2.GenerateDaily(3);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(m1[i].Type, m2[i].Type);
                Assert.AreEqual(m1[i].Target, m2[i].Target);
            }
        }
    }
}
```

- [ ] **Step 2: Implement MissionSystem**

```csharp
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class MissionSystem
    {
        readonly MissionDef[] _pool;
        RNG _rng;

        public MissionSystem(MissionDef[] pool, int seed)
        {
            _pool = pool;
            _rng = new RNG((uint)seed);
        }

        public MissionProgress[] GenerateDaily(int count)
        {
            int actualCount = System.Math.Min(count, _pool.Length);
            var used = new bool[_pool.Length];
            var missions = new MissionProgress[actualCount];

            for (int i = 0; i < actualCount; i++)
            {
                int idx;
                do { idx = _rng.Next(0, _pool.Length); }
                while (used[idx]);

                used[idx] = true;
                var def = _pool[idx];
                missions[i] = new MissionProgress
                {
                    Type = def.type,
                    Description = string.Format(def.description, def.target),
                    Target = def.target,
                    Progress = 0,
                    CoinReward = def.coinReward,
                    DiceReward = def.diceReward,
                };
            }

            return missions;
        }

        public void TrackProgress(MissionProgress[] missions, MissionType type, int amount)
        {
            if (missions == null) return;
            foreach (var m in missions)
            {
                if (m.Type == type && !m.Completed)
                    m.Progress += amount;
            }
        }

        public bool AllCompleted(MissionProgress[] missions)
        {
            if (missions == null || missions.Length == 0) return false;
            foreach (var m in missions)
                if (!m.Completed) return false;
            return true;
        }
    }
}
```

- [ ] **Step 3: Create .meta files, run tests, commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/MissionSystem.cs Assets/Tests/EditMode/MissionSystemTests.cs
git commit -m "feat(phase4a): add MissionSystem — daily mission generation and progress tracking"
```

---

## Task 4: StickerSystem + Tests

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/StickerSystem.cs`
- Create: `Assets/Tests/EditMode/StickerSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class StickerSystemTests
    {
        StickerSystem _system;
        StickerState _state;
        AlbumDef _album;

        [SetUp]
        public void SetUp()
        {
            _system = new StickerSystem(42);
            _state = new StickerState();
            _album = new AlbumDef
            {
                name = "Test Album",
                sets = new StickerSetDef[]
                {
                    new StickerSetDef { name = "Set A", stickerCount = 3, coinReward = 500, diceReward = 25 },
                    new StickerSetDef { name = "Set B", stickerCount = 3, coinReward = 1000, diceReward = 50 },
                },
                stickers = new StickerDef[]
                {
                    new StickerDef { id = 0, name = "S0", setIndex = 0, rarity = StickerRarity.Star1 },
                    new StickerDef { id = 1, name = "S1", setIndex = 0, rarity = StickerRarity.Star2 },
                    new StickerDef { id = 2, name = "S2", setIndex = 0, rarity = StickerRarity.Star3 },
                    new StickerDef { id = 3, name = "S3", setIndex = 1, rarity = StickerRarity.Star1 },
                    new StickerDef { id = 4, name = "S4", setIndex = 1, rarity = StickerRarity.Star2 },
                    new StickerDef { id = 5, name = "S5", setIndex = 1, rarity = StickerRarity.Star4 },
                },
            };
        }

        [Test]
        public void GrantRandom_ReturnsValidStickerId()
        {
            int id = _system.GrantRandom(_state, _album);
            Assert.GreaterOrEqual(id, 0);
            Assert.Less(id, _album.stickers.Length);
        }

        [Test]
        public void GrantRandom_AddsStickerToState()
        {
            int id = _system.GrantRandom(_state, _album);
            Assert.IsTrue(_state.HasSticker(id));
            Assert.AreEqual(1, _state.GetStickerCount(id));
        }

        [Test]
        public void GrantRandom_DuplicateAddsDuplicateStars()
        {
            int id = _system.GrantRandom(_state, _album);
            var def = _album.stickers[id];

            // Grant same sticker again
            _state.AddSticker(id, def.rarity);

            Assert.AreEqual(2, _state.GetStickerCount(id));
            Assert.AreEqual((int)def.rarity, _state.DuplicateStars);
        }

        [Test]
        public void IsSetComplete_FalseWhenMissing()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            // Missing id 2

            Assert.IsFalse(_system.IsSetComplete(_state, _album, 0));
        }

        [Test]
        public void IsSetComplete_TrueWhenAllOwned()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            _state.AddSticker(2, StickerRarity.Star3);

            Assert.IsTrue(_system.IsSetComplete(_state, _album, 0));
        }

        [Test]
        public void GetSetOwnedCount_ReturnsCorrect()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(2, StickerRarity.Star3);

            Assert.AreEqual(2, _system.GetSetOwnedCount(_state, _album, 0));
        }

        [Test]
        public void IsAlbumComplete_FalseWhenIncomplete()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            _state.AddSticker(2, StickerRarity.Star3);
            // Set B missing

            Assert.IsFalse(_system.IsAlbumComplete(_state, _album));
        }

        [Test]
        public void IsAlbumComplete_TrueWhenAllStickersOwned()
        {
            for (int i = 0; i < 6; i++)
                _state.AddSticker(i, _album.stickers[i].rarity);

            Assert.IsTrue(_system.IsAlbumComplete(_state, _album));
        }

        [Test]
        public void GrantRandom_Deterministic()
        {
            var s1 = new StickerSystem(99);
            var s2 = new StickerSystem(99);
            var st1 = new StickerState();
            var st2 = new StickerState();

            int id1 = s1.GrantRandom(st1, _album);
            int id2 = s2.GrantRandom(st2, _album);

            Assert.AreEqual(id1, id2);
        }
    }
}
```

- [ ] **Step 2: Implement StickerSystem**

```csharp
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class StickerSystem
    {
        RNG _rng;

        public StickerSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public int GrantRandom(StickerState state, AlbumDef album)
        {
            int idx = _rng.Next(0, album.stickers.Length);
            var sticker = album.stickers[idx];
            state.AddSticker(sticker.id, sticker.rarity);
            return sticker.id;
        }

        public bool IsSetComplete(StickerState state, AlbumDef album, int setIndex)
        {
            foreach (var s in album.stickers)
            {
                if (s.setIndex == setIndex && !state.HasSticker(s.id))
                    return false;
            }
            return true;
        }

        public int GetSetOwnedCount(StickerState state, AlbumDef album, int setIndex)
        {
            int count = 0;
            foreach (var s in album.stickers)
            {
                if (s.setIndex == setIndex && state.HasSticker(s.id))
                    count++;
            }
            return count;
        }

        public bool IsAlbumComplete(StickerState state, AlbumDef album)
        {
            foreach (var s in album.stickers)
            {
                if (!state.HasSticker(s.id))
                    return false;
            }
            return true;
        }
    }
}
```

- [ ] **Step 3: Create .meta files, run tests, commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/StickerSystem.cs Assets/Tests/EditMode/StickerSystemTests.cs
git commit -m "feat(phase4a): add StickerSystem — sticker collection, set and album completion"
```

---

## Task 5: Config Loaders

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Config/MissionConfigLoader.cs`
- Create: `Assets/Scripts/MonopolyLite/Config/StickerConfigLoader.cs`

- [ ] **Step 1: Create MissionConfigLoader**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public static class MissionConfigLoader
    {
        public static MissionDef[] CreateDefaultPool()
        {
            return new MissionDef[]
            {
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 5,    coinReward = 200,  diceReward = 10 },
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 10,   coinReward = 500,  diceReward = 20 },
                new MissionDef { type = MissionType.RollDice,      description = "Roll dice {0} times",      target = 20,   coinReward = 1000, diceReward = 40 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmark(s)",    target = 1,    coinReward = 300,  diceReward = 15 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmarks",      target = 3,    coinReward = 800,  diceReward = 30 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} Bank Heist",   target = 1,    coinReward = 400,  diceReward = 20 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} Bank Heists",  target = 3,    coinReward = 1000, diceReward = 40 },
                new MissionDef { type = MissionType.EarnCoins,     description = "Earn {0} coins",           target = 1000, coinReward = 300,  diceReward = 15 },
                new MissionDef { type = MissionType.EarnCoins,     description = "Earn {0} coins",           target = 5000, coinReward = 750,  diceReward = 30 },
            };
        }
    }
}
```

- [ ] **Step 2: Create StickerConfigLoader**

```csharp
using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public static class StickerConfigLoader
    {
        public static AlbumDef CreateDefault()
        {
            return new AlbumDef
            {
                name = "Istanbul Collection",
                sets = new StickerSetDef[]
                {
                    new StickerSetDef { name = "Hagia Sophia",  stickerCount = 6, coinReward = 1000, diceReward = 50 },
                    new StickerSetDef { name = "Grand Bazaar",  stickerCount = 6, coinReward = 1500, diceReward = 75 },
                    new StickerSetDef { name = "Bosphorus",     stickerCount = 6, coinReward = 2000, diceReward = 100 },
                    new StickerSetDef { name = "Galata Tower",  stickerCount = 6, coinReward = 3000, diceReward = 150 },
                },
                stickers = BuildStickers(),
            };
        }

        static StickerDef[] BuildStickers()
        {
            var stickers = new StickerDef[24];
            string[][] names =
            {
                new[] { "Dome", "Minaret", "Fountain", "Garden", "Interior", "Mosaic" },
                new[] { "Carpet", "Lamp", "Spice", "Tea Set", "Jewelry", "Ceramic" },
                new[] { "Ferry", "Bridge", "Sunset", "Fisherman", "Seagull", "Lighthouse" },
                new[] { "Tower", "View", "Stairs", "Museum", "Cafe", "Night" },
            };
            StickerRarity[][] rarities =
            {
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4 },
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star3, StickerRarity.Star5 },
                new[] { StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4, StickerRarity.Star5 },
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4, StickerRarity.Star5 },
            };

            int id = 0;
            for (int set = 0; set < 4; set++)
            {
                for (int s = 0; s < 6; s++)
                {
                    stickers[id] = new StickerDef
                    {
                        id = id,
                        name = names[set][s],
                        setIndex = set,
                        rarity = rarities[set][s],
                    };
                    id++;
                }
            }
            return stickers;
        }
    }
}
```

- [ ] **Step 3: Create .meta files, commit**

```bash
git add Assets/Scripts/MonopolyLite/Config/MissionConfigLoader.cs Assets/Scripts/MonopolyLite/Config/StickerConfigLoader.cs
git commit -m "feat(phase4a): add MissionConfigLoader and StickerConfigLoader with defaults"
```

---

## Task 6: GameState + SaveData + SaveAdapter Updates

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/State/GameState.cs`
- Modify: `Assets/Scripts/MonopolyLite/Data/SaveData.cs`
- Modify: `Assets/Scripts/MonopolyLite/Logic/SaveAdapter.cs`

- [ ] **Step 1: Update GameState**

Read current `GameState.cs`. Add `MissionState` and `StickerState` properties:

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
        public MissionState MissionState { get; }
        public StickerState StickerState { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap,
                         ProgressionState progression = null, PlayerStats stats = null,
                         MissionState missionState = null, StickerState stickerState = null)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
            Progression = progression;
            Stats = stats ?? new PlayerStats();
            MissionState = missionState ?? new MissionState();
            StickerState = stickerState ?? new StickerState();
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

- [ ] **Step 2: Update SaveData**

Add mission and sticker fields to `SaveData.cs`:

```csharp
// Add after the stats fields:

// Missions
public string missionDate;
public MissionSaveEntry[] missions;
public bool missionBonusClaimed;

// Stickers
public StickerSaveEntry[] ownedStickers;
public int duplicateStars;
```

- [ ] **Step 3: Update SaveAdapter.ToSaveData**

Read current `SaveAdapter.cs`. Add mission and sticker serialization to `ToSaveData`, after the Stats block:

```csharp
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
```

- [ ] **Step 4: Update SaveAdapter.ApplyToGameState**

Add mission and sticker deserialization, after the Stats block:

```csharp
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
```

- [ ] **Step 5: Verify compilation, commit**

```bash
git add Assets/Scripts/MonopolyLite/State/GameState.cs Assets/Scripts/MonopolyLite/Data/SaveData.cs Assets/Scripts/MonopolyLite/Logic/SaveAdapter.cs
git commit -m "feat(phase4a): extend GameState, SaveData, SaveAdapter with mission and sticker support"
```

---

## Task 7: GameController Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Read current GameController.cs fully**

- [ ] **Step 2: Add new system fields**

After existing system fields:

```csharp
MissionSystem _missionSystem;
StickerSystem _stickerSystem;
MissionDef[] _missionPool;
AlbumDef _albumDef;
```

- [ ] **Step 3: Add new events**

```csharp
public event System.Action<MissionProgress> OnMissionCompleted;
public event System.Action OnAllMissionsCompleted;
public event System.Action<int> OnStickerGranted; // sticker id
```

- [ ] **Step 4: Initialize new systems in Initialize()**

Add after the existing Phase 3a system initialization:

```csharp
_missionPool = MissionConfigLoader.CreateDefaultPool();
_albumDef = StickerConfigLoader.CreateDefault();
_missionSystem = new MissionSystem(_missionPool, RngSeed + 400);
_stickerSystem = new StickerSystem(RngSeed + 500);
```

After the daily login section, add mission daily reset:

```csharp
// Daily mission reset
string missionToday = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
if (State.MissionState.Date != missionToday)
{
    State.MissionState.Date = missionToday;
    State.MissionState.Missions = _missionSystem.GenerateDaily(4);
    State.MissionState.BonusClaimed = false;
    AutoSave();
}
```

- [ ] **Step 5: Add mission tracking to DoRoll()**

In the non-jail path, after `State.Stats.TotalRolls++;`, add:

```csharp
TrackMission(MissionType.RollDice, 1);
if (resolve.Type == TileResolveType.CoinsGained)
    TrackMission(MissionType.EarnCoins, resolve.Amount);
```

In the jail roll paths, after `State.Stats.TotalRolls++;`, add:

```csharp
TrackMission(MissionType.RollDice, 1);
```

- [ ] **Step 6: Add mission tracking to DoUpgradeLandmark()**

After `OnLandmarkUpgraded?.Invoke(group, level);`, add:

```csharp
TrackMission(MissionType.BuildLandmark, 1);
GrantSticker();
```

- [ ] **Step 7: Add mission tracking to HandleRailroadEvent() heist branch**

After `State.Stats.HeistsCompleted++;`, add:

```csharp
TrackMission(MissionType.CompleteHeist, 1);
TrackMission(MissionType.EarnCoins, result.CoinsEarned);
```

- [ ] **Step 8: Add mission tracking to DoShutdownAttack()**

After `State.Stats.ShutdownsDealt++;`, add:

```csharp
TrackMission(MissionType.EarnCoins, result.CoinsEarned);
```

- [ ] **Step 9: Add TrackMission and GrantSticker helper methods**

```csharp
void TrackMission(MissionType type, int amount)
{
    if (State.MissionState?.Missions == null) return;
    _missionSystem.TrackProgress(State.MissionState.Missions, type, amount);

    // Check for newly completed missions
    foreach (var m in State.MissionState.Missions)
    {
        if (m.Completed && m.Progress - amount < m.Target)
        {
            State.Player.AddCoins(m.CoinReward);
            State.Player.AddDice(m.DiceReward);
            OnMissionCompleted?.Invoke(m);
        }
    }

    // Check all completed bonus
    if (!State.MissionState.BonusClaimed && _missionSystem.AllCompleted(State.MissionState.Missions))
    {
        State.MissionState.BonusClaimed = true;
        State.Player.AddCoins(2000);
        State.Player.AddDice(100);
        // Grant 3 stickers as bonus
        for (int i = 0; i < 3; i++)
            GrantSticker();
        OnAllMissionsCompleted?.Invoke();
    }
}

void GrantSticker()
{
    if (_albumDef == null || State.StickerState == null) return;
    int id = _stickerSystem.GrantRandom(State.StickerState, _albumDef);
    OnStickerGranted?.Invoke(id);
}
```

- [ ] **Step 10: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs
git commit -m "feat(phase4a): integrate missions and stickers into GameController"
```

---

## Task 8: MissionPanelView + UIManager/HUDView Integration

**Files:**
- Create: `Assets/Scripts/MonopolyLite/View/MissionPanelView.cs`
- Modify: `Assets/Scripts/MonopolyLite/View/UIManager.cs`
- Modify: `Assets/Scripts/MonopolyLite/View/HUDView.cs`

- [ ] **Step 1: Create MissionPanelView**

```csharp
using MonopolyLite.Core;
using MonopolyLite.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class MissionPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI[] _missionLabels;

        static readonly Color PanelBg = new Color(0.05f, 0.05f, 0.15f, 0.85f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            controller.OnMissionCompleted += _ => Refresh();
            controller.OnAllMissionsCompleted += Refresh;
            controller.OnRollComplete += (_, _) => Refresh();
            Refresh();
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("MissionPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0f, 0f);
            _panel.anchorMax = new Vector2(0f, 0f);
            _panel.anchoredPosition = new Vector2(20f, 20f);
            _panel.sizeDelta = new Vector2(320f, 200f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -15f);
            titleRt.sizeDelta = new Vector2(0f, 28f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.text = "DAILY MISSIONS";
            _titleLabel.fontSize = 18f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            _missionLabels = new TextMeshProUGUI[5];
            for (int i = 0; i < 5; i++)
            {
                var labelGo = new GameObject($"Mission_{i}", typeof(RectTransform));
                labelGo.transform.SetParent(_panel, false);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0f, 1f);
                labelRt.anchorMax = new Vector2(1f, 1f);
                labelRt.anchoredPosition = new Vector2(0f, -42f - i * 28f);
                labelRt.sizeDelta = new Vector2(-20f, 26f);
                _missionLabels[i] = labelGo.AddComponent<TextMeshProUGUI>();
                _missionLabels[i].fontSize = 14f;
                _missionLabels[i].color = Color.white;
                _missionLabels[i].text = "";
            }
        }

        void Refresh()
        {
            var missions = _controller?.State?.MissionState?.Missions;
            if (missions == null) return;

            for (int i = 0; i < _missionLabels.Length; i++)
            {
                if (i < missions.Length)
                {
                    var m = missions[i];
                    string check = m.Completed ? "[X]" : "[ ]";
                    _missionLabels[i].text = $"{check} {m.Description} ({m.Progress}/{m.Target})";
                    _missionLabels[i].color = m.Completed ? Color.green : Color.white;
                }
                else
                {
                    _missionLabels[i].text = "";
                }
            }
        }
    }
}
```

- [ ] **Step 2: Add MissionPanelView to UIManager**

In `UIManager.Initialize()`, after ShutdownPanelView creation, add:

```csharp
var missionGo = new GameObject("MissionPanelView");
missionGo.transform.SetParent(transform, false);
var missionPanel = missionGo.AddComponent<MissionPanelView>();
missionPanel.Initialize(controller, canvasRect);
```

- [ ] **Step 3: Add event handlers to HUDView**

Subscribe in `Initialize()`:

```csharp
controller.OnMissionCompleted += HandleMissionCompleted;
controller.OnAllMissionsCompleted += HandleAllMissionsCompleted;
controller.OnStickerGranted += HandleStickerGranted;
```

Add handlers:

```csharp
void HandleMissionCompleted(MissionProgress mission)
{
    _statusLabel.text = $"Mission complete: {mission.Description}!";
    RefreshStats();
}

void HandleAllMissionsCompleted()
{
    _statusLabel.text = "All daily missions complete! Bonus claimed!";
    RefreshStats();
}

void HandleStickerGranted(int stickerId)
{
    _statusLabel.text = $"New sticker! (ID: {stickerId})";
    RefreshStats();
}
```

Add `using MonopolyLite.State;` at top of HUDView if not present (needed for `MissionProgress`).

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/MissionPanelView.cs Assets/Scripts/MonopolyLite/View/UIManager.cs Assets/Scripts/MonopolyLite/View/HUDView.cs
git commit -m "feat(phase4a): add MissionPanelView, wire mission/sticker events into UI"
```

---

## Summary

| Task | Files | Tests | Description |
|---|---|---|---|
| 1 | 8 new | — | Mission + Sticker data types (enums, structs, DTOs) |
| 2 | 2 new | — | MissionState + StickerState classes |
| 3 | 1 new | 9 | MissionSystem — daily generation, progress tracking |
| 4 | 1 new | 9 | StickerSystem — collection, set/album completion |
| 5 | 2 new | — | MissionConfigLoader + StickerConfigLoader defaults |
| 6 | 3 mod | — | GameState, SaveData, SaveAdapter extensions |
| 7 | 1 mod | — | GameController: mission tracking, sticker grants |
| 8 | 1 new + 2 mod | — | MissionPanelView + UIManager/HUDView integration |

**Total: 15 new files, 6 modified files, 18 new unit tests**
