# Phase 1: Core Loop — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the playable MVP core loop — tap to roll dice, move token around a themed board, earn coins from tiles, build/upgrade landmarks, complete board.

**Architecture:** Clean rewrite of game logic. Keep existing Systems/ infrastructure (GameSystem, Services, UniTask). New modular data model with ScriptableObject configs per the spec. Pure C# game logic separated from Unity MonoBehaviour rendering for testability.

**Tech Stack:** Unity 6.0.3, C# 9.0, UniTask, NUnit (Unity Test Framework), TextMeshPro

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md`

**Agent Team Roles for This Phase:**
- **Unity Architect** — data model, assembly definitions, module boundaries
- **Game Designer** — balance values, tile layout, card definitions
- **Senior Developer** — implementation
- **Code Reviewer** — quality gate after each chunk

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   ├── MonopolyLite/
│   │   ├── Data/
│   │   │   ├── TileType.cs                    # Enum: Go, Property, Tax, Chance, CommunityChest, GoToJail, Jail, FreeParking, Railroad
│   │   │   ├── ColorGroup.cs                  # Enum: None, Brown, LightBlue, Pink, Orange, Red, Yellow, Green, Blue
│   │   │   ├── CardType.cs                    # Enum: GainCoins, LoseCoins, GoToTile, GoToJail, GainDice, GainShield
│   │   │   ├── TileDef.cs                     # Struct: name, type, colorGroup, baseReward, taxAmount
│   │   │   ├── CardDef.cs                     # Struct: type, description, amount, tileIndex
│   │   │   ├── LandmarkDef.cs                 # Struct: colorGroup, name, costs[5], nwPoints[5]
│   │   │   └── BoardDef.cs                    # Class: theme, tiles[], landmarks[], chanceCards[], communityChestCards[], boardMultiplier
│   │   ├── Config/
│   │   │   ├── BoardConfig.cs                 # ScriptableObject wrapping BoardDef
│   │   │   └── BoardConfigLoader.cs           # Loads BoardDef from JSON, fallback to default
│   │   ├── State/
│   │   │   ├── PlayerState.cs                 # Class: coins, dice, position, shields, netWorth, multiplier, jailTurnsLeft
│   │   │   ├── BoardState.cs                  # Class: landmarkLevels[], cardDeckIndices, currentBoard
│   │   │   └── GameState.cs                   # Class: PlayerState + BoardState, facade methods
│   │   ├── Logic/
│   │   │   ├── DiceSystem.cs                  # Dice resource: consume, regen, cap, multiplier cost
│   │   │   ├── MovementSystem.cs              # Move token N steps, handle GO pass
│   │   │   ├── TileResolver.cs                # Resolve tile landing effects (coins, tax, jail, cards)
│   │   │   ├── CardSystem.cs                  # Draw card from shuffled deck, apply effect
│   │   │   ├── LandmarkSystem.cs              # Build/upgrade landmarks, check board completion
│   │   │   └── JailSystem.cs                  # Jail entry/exit logic (wait, doubles, dice cost)
│   │   ├── Core/
│   │   │   ├── GameController.cs              # MonoBehaviour: orchestrates game loop, connects logic to rendering
│   │   │   └── Bootstrap.cs                   # REWRITE: entry point, init GameController + Services
│   │   └── View/
│   │       ├── BoardRenderer.cs               # Renders board tiles using Layout helper
│   │       ├── TokenRenderer.cs               # Renders and animates player token
│   │       ├── LandmarkRenderer.cs            # Renders landmark visuals on board
│   │       ├── UIManager.cs                   # Screen management: HUD, popups, overlays
│   │       ├── HUDView.cs                     # Dice count, coin count, multiplier, shields
│   │       └── LandmarkPanelView.cs           # Landmark build/upgrade UI
├── Resources/
│   └── Boards/
│       └── board_01_istanbul.json             # Board 1 definition
├── Tests/
│   └── EditMode/
│       ├── EditModeTests.asmdef               # Assembly definition for edit mode tests
│       ├── DiceSystemTests.cs                 # Tests for DiceSystem
│       ├── MovementSystemTests.cs             # Tests for MovementSystem
│       ├── TileResolverTests.cs               # Tests for TileResolver
│       ├── CardSystemTests.cs                 # Tests for CardSystem
│       ├── LandmarkSystemTests.cs             # Tests for LandmarkSystem
│       ├── JailSystemTests.cs                 # Tests for JailSystem
│       └── GameStateTests.cs                  # Tests for GameState
```

### Existing Files (Modify/Delete)

| File | Action |
|---|---|
| `Assets/Scripts/MonopolyLite/Core/Main.cs` | DELETE |
| `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs` | DELETE |
| `Assets/Scripts/MonopolyLite/Core/Main.Input.cs` | DELETE |
| `Assets/Scripts/MonopolyLite/Core/Recorder.cs` | DELETE |
| `Assets/Scripts/MonopolyLite/Core/Bootstrap.cs` | REWRITE (Task 9) |
| `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs` | DELETE (replaced by BoardConfig) |
| `Assets/Scripts/MonopolyLite/Shared/GameConfigJson.cs` | DELETE (replaced by BoardConfigLoader) |
| `Assets/Scripts/MonopolyLite/Shared/TileDef.cs` | DELETE (new version in Data/) |
| `Assets/Scripts/MonopolyLite/Shared/TileType.cs` | DELETE (new version in Data/) |
| `Assets/Scripts/MonopolyLite/Shared/ConfigLoader.cs` | DELETE (replaced by BoardConfigLoader) |
| `Assets/Scripts/MonopolyLite/Shared/Helpers.cs` | KEEP |
| `Assets/Resources/gameconfig.json` | DELETE (replaced by Boards/board_01_istanbul.json) |

---

## Task 1: Project Cleanup & Test Infrastructure

**Files:**
- Delete: `Assets/Scripts/MonopolyLite/Core/Main.cs`, `Main.Logic.cs`, `Main.Input.cs`, `Recorder.cs`
- Delete: `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs`, `GameConfigJson.cs`, `TileDef.cs`, `TileType.cs`, `ConfigLoader.cs`
- Delete: `Assets/Resources/gameconfig.json`
- Create: `Assets/Tests/EditMode/EditModeTests.asmdef`

- [ ] **Step 1: Delete old game logic files**

```bash
rm Assets/Scripts/MonopolyLite/Core/Main.cs
rm Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
rm Assets/Scripts/MonopolyLite/Core/Main.Input.cs
rm Assets/Scripts/MonopolyLite/Core/Recorder.cs
rm Assets/Scripts/MonopolyLite/Core/Main.cs.meta
rm Assets/Scripts/MonopolyLite/Core/Main.Logic.cs.meta
rm Assets/Scripts/MonopolyLite/Core/Main.Input.cs.meta
rm Assets/Scripts/MonopolyLite/Core/Recorder.cs.meta
```

- [ ] **Step 2: Delete old shared data model files**

```bash
rm Assets/Scripts/MonopolyLite/Shared/GameConfig.cs
rm Assets/Scripts/MonopolyLite/Shared/GameConfigJson.cs
rm Assets/Scripts/MonopolyLite/Shared/TileDef.cs
rm Assets/Scripts/MonopolyLite/Shared/TileType.cs
rm Assets/Scripts/MonopolyLite/Shared/ConfigLoader.cs
rm Assets/Scripts/MonopolyLite/Shared/GameConfig.cs.meta
rm Assets/Scripts/MonopolyLite/Shared/TileDef.cs.meta
rm Assets/Scripts/MonopolyLite/Shared/TileType.cs.meta
rm Assets/Scripts/MonopolyLite/Shared/ConfigLoader.cs.meta
rm Assets/Resources/gameconfig.json
rm Assets/Resources/gameconfig.json.meta
```

- [ ] **Step 3: Create directory structure**

```bash
mkdir -p Assets/Scripts/MonopolyLite/Data
mkdir -p Assets/Scripts/MonopolyLite/Config
mkdir -p Assets/Scripts/MonopolyLite/State
mkdir -p Assets/Scripts/MonopolyLite/Logic
mkdir -p Assets/Scripts/MonopolyLite/View
mkdir -p Assets/Resources/Boards
mkdir -p Assets/Tests/EditMode
```

- [ ] **Step 4: Create test assembly definition**

Create `Assets/Tests/EditMode/EditModeTests.asmdef`:
```json
{
    "name": "EditModeTests",
    "rootNamespace": "MonopolyLite.Tests",
    "references": [],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Note: Since the project uses no `.asmdef` files (everything is in Assembly-CSharp), the test assembly can reference game code via the default assembly. No game-side `.asmdef` needed for now.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "chore: clean old game logic, set up test infrastructure for Monopoly Go redesign"
```

---

## Task 2: Data Model — Enums & Structs

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Data/TileType.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/ColorGroup.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/CardType.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/TileDef.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/CardDef.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/LandmarkDef.cs`
- Create: `Assets/Scripts/MonopolyLite/Data/BoardDef.cs`

- [ ] **Step 1: Create TileType enum**

```csharp
// Assets/Scripts/MonopolyLite/Data/TileType.cs
namespace MonopolyLite.Data
{
    public enum TileType
    {
        Go,
        Property,
        Tax,
        Chance,
        CommunityChest,
        GoToJail,
        Jail,
        FreeParking,
        Railroad
    }
}
```

- [ ] **Step 2: Create ColorGroup enum**

```csharp
// Assets/Scripts/MonopolyLite/Data/ColorGroup.cs
namespace MonopolyLite.Data
{
    public enum ColorGroup
    {
        None,
        Brown,
        LightBlue,
        Pink,
        Orange,
        Red,
        Yellow,
        Green,
        Blue
    }
}
```

- [ ] **Step 3: Create CardType enum**

```csharp
// Assets/Scripts/MonopolyLite/Data/CardType.cs
namespace MonopolyLite.Data
{
    public enum CardType
    {
        GainCoins,
        LoseCoins,
        GoToTile,
        GoToJail,
        GainDice,
        GainShield
    }
}
```

- [ ] **Step 4: Create TileDef struct**

```csharp
// Assets/Scripts/MonopolyLite/Data/TileDef.cs
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct TileDef
    {
        public string name;
        public TileType type;
        public ColorGroup colorGroup;
        public int baseReward;   // coins earned on landing (Property tiles)
        public int taxAmount;    // coins lost on landing (Tax tiles)
    }
}
```

- [ ] **Step 5: Create CardDef struct**

```csharp
// Assets/Scripts/MonopolyLite/Data/CardDef.cs
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct CardDef
    {
        public CardType type;
        public string description;
        public int amount;      // coins/dice/shields gained or lost
        public int tileIndex;   // target tile for GoToTile type
    }
}
```

- [ ] **Step 6: Create LandmarkDef struct**

```csharp
// Assets/Scripts/MonopolyLite/Data/LandmarkDef.cs
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct LandmarkDef
    {
        public ColorGroup colorGroup;
        public string name;
        public int[] costs;      // cost per level [0..4] for levels 1-5
        public int[] nwPoints;   // net worth granted per level [0..4] for levels 1-5
    }
}
```

- [ ] **Step 7: Create BoardDef class**

```csharp
// Assets/Scripts/MonopolyLite/Data/BoardDef.cs
using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class BoardDef
    {
        public string id;
        public string theme;
        public float sideLength;
        public float tileSize;
        public int jailTileIndex;
        public int goTileIndex;
        public int goBonus;               // coins earned passing GO
        public float boardMultiplier;     // cost/reward scale vs board 1
        public TileDef[] tiles;
        public LandmarkDef[] landmarks;
        public CardDef[] chanceCards;
        public CardDef[] communityChestCards;
    }
}
```

- [ ] **Step 8: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Data/
git commit -m "feat: add data model enums and structs for Monopoly Go redesign"
```

---

## Task 3: Board Config — ScriptableObject & JSON Loader

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Config/BoardConfig.cs`
- Create: `Assets/Scripts/MonopolyLite/Config/BoardConfigLoader.cs`
- Create: `Assets/Resources/Boards/board_01_istanbul.json`

- [ ] **Step 1: Create BoardConfig ScriptableObject**

```csharp
// Assets/Scripts/MonopolyLite/Config/BoardConfig.cs
using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "MonopolyLite/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public BoardDef board;
    }
}
```

- [ ] **Step 2: Create BoardConfigLoader**

```csharp
// Assets/Scripts/MonopolyLite/Config/BoardConfigLoader.cs
using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    public static class BoardConfigLoader
    {
        public static BoardDef Load(string boardId)
        {
            var textAsset = Resources.Load<TextAsset>($"Boards/{boardId}");
            if (textAsset != null)
            {
                return JsonUtility.FromJson<BoardDef>(textAsset.text);
            }
            Debug.LogWarning($"Board config not found: {boardId}, using default");
            return CreateDefault();
        }

        public static BoardDef CreateDefault()
        {
            return new BoardDef
            {
                id = "board_01_istanbul",
                theme = "Istanbul",
                sideLength = 16f,
                tileSize = 1.4f,
                jailTileIndex = 8,
                goTileIndex = 0,
                goBonus = 200,
                boardMultiplier = 1.0f,
                tiles = CreateDefaultTiles(),
                landmarks = CreateDefaultLandmarks(),
                chanceCards = CreateDefaultChanceCards(),
                communityChestCards = CreateDefaultCommunityChestCards()
            };
        }

        static TileDef[] CreateDefaultTiles()
        {
            return new[]
            {
                new TileDef { name = "GO",                type = TileType.Go,             colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Sultanahmet",       type = TileType.Property,       colorGroup = ColorGroup.Brown,     baseReward = 50,  taxAmount = 0 },
                new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Balat",             type = TileType.Property,       colorGroup = ColorGroup.Brown,     baseReward = 60,  taxAmount = 0 },
                new TileDef { name = "Income Tax",        type = TileType.Tax,            colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 150 },
                new TileDef { name = "Haydarpasa",        type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 100, taxAmount = 0 },
                new TileDef { name = "Kadikoy",           type = TileType.Property,       colorGroup = ColorGroup.LightBlue, baseReward = 80,  taxAmount = 0 },
                new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Jail",              type = TileType.Jail,           colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Moda",              type = TileType.Property,       colorGroup = ColorGroup.LightBlue, baseReward = 90,  taxAmount = 0 },
                new TileDef { name = "Besiktas",          type = TileType.Property,       colorGroup = ColorGroup.Pink,      baseReward = 100, taxAmount = 0 },
                new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Ortakoy",           type = TileType.Property,       colorGroup = ColorGroup.Pink,      baseReward = 110, taxAmount = 0 },
                new TileDef { name = "Sirkeci",           type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 100, taxAmount = 0 },
                new TileDef { name = "Bebek",             type = TileType.Property,       colorGroup = ColorGroup.Orange,    baseReward = 120, taxAmount = 0 },
                new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Free Parking",      type = TileType.FreeParking,    colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Nisantasi",         type = TileType.Property,       colorGroup = ColorGroup.Orange,    baseReward = 130, taxAmount = 0 },
                new TileDef { name = "Etiler",            type = TileType.Property,       colorGroup = ColorGroup.Red,       baseReward = 140, taxAmount = 0 },
                new TileDef { name = "Community Chest",   type = TileType.CommunityChest, colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Levent",            type = TileType.Property,       colorGroup = ColorGroup.Red,       baseReward = 150, taxAmount = 0 },
                new TileDef { name = "Eminonu",           type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 100, taxAmount = 0 },
                new TileDef { name = "Taksim",            type = TileType.Property,       colorGroup = ColorGroup.Yellow,    baseReward = 160, taxAmount = 0 },
                new TileDef { name = "Chance",            type = TileType.Chance,         colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Go To Jail",        type = TileType.GoToJail,       colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 0 },
                new TileDef { name = "Istiklal",          type = TileType.Property,       colorGroup = ColorGroup.Yellow,    baseReward = 170, taxAmount = 0 },
                new TileDef { name = "Galata",            type = TileType.Property,       colorGroup = ColorGroup.Green,     baseReward = 180, taxAmount = 0 },
                new TileDef { name = "Luxury Tax",        type = TileType.Tax,            colorGroup = ColorGroup.None,      baseReward = 0,   taxAmount = 200 },
                new TileDef { name = "Karakoy",           type = TileType.Property,       colorGroup = ColorGroup.Green,     baseReward = 190, taxAmount = 0 },
                new TileDef { name = "Kabatas",           type = TileType.Railroad,       colorGroup = ColorGroup.None,      baseReward = 100, taxAmount = 0 },
                new TileDef { name = "Uskudar",           type = TileType.Property,       colorGroup = ColorGroup.Blue,      baseReward = 200, taxAmount = 0 },
                new TileDef { name = "Beylerbeyi",        type = TileType.Property,       colorGroup = ColorGroup.Blue,      baseReward = 210, taxAmount = 0 },
            };
        }

        static LandmarkDef[] CreateDefaultLandmarks()
        {
            return new[]
            {
                new LandmarkDef { colorGroup = ColorGroup.Brown,     name = "Hagia Sophia",      costs = new[] { 500, 1200, 3000, 7000, 15000 },      nwPoints = new[] { 100, 300, 600, 1200, 2500 } },
                new LandmarkDef { colorGroup = ColorGroup.LightBlue, name = "Maiden's Tower",    costs = new[] { 600, 1400, 3500, 8000, 18000 },      nwPoints = new[] { 100, 300, 600, 1200, 2500 } },
                new LandmarkDef { colorGroup = ColorGroup.Pink,      name = "Dolmabahce Palace", costs = new[] { 700, 1600, 4000, 9000, 20000 },      nwPoints = new[] { 120, 350, 700, 1400, 2800 } },
                new LandmarkDef { colorGroup = ColorGroup.Orange,    name = "Topkapi Palace",    costs = new[] { 800, 1800, 4500, 10000, 22000 },     nwPoints = new[] { 140, 400, 800, 1600, 3200 } },
                new LandmarkDef { colorGroup = ColorGroup.Red,       name = "Blue Mosque",       costs = new[] { 900, 2000, 5000, 11000, 25000 },     nwPoints = new[] { 160, 450, 900, 1800, 3600 } },
                new LandmarkDef { colorGroup = ColorGroup.Yellow,    name = "Galata Tower",      costs = new[] { 1000, 2200, 5500, 12000, 28000 },    nwPoints = new[] { 180, 500, 1000, 2000, 4000 } },
                new LandmarkDef { colorGroup = ColorGroup.Green,     name = "Grand Bazaar",      costs = new[] { 1200, 2500, 6000, 14000, 32000 },    nwPoints = new[] { 200, 550, 1100, 2200, 4400 } },
                new LandmarkDef { colorGroup = ColorGroup.Blue,      name = "Bosphorus Bridge",  costs = new[] { 1500, 3000, 7000, 16000, 38000 },    nwPoints = new[] { 250, 650, 1300, 2600, 5200 } },
            };
        }

        static CardDef[] CreateDefaultChanceCards()
        {
            return new[]
            {
                new CardDef { type = CardType.GainCoins,  description = "Street vendor tips!",         amount = 150,  tileIndex = 0 },
                new CardDef { type = CardType.GainCoins,  description = "Won a backgammon bet!",       amount = 200,  tileIndex = 0 },
                new CardDef { type = CardType.LoseCoins,  description = "Taxi overcharged you!",       amount = 100,  tileIndex = 0 },
                new CardDef { type = CardType.LoseCoins,  description = "Lost your wallet at bazaar!", amount = 250,  tileIndex = 0 },
                new CardDef { type = CardType.GoToTile,   description = "Ferry to Kadikoy!",           amount = 0,    tileIndex = 6 },
                new CardDef { type = CardType.GoToTile,   description = "Advance to GO!",              amount = 0,    tileIndex = 0 },
                new CardDef { type = CardType.GoToJail,   description = "Caught jaywalking!",          amount = 0,    tileIndex = 0 },
                new CardDef { type = CardType.GainDice,   description = "Found extra dice!",           amount = 30,   tileIndex = 0 },
                new CardDef { type = CardType.GainDice,   description = "Lucky cat cafe visit!",       amount = 20,   tileIndex = 0 },
                new CardDef { type = CardType.GainShield, description = "Bodyguard hired!",            amount = 1,    tileIndex = 0 },
            };
        }

        static CardDef[] CreateDefaultCommunityChestCards()
        {
            return new[]
            {
                new CardDef { type = CardType.GainCoins,  description = "Simit sales bonus!",          amount = 100,  tileIndex = 0 },
                new CardDef { type = CardType.GainCoins,  description = "Tea garden profits!",          amount = 180,  tileIndex = 0 },
                new CardDef { type = CardType.GainCoins,  description = "Carpet export deal!",          amount = 300,  tileIndex = 0 },
                new CardDef { type = CardType.LoseCoins,  description = "Baklava addiction!",           amount = 80,   tileIndex = 0 },
                new CardDef { type = CardType.LoseCoins,  description = "Parking ticket!",              amount = 120,  tileIndex = 0 },
                new CardDef { type = CardType.GoToTile,   description = "Visit Taksim!",                amount = 0,    tileIndex = 22 },
                new CardDef { type = CardType.GoToJail,   description = "Tax evasion caught!",          amount = 0,    tileIndex = 0 },
                new CardDef { type = CardType.GainDice,   description = "Generous neighbor!",           amount = 25,   tileIndex = 0 },
                new CardDef { type = CardType.GainShield, description = "Neighborhood watch!",          amount = 1,    tileIndex = 0 },
                new CardDef { type = CardType.GainCoins,  description = "Tulip festival prize!",        amount = 250,  tileIndex = 0 },
            };
        }
    }
}
```

- [ ] **Step 3: Create Board 1 JSON**

Create `Assets/Resources/Boards/board_01_istanbul.json` — serialize the same data as `CreateDefault()` above using `JsonUtility.ToJson(CreateDefault(), true)`. Run this once in a Unity Editor script or create the JSON manually matching the BoardDef structure.

For initial development, the `CreateDefault()` fallback is sufficient. JSON file can be generated later.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Config/ Assets/Resources/Boards/
git commit -m "feat: add BoardConfig ScriptableObject and JSON loader with Istanbul board"
```

---

## Task 4: Game State

**Files:**
- Create: `Assets/Scripts/MonopolyLite/State/PlayerState.cs`
- Create: `Assets/Scripts/MonopolyLite/State/BoardState.cs`
- Create: `Assets/Scripts/MonopolyLite/State/GameState.cs`
- Test: `Assets/Tests/EditMode/GameStateTests.cs`

- [ ] **Step 1: Write failing test for GameState**

```csharp
// Assets/Tests/EditMode/GameStateTests.cs
using NUnit.Framework;
using MonopolyLite.State;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class GameStateTests
    {
        [Test]
        public void NewGameState_HasCorrectDefaults()
        {
            var board = new BoardDef
            {
                goBonus = 200,
                tiles = new TileDef[32],
                landmarks = new LandmarkDef[]
                {
                    new LandmarkDef { colorGroup = ColorGroup.Brown, costs = new[] { 500, 1000, 2000, 4000, 8000 }, nwPoints = new[] { 100, 300, 600, 1200, 2500 } },
                }
            };
            var state = new GameState(board, startingDice: 100, diceCap: 1000);

            Assert.AreEqual(0, state.Player.Coins);
            Assert.AreEqual(100, state.Player.Dice);
            Assert.AreEqual(0, state.Player.Position);
            Assert.AreEqual(0, state.Player.Shields);
            Assert.AreEqual(0, state.Player.NetWorth);
            Assert.AreEqual(1, state.Player.Multiplier);
            Assert.AreEqual(0, state.Player.JailTurnsLeft);
            Assert.AreEqual(1000, state.Player.DiceCap);
            Assert.AreEqual(0, state.Board.GetLandmarkLevel(ColorGroup.Brown));
        }

        [Test]
        public void PlayerState_AddCoins_IncreasesCoins()
        {
            var player = new PlayerState(startingDice: 100, diceCap: 1000);
            player.AddCoins(500);
            Assert.AreEqual(500, player.Coins);
        }

        [Test]
        public void PlayerState_SpendCoins_DecreasesCoins()
        {
            var player = new PlayerState(startingDice: 100, diceCap: 1000);
            player.AddCoins(500);
            bool success = player.SpendCoins(200);
            Assert.IsTrue(success);
            Assert.AreEqual(300, player.Coins);
        }

        [Test]
        public void PlayerState_SpendCoins_FailsWhenInsufficient()
        {
            var player = new PlayerState(startingDice: 100, diceCap: 1000);
            player.AddCoins(100);
            bool success = player.SpendCoins(200);
            Assert.IsFalse(success);
            Assert.AreEqual(100, player.Coins);
        }

        [Test]
        public void PlayerState_ConsumeDice_RespectsMultiplier()
        {
            var player = new PlayerState(startingDice: 100, diceCap: 1000);
            player.Multiplier = 5;
            bool success = player.ConsumeDice();
            Assert.IsTrue(success);
            Assert.AreEqual(95, player.Dice);
        }

        [Test]
        public void PlayerState_ConsumeDice_FailsWhenInsufficient()
        {
            var player = new PlayerState(startingDice: 3, diceCap: 1000);
            player.Multiplier = 5;
            bool success = player.ConsumeDice();
            Assert.IsFalse(success);
            Assert.AreEqual(3, player.Dice);
        }

        [Test]
        public void PlayerState_AddDice_RespectsCapFromCap()
        {
            var player = new PlayerState(startingDice: 990, diceCap: 1000);
            player.AddDice(50);
            Assert.AreEqual(1000, player.Dice);
        }

        [Test]
        public void BoardState_GetSetLandmarkLevel()
        {
            var landmarks = new[] { new LandmarkDef { colorGroup = ColorGroup.Brown } };
            var boardState = new BoardState(landmarks);
            Assert.AreEqual(0, boardState.GetLandmarkLevel(ColorGroup.Brown));
            boardState.SetLandmarkLevel(ColorGroup.Brown, 3);
            Assert.AreEqual(3, boardState.GetLandmarkLevel(ColorGroup.Brown));
        }

        [Test]
        public void BoardState_IsComplete_WhenAllLandmarksMaxLevel()
        {
            var landmarks = new[]
            {
                new LandmarkDef { colorGroup = ColorGroup.Brown },
                new LandmarkDef { colorGroup = ColorGroup.Blue },
            };
            var boardState = new BoardState(landmarks);
            Assert.IsFalse(boardState.IsComplete());
            boardState.SetLandmarkLevel(ColorGroup.Brown, 5);
            Assert.IsFalse(boardState.IsComplete());
            boardState.SetLandmarkLevel(ColorGroup.Blue, 5);
            Assert.IsTrue(boardState.IsComplete());
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Open Unity → Window → General → Test Runner → EditMode → Run All
Expected: All tests FAIL (classes don't exist yet)

- [ ] **Step 3: Implement PlayerState**

```csharp
// Assets/Scripts/MonopolyLite/State/PlayerState.cs
namespace MonopolyLite.State
{
    public class PlayerState
    {
        public int Coins { get; private set; }
        public int Dice { get; private set; }
        public int DiceCap { get; private set; }
        public int Position { get; set; }
        public int Shields { get; set; }
        public int NetWorth { get; set; }
        public int Multiplier { get; set; } = 1;
        public int JailTurnsLeft { get; set; }

        public PlayerState(int startingDice, int diceCap)
        {
            Dice = startingDice;
            DiceCap = diceCap;
        }

        public void AddCoins(int amount)
        {
            Coins += amount;
        }

        public bool SpendCoins(int amount)
        {
            if (Coins < amount) return false;
            Coins -= amount;
            return true;
        }

        public bool ConsumeDice()
        {
            int cost = Multiplier;
            if (Dice < cost) return false;
            Dice -= cost;
            return true;
        }

        public void AddDice(int amount)
        {
            Dice = System.Math.Min(Dice + amount, DiceCap);
        }

        public bool SpendDice(int amount)
        {
            if (Dice < amount) return false;
            Dice -= amount;
            return true;
        }

        public void AddShield(int count = 1)
        {
            Shields = System.Math.Min(Shields + count, 3);
        }
    }
}
```

- [ ] **Step 4: Implement BoardState**

```csharp
// Assets/Scripts/MonopolyLite/State/BoardState.cs
using System.Collections.Generic;
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class BoardState
    {
        readonly Dictionary<ColorGroup, int> _landmarkLevels = new();
        readonly LandmarkDef[] _landmarks;

        public int ChanceDrawIndex { get; set; }
        public int CommunityChestDrawIndex { get; set; }

        public BoardState(LandmarkDef[] landmarks)
        {
            _landmarks = landmarks;
            foreach (var lm in landmarks)
                _landmarkLevels[lm.colorGroup] = 0;
        }

        public int GetLandmarkLevel(ColorGroup group)
        {
            return _landmarkLevels.TryGetValue(group, out int level) ? level : 0;
        }

        public void SetLandmarkLevel(ColorGroup group, int level)
        {
            _landmarkLevels[group] = System.Math.Clamp(level, 0, 5);
        }

        public bool IsComplete()
        {
            foreach (var lm in _landmarks)
            {
                if (GetLandmarkLevel(lm.colorGroup) < 5) return false;
            }
            return true;
        }
    }
}
```

- [ ] **Step 5: Implement GameState**

```csharp
// Assets/Scripts/MonopolyLite/State/GameState.cs
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class GameState
    {
        public PlayerState Player { get; }
        public BoardState Board { get; }
        public BoardDef BoardDef { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
        }
    }
}
```

- [ ] **Step 6: Run tests to verify they pass**

Run: Unity Test Runner → EditMode → Run All
Expected: All 9 tests PASS

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/State/ Assets/Tests/EditMode/GameStateTests.cs
git commit -m "feat: add PlayerState, BoardState, GameState with tests"
```

---

## Task 5: DiceSystem Logic

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/DiceSystem.cs`
- Test: `Assets/Tests/EditMode/DiceSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// Assets/Tests/EditMode/DiceSystemTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class DiceSystemTests
    {
        PlayerState _player;

        [SetUp]
        public void SetUp()
        {
            _player = new PlayerState(startingDice: 100, diceCap: 1000);
        }

        [Test]
        public void Roll_ReturnsDiceTotal_Between2And12()
        {
            var dice = new DiceSystem(seed: 42);
            var result = dice.Roll(_player);
            Assert.IsTrue(result.Success);
            Assert.GreaterOrEqual(result.Total, 2);
            Assert.LessOrEqual(result.Total, 12);
            Assert.GreaterOrEqual(result.Die1, 1);
            Assert.LessOrEqual(result.Die1, 6);
            Assert.GreaterOrEqual(result.Die2, 1);
            Assert.LessOrEqual(result.Die2, 6);
        }

        [Test]
        public void Roll_ConsumesDiceByMultiplier()
        {
            var dice = new DiceSystem(seed: 42);
            _player.Multiplier = 5;
            dice.Roll(_player);
            Assert.AreEqual(95, _player.Dice);
        }

        [Test]
        public void Roll_FailsWhenNotEnoughDice()
        {
            var player = new PlayerState(startingDice: 2, diceCap: 1000);
            player.Multiplier = 5;
            var dice = new DiceSystem(seed: 42);
            var result = dice.Roll(player);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, player.Dice);
        }

        [Test]
        public void Roll_IsReproducibleWithSameSeed()
        {
            var dice1 = new DiceSystem(seed: 123);
            var dice2 = new DiceSystem(seed: 123);
            var p1 = new PlayerState(startingDice: 100, diceCap: 1000);
            var p2 = new PlayerState(startingDice: 100, diceCap: 1000);
            var r1 = dice1.Roll(p1);
            var r2 = dice2.Roll(p2);
            Assert.AreEqual(r1.Die1, r2.Die1);
            Assert.AreEqual(r1.Die2, r2.Die2);
        }

        [Test]
        public void Roll_DetectsDoubles()
        {
            // Keep rolling until we get doubles or exhaust attempts
            var dice = new DiceSystem(seed: 0);
            bool foundDoubles = false;
            bool foundNonDoubles = false;
            for (int i = 0; i < 100; i++)
            {
                var p = new PlayerState(startingDice: 10000, diceCap: 10000);
                var diceInst = new DiceSystem(seed: i);
                var r = diceInst.Roll(p);
                if (r.IsDoubles) foundDoubles = true;
                else foundNonDoubles = true;
                if (foundDoubles && foundNonDoubles) break;
            }
            Assert.IsTrue(foundDoubles, "Should find at least one doubles in 100 rolls");
            Assert.IsTrue(foundNonDoubles, "Should find at least one non-doubles in 100 rolls");
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL (DiceSystem class doesn't exist)

- [ ] **Step 3: Implement DiceSystem**

```csharp
// Assets/Scripts/MonopolyLite/Logic/DiceSystem.cs
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public struct RollResult
    {
        public bool Success;
        public int Die1;
        public int Die2;
        public int Total;
        public bool IsDoubles;
    }

    public class DiceSystem
    {
        RNG _rng;

        public DiceSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public RollResult Roll(PlayerState player)
        {
            if (!player.ConsumeDice())
                return new RollResult { Success = false };

            int die1 = _rng.Int(1, 7); // inclusive min, exclusive max
            int die2 = _rng.Int(1, 7);
            return new RollResult
            {
                Success = true,
                Die1 = die1,
                Die2 = die2,
                Total = die1 + die2,
                IsDoubles = die1 == die2
            };
        }
    }
}
```

Note: Uses existing `RNG` struct from `Assets/Scripts/MonopolyLite/Shared/Helpers.cs` (PCG-XSH-RR implementation). `RNG.Int(min, max)` returns `[min, max)`.

- [ ] **Step 4: Run tests to verify they pass**

Expected: All 5 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/DiceSystem.cs Assets/Tests/EditMode/DiceSystemTests.cs
git commit -m "feat: add DiceSystem with PCG RNG and multiplier support"
```

---

## Task 6: MovementSystem Logic

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/MovementSystem.cs`
- Test: `Assets/Tests/EditMode/MovementSystemTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// Assets/Tests/EditMode/MovementSystemTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;
using MonopolyLite.Data;

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

        [Test]
        public void Move_AdvancesPosition()
        {
            var move = new MovementSystem();
            var result = move.Move(_state, 5);
            Assert.AreEqual(5, _state.Player.Position);
            Assert.IsFalse(result.PassedGo);
        }

        [Test]
        public void Move_WrapsAroundBoard()
        {
            var move = new MovementSystem();
            _state.Player.Position = 30;
            var result = move.Move(_state, 5);
            Assert.AreEqual(3, _state.Player.Position);
            Assert.IsTrue(result.PassedGo);
        }

        [Test]
        public void Move_GrantsGoBonusOnPass()
        {
            var move = new MovementSystem();
            _state.Player.Position = 30;
            move.Move(_state, 5);
            Assert.AreEqual(200, _state.Player.Coins); // goBonus × multiplier(1)
        }

        [Test]
        public void Move_GoBonusScalesWithMultiplier()
        {
            var move = new MovementSystem();
            _state.Player.Position = 30;
            _state.Player.Multiplier = 5;
            move.Move(_state, 5);
            Assert.AreEqual(1000, _state.Player.Coins); // 200 × 5
        }

        [Test]
        public void MoveToTile_DirectMovement_NoGoBonus()
        {
            var move = new MovementSystem();
            _state.Player.Position = 10;
            move.MoveToTile(_state, 5, grantGoBonus: false);
            Assert.AreEqual(5, _state.Player.Position);
            Assert.AreEqual(0, _state.Player.Coins);
        }

        [Test]
        public void MoveToTile_Forward_WithGoBonus()
        {
            var move = new MovementSystem();
            _state.Player.Position = 30;
            move.MoveToTile(_state, 5, grantGoBonus: true);
            Assert.AreEqual(5, _state.Player.Position);
            Assert.AreEqual(200, _state.Player.Coins);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

- [ ] **Step 3: Implement MovementSystem**

```csharp
// Assets/Scripts/MonopolyLite/Logic/MovementSystem.cs
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public struct MoveResult
    {
        public bool PassedGo;
        public int LandedTileIndex;
    }

    public class MovementSystem
    {
        public MoveResult Move(GameState state, int steps)
        {
            int boardSize = state.BoardDef.tiles.Length;
            int oldPos = state.Player.Position;
            int newPos = (oldPos + steps) % boardSize;
            bool passedGo = newPos < oldPos;

            state.Player.Position = newPos;

            if (passedGo)
            {
                int bonus = state.BoardDef.goBonus * state.Player.Multiplier;
                state.Player.AddCoins(bonus);
            }

            return new MoveResult { PassedGo = passedGo, LandedTileIndex = newPos };
        }

        public void MoveToTile(GameState state, int tileIndex, bool grantGoBonus)
        {
            int boardSize = state.BoardDef.tiles.Length;
            int oldPos = state.Player.Position;
            bool passedGo = grantGoBonus && tileIndex < oldPos;

            state.Player.Position = tileIndex;

            if (passedGo)
            {
                int bonus = state.BoardDef.goBonus * state.Player.Multiplier;
                state.Player.AddCoins(bonus);
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All 6 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/MovementSystem.cs Assets/Tests/EditMode/MovementSystemTests.cs
git commit -m "feat: add MovementSystem with GO pass bonus and direct movement"
```

---

## Task 7: CardSystem & JailSystem Logic

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/CardSystem.cs`
- Create: `Assets/Scripts/MonopolyLite/Logic/JailSystem.cs`
- Test: `Assets/Tests/EditMode/CardSystemTests.cs`
- Test: `Assets/Tests/EditMode/JailSystemTests.cs`

- [ ] **Step 1: Write failing CardSystem tests**

```csharp
// Assets/Tests/EditMode/CardSystemTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class CardSystemTests
    {
        GameState _state;
        CardSystem _cardSystem;

        [SetUp]
        public void SetUp()
        {
            var board = new BoardDef
            {
                tiles = new TileDef[32],
                goTileIndex = 0,
                goBonus = 200,
                jailTileIndex = 8,
                landmarks = new LandmarkDef[0],
                chanceCards = new[]
                {
                    new CardDef { type = CardType.GainCoins, description = "Win!", amount = 100 },
                    new CardDef { type = CardType.LoseCoins, description = "Lose!", amount = 50 },
                    new CardDef { type = CardType.GainDice, description = "Dice!", amount = 20 },
                    new CardDef { type = CardType.GainShield, description = "Shield!", amount = 1 },
                    new CardDef { type = CardType.GoToJail, description = "Jail!", amount = 0 },
                    new CardDef { type = CardType.GoToTile, description = "Move!", amount = 0, tileIndex = 10 },
                },
                communityChestCards = new[]
                {
                    new CardDef { type = CardType.GainCoins, description = "Bonus!", amount = 200 },
                }
            };
            _state = new GameState(board, startingDice: 100, diceCap: 1000);
            _cardSystem = new CardSystem(seed: 42);
        }

        [Test]
        public void DrawChance_ReturnsCardAndAdvancesIndex()
        {
            var card = _cardSystem.DrawChance(_state);
            Assert.IsNotNull(card.description);
            Assert.AreEqual(1, _state.Board.ChanceDrawIndex);
        }

        [Test]
        public void DrawChance_WrapsAroundDeck()
        {
            for (int i = 0; i < 6; i++) _cardSystem.DrawChance(_state);
            Assert.AreEqual(0, _state.Board.ChanceDrawIndex); // wrapped after 6 cards
        }

        [Test]
        public void ApplyCard_GainCoins_ScalesWithMultiplier()
        {
            var card = new CardDef { type = CardType.GainCoins, amount = 100 };
            _state.Player.Multiplier = 3;
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(300, _state.Player.Coins);
        }

        [Test]
        public void ApplyCard_LoseCoins_DoesNotScaleWithMultiplier()
        {
            _state.Player.AddCoins(500);
            var card = new CardDef { type = CardType.LoseCoins, amount = 100 };
            _state.Player.Multiplier = 3;
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(400, _state.Player.Coins); // flat 100 loss, not 300
        }

        [Test]
        public void ApplyCard_GainDice_AddsDice()
        {
            var card = new CardDef { type = CardType.GainDice, amount = 30 };
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(130, _state.Player.Dice);
        }

        [Test]
        public void ApplyCard_GainShield_AddsShield()
        {
            var card = new CardDef { type = CardType.GainShield, amount = 1 };
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(1, _state.Player.Shields);
        }

        [Test]
        public void ApplyCard_GoToJail_SetsJail()
        {
            var card = new CardDef { type = CardType.GoToJail };
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(8, _state.Player.Position); // jailTileIndex
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void ApplyCard_GoToTile_MovesPlayer()
        {
            var card = new CardDef { type = CardType.GoToTile, tileIndex = 10 };
            _cardSystem.ApplyCard(_state, card);
            Assert.AreEqual(10, _state.Player.Position);
        }
    }
}
```

- [ ] **Step 2: Write failing JailSystem tests**

```csharp
// Assets/Tests/EditMode/JailSystemTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class JailSystemTests
    {
        GameState _state;
        JailSystem _jail;

        [SetUp]
        public void SetUp()
        {
            var board = new BoardDef
            {
                tiles = new TileDef[32],
                jailTileIndex = 8,
                goTileIndex = 0,
                goBonus = 200,
                landmarks = new LandmarkDef[0]
            };
            _state = new GameState(board, startingDice: 100, diceCap: 1000);
            _jail = new JailSystem(jailDiceCost: 50);
        }

        [Test]
        public void SendToJail_SetsPositionAndTurns()
        {
            _jail.SendToJail(_state);
            Assert.AreEqual(8, _state.Player.Position);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void IsInJail_TrueWhenTurnsRemain()
        {
            _jail.SendToJail(_state);
            Assert.IsTrue(_jail.IsInJail(_state));
        }

        [Test]
        public void TickJailTurn_DecrementsTurns()
        {
            _jail.SendToJail(_state);
            _jail.TickJailTurn(_state);
            Assert.AreEqual(2, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void TickJailTurn_FreesAfterThreeTurns()
        {
            _jail.SendToJail(_state);
            _jail.TickJailTurn(_state);
            _jail.TickJailTurn(_state);
            _jail.TickJailTurn(_state);
            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
            Assert.IsFalse(_jail.IsInJail(_state));
        }

        [Test]
        public void PayToExit_ConsumeDiceAndFrees()
        {
            _jail.SendToJail(_state);
            bool success = _jail.PayToExit(_state);
            Assert.IsTrue(success);
            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
            Assert.AreEqual(50, _state.Player.Dice); // 100 - 50
        }

        [Test]
        public void PayToExit_FailsWhenNotEnoughDice()
        {
            var board = new BoardDef { tiles = new TileDef[32], jailTileIndex = 8, landmarks = new LandmarkDef[0] };
            var state = new GameState(board, startingDice: 30, diceCap: 1000);
            _jail.SendToJail(state);
            bool success = _jail.PayToExit(state);
            Assert.IsFalse(success);
            Assert.AreEqual(3, state.Player.JailTurnsLeft);
        }

        [Test]
        public void ExitOnDoubles_FreesPlayer()
        {
            _jail.SendToJail(_state);
            _jail.TryExitOnDoubles(_state, isDoubles: true);
            Assert.AreEqual(0, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void ExitOnDoubles_DoesNotFreeOnNonDoubles()
        {
            _jail.SendToJail(_state);
            _jail.TryExitOnDoubles(_state, isDoubles: false);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

- [ ] **Step 4: Implement CardSystem**

```csharp
// Assets/Scripts/MonopolyLite/Logic/CardSystem.cs
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    /// Design decision: Card-based LoseCoins does NOT scale with multiplier.
    /// Rationale: In Monopoly Go, multiplier only affects dice-triggered gains/losses
    /// (property rewards, tax, GO bonus). Card effects are flat amounts.
    /// Tax tiles DO scale because they are dice-triggered landing events.
    public class CardSystem
    {
        readonly int _seed;
        readonly MovementSystem _movementSystem;
        int[] _chanceOrder;
        int[] _communityChestOrder;

        public CardSystem(int seed, MovementSystem movementSystem = null)
        {
            _seed = seed;
            _movementSystem = movementSystem;
        }

        public CardDef DrawChance(GameState state)
        {
            var cards = state.BoardDef.chanceCards;
            if (_chanceOrder == null || _chanceOrder.Length != cards.Length)
                _chanceOrder = CreateShuffledIndices(cards.Length, _seed);

            int idx = state.Board.ChanceDrawIndex;
            var card = cards[_chanceOrder[idx]];
            state.Board.ChanceDrawIndex = (idx + 1) % cards.Length;
            return card;
        }

        public CardDef DrawCommunityChest(GameState state)
        {
            var cards = state.BoardDef.communityChestCards;
            if (_communityChestOrder == null || _communityChestOrder.Length != cards.Length)
                _communityChestOrder = CreateShuffledIndices(cards.Length, _seed + 1);

            int idx = state.Board.CommunityChestDrawIndex;
            var card = cards[_communityChestOrder[idx]];
            state.Board.CommunityChestDrawIndex = (idx + 1) % cards.Length;
            return card;
        }

        public void ApplyCard(GameState state, CardDef card)
        {
            switch (card.type)
            {
                case CardType.GainCoins:
                    state.Player.AddCoins(card.amount * state.Player.Multiplier);
                    break;
                case CardType.LoseCoins:
                    state.Player.SpendCoins(card.amount);
                    break;
                case CardType.GainDice:
                    state.Player.AddDice(card.amount);
                    break;
                case CardType.GainShield:
                    state.Player.AddShield(card.amount);
                    break;
                case CardType.GoToJail:
                    state.Player.Position = state.BoardDef.jailTileIndex;
                    state.Player.JailTurnsLeft = 3;
                    break;
                case CardType.GoToTile:
                    if (_movementSystem != null)
                        _movementSystem.MoveToTile(state, card.tileIndex, grantGoBonus: true);
                    else
                        state.Player.Position = card.tileIndex;
                    break;
            }
        }

        static int[] CreateShuffledIndices(int count, int seed)
        {
            var indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;
            // Fisher-Yates shuffle using existing RNG from Helpers.cs
            var rng = new RNG((uint)seed);
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Int(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }
            return indices;
        }
    }
}
```

- [ ] **Step 5: Implement JailSystem**

```csharp
// Assets/Scripts/MonopolyLite/Logic/JailSystem.cs
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class JailSystem
    {
        readonly int _diceCost;

        public JailSystem(int jailDiceCost)
        {
            _diceCost = jailDiceCost;
        }

        public void SendToJail(GameState state)
        {
            state.Player.Position = state.BoardDef.jailTileIndex;
            state.Player.JailTurnsLeft = 3;
        }

        public bool IsInJail(GameState state)
        {
            return state.Player.JailTurnsLeft > 0;
        }

        public void TickJailTurn(GameState state)
        {
            if (state.Player.JailTurnsLeft > 0)
                state.Player.JailTurnsLeft--;
        }

        public bool PayToExit(GameState state)
        {
            if (!state.Player.SpendDice(_diceCost)) return false;
            state.Player.JailTurnsLeft = 0;
            return true;
        }

        public bool TryExitOnDoubles(GameState state, bool isDoubles)
        {
            if (!isDoubles) return false;
            state.Player.JailTurnsLeft = 0;
            return true;
        }
    }
}
```

- [ ] **Step 6: Run all tests to verify they pass**

Expected: All CardSystem (8) + JailSystem (8) + previous tests PASS

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/CardSystem.cs Assets/Scripts/MonopolyLite/Logic/JailSystem.cs Assets/Tests/EditMode/CardSystemTests.cs Assets/Tests/EditMode/JailSystemTests.cs Assets/Scripts/MonopolyLite/State/PlayerState.cs
git commit -m "feat: add CardSystem and JailSystem with full test coverage"
```

---

## Task 8: TileResolver & LandmarkSystem Logic

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Logic/TileResolver.cs`
- Create: `Assets/Scripts/MonopolyLite/Logic/LandmarkSystem.cs`
- Test: `Assets/Tests/EditMode/TileResolverTests.cs`
- Test: `Assets/Tests/EditMode/LandmarkSystemTests.cs`

- [ ] **Step 1: Write failing TileResolver tests**

```csharp
// Assets/Tests/EditMode/TileResolverTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class TileResolverTests
    {
        BoardDef _board;
        GameState _state;
        CardSystem _cardSystem;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardDef
            {
                tiles = new[]
                {
                    new TileDef { name = "GO",           type = TileType.Go },
                    new TileDef { name = "Brown1",       type = TileType.Property,       colorGroup = ColorGroup.Brown, baseReward = 50 },
                    new TileDef { name = "Tax",          type = TileType.Tax,             taxAmount = 150 },
                    new TileDef { name = "Chance",       type = TileType.Chance },
                    new TileDef { name = "CC",           type = TileType.CommunityChest },
                    new TileDef { name = "Railroad",     type = TileType.Railroad,        baseReward = 100 },
                    new TileDef { name = "Jail",         type = TileType.Jail },
                    new TileDef { name = "FreeParking",  type = TileType.FreeParking },
                    new TileDef { name = "GoToJail",     type = TileType.GoToJail },
                },
                goTileIndex = 0,
                goBonus = 200,
                jailTileIndex = 6,
                landmarks = new LandmarkDef[0],
                chanceCards = new[] { new CardDef { type = CardType.GainCoins, description = "Win!", amount = 100 } },
                communityChestCards = new[] { new CardDef { type = CardType.GainCoins, description = "CC Win!", amount = 80 } },
            };
            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
            _cardSystem = new CardSystem(seed: 42);
        }

        [Test]
        public void Resolve_Property_GrantsCoinsTimesMultiplier()
        {
            _state.Player.Position = 1;
            _state.Player.Multiplier = 2;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.CoinsGained, result.Type);
            Assert.AreEqual(100, _state.Player.Coins); // 50 × 2
        }

        [Test]
        public void Resolve_Tax_LosesCoins()
        {
            _state.Player.AddCoins(500);
            _state.Player.Position = 2;
            _state.Player.Multiplier = 2;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.CoinsLost, result.Type);
            Assert.AreEqual(200, _state.Player.Coins); // 500 - (150 × 2)
        }

        [Test]
        public void Resolve_Railroad_GrantsBonusCoins()
        {
            _state.Player.Position = 5;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.Railroad, result.Type);
            Assert.AreEqual(100, _state.Player.Coins); // placeholder bonus
        }

        [Test]
        public void Resolve_GoToJail_SendsToJail()
        {
            _state.Player.Position = 8;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            resolver.Resolve(_state);
            Assert.AreEqual(6, _state.Player.Position);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        [Test]
        public void Resolve_FreeParking_NoEffect()
        {
            _state.Player.Position = 7;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.Nothing, result.Type);
            Assert.AreEqual(0, _state.Player.Coins);
        }

        [Test]
        public void Resolve_Chance_DrawsCard()
        {
            _state.Player.Position = 3;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.Card, result.Type);
            Assert.AreEqual(100, _state.Player.Coins); // GainCoins 100 × multiplier 1
        }

        [Test]
        public void Resolve_CommunityChest_DrawsCard()
        {
            _state.Player.Position = 4;
            var resolver = new TileResolver(_cardSystem, new JailSystem(50));
            var result = resolver.Resolve(_state);
            Assert.AreEqual(TileResolveType.Card, result.Type);
            Assert.AreEqual(80, _state.Player.Coins);
        }
    }
}
```

- [ ] **Step 2: Write failing LandmarkSystem tests**

```csharp
// Assets/Tests/EditMode/LandmarkSystemTests.cs
using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class LandmarkSystemTests
    {
        GameState _state;
        LandmarkSystem _landmarks;

        [SetUp]
        public void SetUp()
        {
            var board = new BoardDef
            {
                tiles = new TileDef[32],
                landmarks = new[]
                {
                    new LandmarkDef
                    {
                        colorGroup = ColorGroup.Brown,
                        name = "Hagia Sophia",
                        costs = new[] { 500, 1200, 3000, 7000, 15000 },
                        nwPoints = new[] { 100, 300, 600, 1200, 2500 }
                    },
                    new LandmarkDef
                    {
                        colorGroup = ColorGroup.Blue,
                        name = "Bosphorus Bridge",
                        costs = new[] { 1500, 3000, 7000, 16000, 38000 },
                        nwPoints = new[] { 250, 650, 1300, 2600, 5200 }
                    },
                },
                goTileIndex = 0,
                goBonus = 200
            };
            _state = new GameState(board, startingDice: 100, diceCap: 1000);
            _landmarks = new LandmarkSystem();
        }

        [Test]
        public void CanUpgrade_TrueWhenEnoughCoins()
        {
            _state.Player.AddCoins(500);
            Assert.IsTrue(_landmarks.CanUpgrade(_state, ColorGroup.Brown));
        }

        [Test]
        public void CanUpgrade_FalseWhenNotEnoughCoins()
        {
            _state.Player.AddCoins(100);
            Assert.IsFalse(_landmarks.CanUpgrade(_state, ColorGroup.Brown));
        }

        [Test]
        public void CanUpgrade_FalseWhenMaxLevel()
        {
            _state.Player.AddCoins(999999);
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            Assert.IsFalse(_landmarks.CanUpgrade(_state, ColorGroup.Brown));
        }

        [Test]
        public void Upgrade_DeductsCoinsAndIncreasesLevel()
        {
            _state.Player.AddCoins(1000);
            bool success = _landmarks.Upgrade(_state, ColorGroup.Brown);
            Assert.IsTrue(success);
            Assert.AreEqual(500, _state.Player.Coins); // 1000 - 500
            Assert.AreEqual(1, _state.Board.GetLandmarkLevel(ColorGroup.Brown));
        }

        [Test]
        public void Upgrade_GrantsNetWorth()
        {
            _state.Player.AddCoins(500);
            _landmarks.Upgrade(_state, ColorGroup.Brown);
            Assert.AreEqual(100, _state.Player.NetWorth);
        }

        [Test]
        public void Upgrade_SecondLevel_CostsMore()
        {
            _state.Player.AddCoins(2000);
            _landmarks.Upgrade(_state, ColorGroup.Brown); // L1: 500
            _landmarks.Upgrade(_state, ColorGroup.Brown); // L2: 1200
            Assert.AreEqual(300, _state.Player.Coins); // 2000 - 500 - 1200
            Assert.AreEqual(2, _state.Board.GetLandmarkLevel(ColorGroup.Brown));
            Assert.AreEqual(400, _state.Player.NetWorth); // 100 + 300
        }

        [Test]
        public void IsBoardComplete_FalseWhenNotAllMax()
        {
            Assert.IsFalse(_landmarks.IsBoardComplete(_state));
        }

        [Test]
        public void IsBoardComplete_TrueWhenAllMax()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            _state.Board.SetLandmarkLevel(ColorGroup.Blue, 5);
            Assert.IsTrue(_landmarks.IsBoardComplete(_state));
        }

        [Test]
        public void GetUpgradeCost_ReturnsCorrectCostPerLevel()
        {
            Assert.AreEqual(500, _landmarks.GetUpgradeCost(_state, ColorGroup.Brown)); // L0 → L1
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 1);
            Assert.AreEqual(1200, _landmarks.GetUpgradeCost(_state, ColorGroup.Brown)); // L1 → L2
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 4);
            Assert.AreEqual(15000, _landmarks.GetUpgradeCost(_state, ColorGroup.Brown)); // L4 → L5
        }

        [Test]
        public void GetUpgradeCost_ReturnsNegativeOneWhenMaxLevel()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            Assert.AreEqual(-1, _landmarks.GetUpgradeCost(_state, ColorGroup.Brown));
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

- [ ] **Step 4: Implement TileResolver**

```csharp
// Assets/Scripts/MonopolyLite/Logic/TileResolver.cs
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public enum TileResolveType
    {
        Nothing,
        CoinsGained,
        CoinsLost,
        Card,
        Jail,
        Railroad
    }

    public struct TileResolveResult
    {
        public TileResolveType Type;
        public int Amount;
        public CardDef? DrawnCard;
    }

    public class TileResolver
    {
        readonly CardSystem _cardSystem;
        readonly JailSystem _jailSystem;

        public TileResolver(CardSystem cardSystem, JailSystem jailSystem)
        {
            _cardSystem = cardSystem;
            _jailSystem = jailSystem;
        }

        public TileResolveResult Resolve(GameState state)
        {
            var tile = state.BoardDef.tiles[state.Player.Position];

            switch (tile.type)
            {
                case TileType.Property:
                {
                    int reward = tile.baseReward * state.Player.Multiplier;
                    state.Player.AddCoins(reward);
                    return new TileResolveResult { Type = TileResolveType.CoinsGained, Amount = reward };
                }
                case TileType.Tax:
                {
                    int loss = tile.taxAmount * state.Player.Multiplier;
                    state.Player.SpendCoins(loss);
                    return new TileResolveResult { Type = TileResolveType.CoinsLost, Amount = loss };
                }
                case TileType.Railroad:
                {
                    // Phase 3: Bank Heist / Shutdown. Placeholder: bonus coins.
                    int reward = tile.baseReward * state.Player.Multiplier;
                    state.Player.AddCoins(reward);
                    return new TileResolveResult { Type = TileResolveType.Railroad, Amount = reward };
                }
                case TileType.Chance:
                {
                    var card = _cardSystem.DrawChance(state);
                    _cardSystem.ApplyCard(state, card);
                    return new TileResolveResult { Type = TileResolveType.Card, DrawnCard = card };
                }
                case TileType.CommunityChest:
                {
                    var card = _cardSystem.DrawCommunityChest(state);
                    _cardSystem.ApplyCard(state, card);
                    return new TileResolveResult { Type = TileResolveType.Card, DrawnCard = card };
                }
                case TileType.GoToJail:
                {
                    _jailSystem.SendToJail(state);
                    return new TileResolveResult { Type = TileResolveType.Jail };
                }
                case TileType.Go:
                case TileType.Jail:
                case TileType.FreeParking:
                default:
                    return new TileResolveResult { Type = TileResolveType.Nothing };
            }
        }
    }
}
```

- [ ] **Step 5: Implement LandmarkSystem**

```csharp
// Assets/Scripts/MonopolyLite/Logic/LandmarkSystem.cs
using System.Linq;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class LandmarkSystem
    {
        public int GetUpgradeCost(GameState state, ColorGroup group)
        {
            int currentLevel = state.Board.GetLandmarkLevel(group);
            if (currentLevel >= 5) return -1;

            var landmark = FindLandmark(state.BoardDef, group);
            if (landmark == null) return -1;

            return landmark.Value.costs[currentLevel];
        }

        public bool CanUpgrade(GameState state, ColorGroup group)
        {
            int cost = GetUpgradeCost(state, group);
            if (cost < 0) return false;
            return state.Player.Coins >= cost;
        }

        public bool Upgrade(GameState state, ColorGroup group)
        {
            if (!CanUpgrade(state, group)) return false;

            int currentLevel = state.Board.GetLandmarkLevel(group);
            var landmark = FindLandmark(state.BoardDef, group).Value;
            int cost = landmark.costs[currentLevel];
            int nw = landmark.nwPoints[currentLevel];

            state.Player.SpendCoins(cost);
            state.Board.SetLandmarkLevel(group, currentLevel + 1);
            state.Player.NetWorth += nw;

            return true;
        }

        public bool IsBoardComplete(GameState state)
        {
            return state.Board.IsComplete();
        }

        static LandmarkDef? FindLandmark(BoardDef board, ColorGroup group)
        {
            foreach (var lm in board.landmarks)
            {
                if (lm.colorGroup == group) return lm;
            }
            return null;
        }
    }
}
```

- [ ] **Step 6: Run all tests to verify they pass**

Expected: All TileResolver (7) + LandmarkSystem (10) + previous tests PASS

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Logic/TileResolver.cs Assets/Scripts/MonopolyLite/Logic/LandmarkSystem.cs Assets/Tests/EditMode/TileResolverTests.cs Assets/Tests/EditMode/LandmarkSystemTests.cs
git commit -m "feat: add TileResolver and LandmarkSystem with full test coverage"
```

---

## Task 9: GameController & Bootstrap (Unity Integration)

**Files:**
- Create: `Assets/Scripts/MonopolyLite/Core/GameController.cs`
- Modify: `Assets/Scripts/MonopolyLite/Core/Bootstrap.cs`

This task connects all pure logic systems to Unity. No test file — this is MonoBehaviour integration, verified by running the game.

- [ ] **Step 1: Create GameController**

```csharp
// Assets/Scripts/MonopolyLite/Core/GameController.cs
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

        // Events for View layer to subscribe
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
                // In jail: roll for doubles
                var jailRoll = _diceSystem.Roll(State.Player);
                if (!jailRoll.Success) return;

                if (_jailSystem.TryExitOnDoubles(State, jailRoll.IsDoubles))
                {
                    // Freed by doubles — move normally
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

        public void DoPayJailExit()
        {
            _jailSystem.PayToExit(State);
        }

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

        public void SetMultiplier(int value)
        {
            State.Player.Multiplier = value;
        }

        public bool CanUpgradeLandmark(ColorGroup group)
        {
            return _landmarkSystem.CanUpgrade(State, group);
        }

        public int GetUpgradeCost(ColorGroup group)
        {
            return _landmarkSystem.GetUpgradeCost(State, group);
        }
    }
}
```

- [ ] **Step 2: Rewrite Bootstrap**

```csharp
// Assets/Scripts/MonopolyLite/Core/Bootstrap.cs
using UnityEngine;

namespace MonopolyLite.Core
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            var go = new GameObject("GameController");
            var controller = go.AddComponent<GameController>();
            controller.Initialize();

            // View layer will be wired here in Task 10-12
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs Assets/Scripts/MonopolyLite/Core/Bootstrap.cs
git commit -m "feat: add GameController orchestrating all logic systems, rewrite Bootstrap"
```

---

## Task 10: Board Rendering (View Layer)

**Files:**
- Create: `Assets/Scripts/MonopolyLite/View/BoardRenderer.cs`
- Create: `Assets/Scripts/MonopolyLite/View/TokenRenderer.cs`

Uses existing `Helpers.Layout` for perimeter position calculation and `Helpers.Sprites` for procedural sprites.

- [ ] **Step 1: Create BoardRenderer**

```csharp
// Assets/Scripts/MonopolyLite/View/BoardRenderer.cs
using MonopolyLite;     // for Layout, Sprites (from Helpers.cs)
using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.View
{
    public class BoardRenderer : MonoBehaviour
    {
        GameObject[] _tileObjects;
        const int TilePixelSize = 64;  // pixel size for procedural sprites
        const float TilePad = 0.1f;    // padding between tiles

        public void Render(BoardDef board)
        {
            _tileObjects = new GameObject[board.tiles.Length];
            var positions = Layout.Perimeter(board.tiles.Length, board.sideLength, board.tileSize, TilePad);

            for (int i = 0; i < board.tiles.Length; i++)
            {
                var tile = board.tiles[i];
                var go = new GameObject($"Tile_{i}_{tile.name}");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(positions[i].x, positions[i].y, 0);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprites.Square(TilePixelSize, GetTileColor(tile));
                sr.transform.localScale = Vector3.one * (board.tileSize / (TilePixelSize / 100f)); // scale to world size
                sr.sortingOrder = 0;

                _tileObjects[i] = go;
            }
        }

        public Vector3 GetTilePosition(int index)
        {
            if (_tileObjects == null || index < 0 || index >= _tileObjects.Length)
                return Vector3.zero;
            return _tileObjects[index].transform.position;
        }

        static Color GetTileColor(TileDef tile)
        {
            return tile.colorGroup switch
            {
                ColorGroup.Brown     => new Color(0.55f, 0.27f, 0.07f),
                ColorGroup.LightBlue => new Color(0.68f, 0.85f, 0.90f),
                ColorGroup.Pink      => new Color(0.85f, 0.44f, 0.84f),
                ColorGroup.Orange    => new Color(1.0f, 0.65f, 0.0f),
                ColorGroup.Red       => new Color(0.9f, 0.1f, 0.1f),
                ColorGroup.Yellow    => new Color(1.0f, 0.95f, 0.0f),
                ColorGroup.Green     => new Color(0.0f, 0.7f, 0.0f),
                ColorGroup.Blue      => new Color(0.0f, 0.0f, 0.8f),
                _ => tile.type switch
                {
                    TileType.Railroad       => new Color(0.3f, 0.3f, 0.3f),
                    TileType.Chance         => new Color(1.0f, 0.8f, 0.2f),
                    TileType.CommunityChest => new Color(0.2f, 0.6f, 1.0f),
                    TileType.Tax            => new Color(0.6f, 0.0f, 0.0f),
                    _                       => new Color(0.9f, 0.9f, 0.85f),
                }
            };
        }
    }
}
```

- [ ] **Step 2: Create TokenRenderer**

```csharp
// Assets/Scripts/MonopolyLite/View/TokenRenderer.cs
using MonopolyLite;  // for Sprites (from Helpers.cs)
using UnityEngine;

namespace MonopolyLite.View
{
    public class TokenRenderer : MonoBehaviour
    {
        SpriteRenderer _sr;
        const int TokenPixelSize = 32;

        public void Initialize(Color color, float worldSize)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = Sprites.Circle(TokenPixelSize, color);
            _sr.transform.localScale = Vector3.one * (worldSize * 0.4f / (TokenPixelSize / 100f));
            _sr.sortingOrder = 10;
        }

        public void MoveTo(Vector3 position)
        {
            transform.position = position + Vector3.back * 0.1f; // slightly in front
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/BoardRenderer.cs Assets/Scripts/MonopolyLite/View/TokenRenderer.cs
git commit -m "feat: add BoardRenderer and TokenRenderer using procedural sprites"
```

---

## Task 11: HUD & UI (View Layer)

**Files:**
- Create: `Assets/Scripts/MonopolyLite/View/UIManager.cs`
- Create: `Assets/Scripts/MonopolyLite/View/HUDView.cs`
- Create: `Assets/Scripts/MonopolyLite/View/LandmarkPanelView.cs`

- [ ] **Step 1: Create HUDView**

```csharp
// Assets/Scripts/MonopolyLite/View/HUDView.cs
using MonopolyLite.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class HUDView : MonoBehaviour
    {
        Button _rollButton;
        Button _multiplierButton;
        TextMeshProUGUI _diceText;
        TextMeshProUGUI _coinText;
        TextMeshProUGUI _shieldText;
        TextMeshProUGUI _nwText;
        TextMeshProUGUI _multiplierText;
        TextMeshProUGUI _statusText;

        GameController _controller;
        int[] _multiplierValues = { 1, 2, 5, 10 };
        int _multiplierIndex;

        public void Initialize(GameController controller, Canvas canvas)
        {
            _controller = controller;
            BuildUI(canvas);
            _controller.OnRollComplete += (roll, move) => UpdateDisplay();
            _controller.OnTileResolved += (result) => UpdateStatus(result.ToString());
            _controller.OnLandmarkUpgraded += (group, level) => UpdateDisplay();
            UpdateDisplay();
        }

        void BuildUI(Canvas canvas)
        {
            var rt = canvas.GetComponent<RectTransform>();

            // Roll button
            _rollButton = CreateButton(canvas.transform, "ROLL", new Vector2(0, -400), new Vector2(200, 100));
            _rollButton.onClick.AddListener(OnRollClicked);

            // Multiplier button
            _multiplierButton = CreateButton(canvas.transform, "x1", new Vector2(130, -350), new Vector2(80, 60));
            _multiplierButton.onClick.AddListener(OnMultiplierClicked);

            // Info texts
            _diceText = CreateText(canvas.transform, "", new Vector2(-350, 420), TextAlignmentOptions.Left);
            _coinText = CreateText(canvas.transform, "", new Vector2(-350, 380), TextAlignmentOptions.Left);
            _shieldText = CreateText(canvas.transform, "", new Vector2(-350, 340), TextAlignmentOptions.Left);
            _nwText = CreateText(canvas.transform, "", new Vector2(-350, 300), TextAlignmentOptions.Left);
            _statusText = CreateText(canvas.transform, "", new Vector2(0, -300), TextAlignmentOptions.Center);
        }

        void OnRollClicked()
        {
            _controller.DoRoll();
        }

        void OnMultiplierClicked()
        {
            _multiplierIndex = (_multiplierIndex + 1) % _multiplierValues.Length;
            int value = _multiplierValues[_multiplierIndex];
            _controller.SetMultiplier(value);
            _multiplierText = _multiplierButton.GetComponentInChildren<TextMeshProUGUI>();
            if (_multiplierText != null) _multiplierText.text = $"x{value}";
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_controller.State == null) return;
            var p = _controller.State.Player;
            _diceText.text = $"Dice: {p.Dice}";
            _coinText.text = $"Coins: {p.Coins}";
            _shieldText.text = $"Shields: {p.Shields}/3";
            _nwText.text = $"Net Worth: {p.NetWorth}";

            var rollText = _rollButton.GetComponentInChildren<TextMeshProUGUI>();
            if (rollText != null)
            {
                rollText.text = p.JailTurnsLeft > 0
                    ? $"ROLL\n(Jail: {p.JailTurnsLeft})"
                    : "ROLL";
            }
        }

        void UpdateStatus(string text)
        {
            if (_statusText != null) _statusText.text = text;
        }

        static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go.GetComponent<Button>();
        }

        static TextMeshProUGUI CreateText(Transform parent, string text, Vector2 pos, TextAlignmentOptions align)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 40);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }
    }
}
```

- [ ] **Step 2: Create LandmarkPanelView**

```csharp
// Assets/Scripts/MonopolyLite/View/LandmarkPanelView.cs
using MonopolyLite.Core;
using MonopolyLite.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class LandmarkPanelView : MonoBehaviour
    {
        GameController _controller;
        Transform _panelRoot;

        public void Initialize(GameController controller, Canvas canvas)
        {
            _controller = controller;
            _controller.OnRollComplete += (_, __) => Refresh();
            _controller.OnLandmarkUpgraded += (_, __) => Refresh();

            BuildPanel(canvas);
            Refresh();
        }

        void BuildPanel(Canvas canvas)
        {
            var go = new GameObject("LandmarkPanel", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(350, 0);
            rt.sizeDelta = new Vector2(250, 600);
            _panelRoot = go.transform;
        }

        public void Refresh()
        {
            if (_controller.State == null || _panelRoot == null) return;

            // Clear existing buttons
            foreach (Transform child in _panelRoot)
                Destroy(child.gameObject);

            var landmarks = _controller.BoardDef.landmarks;
            float yOffset = 250f;

            for (int i = 0; i < landmarks.Length; i++)
            {
                var lm = landmarks[i];
                int level = _controller.State.Board.GetLandmarkLevel(lm.colorGroup);
                int cost = _controller.GetUpgradeCost(lm.colorGroup);
                bool canUpgrade = _controller.CanUpgradeLandmark(lm.colorGroup);

                var entryGo = new GameObject($"LM_{lm.name}", typeof(RectTransform));
                entryGo.transform.SetParent(_panelRoot, false);
                var entryRt = entryGo.GetComponent<RectTransform>();
                entryRt.anchoredPosition = new Vector2(0, yOffset - i * 70);
                entryRt.sizeDelta = new Vector2(240, 60);

                // Label
                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(entryGo.transform, false);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchoredPosition = new Vector2(-40, 0);
                labelRt.sizeDelta = new Vector2(160, 60);
                var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
                labelTmp.text = $"{lm.name}\nL{level}/5";
                labelTmp.fontSize = 16;
                labelTmp.color = level >= 5 ? Color.green : Color.white;

                if (level < 5)
                {
                    // Build button
                    var btnGo = new GameObject("BuildBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    btnGo.transform.SetParent(entryGo.transform, false);
                    var btnRt = btnGo.GetComponent<RectTransform>();
                    btnRt.anchoredPosition = new Vector2(90, 0);
                    btnRt.sizeDelta = new Vector2(70, 40);
                    btnGo.GetComponent<Image>().color = canUpgrade ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.4f, 0.4f, 0.4f);

                    var btnTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    btnTextGo.transform.SetParent(btnGo.transform, false);
                    var btnTextRt = btnTextGo.GetComponent<RectTransform>();
                    btnTextRt.anchorMin = Vector2.zero;
                    btnTextRt.anchorMax = Vector2.one;
                    btnTextRt.offsetMin = Vector2.zero;
                    btnTextRt.offsetMax = Vector2.zero;
                    var btnTmp = btnTextGo.GetComponent<TextMeshProUGUI>();
                    btnTmp.text = cost >= 0 ? $"${cost}" : "MAX";
                    btnTmp.fontSize = 14;
                    btnTmp.alignment = TextAlignmentOptions.Center;
                    btnTmp.color = Color.white;

                    var colorGroup = lm.colorGroup; // capture for closure
                    btnGo.GetComponent<Button>().onClick.AddListener(() => _controller.DoUpgradeLandmark(colorGroup));
                    btnGo.GetComponent<Button>().interactable = canUpgrade;
                }
            }
        }
    }
}
```

- [ ] **Step 3: Create UIManager to wire everything**

```csharp
// Assets/Scripts/MonopolyLite/View/UIManager.cs
using MonopolyLite.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class UIManager : MonoBehaviour
    {
        Canvas _canvas;
        HUDView _hud;
        LandmarkPanelView _landmarkPanel;

        public void Initialize(GameController controller)
        {
            // Create Canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            // Create EventSystem if not present
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            // HUD
            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(canvasGo.transform, false);
            _hud = hudGo.AddComponent<HUDView>();
            _hud.Initialize(controller, _canvas);

            // Landmark panel
            var lmGo = new GameObject("LandmarkPanel");
            lmGo.transform.SetParent(canvasGo.transform, false);
            _landmarkPanel = lmGo.AddComponent<LandmarkPanelView>();
            _landmarkPanel.Initialize(controller, _canvas);
        }
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/View/
git commit -m "feat: add HUD, LandmarkPanel, and UIManager for core game UI"
```

---

## Task 12: Wire Everything in Bootstrap & Verify

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Bootstrap.cs`
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Update Bootstrap to wire all systems**

```csharp
// Assets/Scripts/MonopolyLite/Core/Bootstrap.cs
using MonopolyLite.View;
using UnityEngine;

namespace MonopolyLite.Core
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Camera setup
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            // Game Controller
            var controllerGo = new GameObject("GameController");
            var controller = controllerGo.AddComponent<GameController>();
            controller.Initialize();

            // Board rendering
            var boardGo = new GameObject("Board");
            var boardRenderer = boardGo.AddComponent<BoardRenderer>();
            boardRenderer.Render(controller.BoardDef);

            // Token rendering
            var tokenGo = new GameObject("Token");
            var tokenRenderer = tokenGo.AddComponent<TokenRenderer>();
            tokenRenderer.Initialize(Color.white, controller.BoardDef.tileSize);
            tokenRenderer.MoveTo(boardRenderer.GetTilePosition(0));

            // Wire token movement to roll events
            controller.OnRollComplete += (roll, move) =>
            {
                if (roll.Success)
                    tokenRenderer.MoveTo(boardRenderer.GetTilePosition(controller.State.Player.Position));
            };

            // UI
            var uiGo = new GameObject("UI");
            var uiManager = uiGo.AddComponent<UIManager>();
            uiManager.Initialize(controller);
        }
    }
}
```

- [ ] **Step 2: Run the game in Unity Editor**

Verification checklist:
- Board renders with colored tiles in a square layout
- Token appears at GO position
- Tap ROLL → dice decreases, token moves, coins change based on tile
- Multiplier button cycles through 1x/2x/5x/10x
- Landmark panel shows 8 landmarks with costs
- Clicking Build on a landmark deducts coins, increases level
- Tax tiles reduce coins
- Go To Jail sends token to Jail
- Card tiles show effects (coin gain/loss, dice, shields)
- HUD updates after every action

- [ ] **Step 3: Run all unit tests**

Run: Unity Test Runner → EditMode → Run All
Expected: All tests PASS (no regressions)

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Bootstrap.cs
git commit -m "feat: wire Bootstrap with board rendering, token, and UI — playable MVP"
```

---

## Summary

| Task | What | Tests |
|---|---|---|
| 1 | Project cleanup & test setup | — |
| 2 | Data model (enums, structs) | — |
| 3 | Board config & JSON loader | — |
| 4 | Game state (Player, Board, Game) | 9 tests |
| 5 | DiceSystem | 5 tests |
| 6 | MovementSystem | 6 tests |
| 7 | CardSystem + JailSystem | 16 tests |
| 8 | TileResolver + LandmarkSystem | 17 tests |
| 9 | GameController + Bootstrap | — |
| 10 | Board & Token rendering | — |
| 11 | HUD & Landmark UI | — |
| 12 | Wire everything & verify | Manual |

**Total: 12 tasks, ~53 unit tests, playable MVP at the end**
