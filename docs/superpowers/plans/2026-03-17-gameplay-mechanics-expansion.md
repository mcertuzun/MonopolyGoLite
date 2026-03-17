# Gameplay Mechanics Expansion — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand MonopolyGoLite with color groups, houses/hotels, Chance/Community Chest card decks, railroads/utilities, decline-to-discount, and win/lose conditions.

**Architecture:** All changes fit within the existing partial-class structure. `GameConfig.cs` holds all data types and config loading. `Main.cs` holds state. `Main.Logic.cs` holds gameplay rules. `Main.Input.cs` holds UI and input handling. No new files. JSON config drives board layout, card decks, and rent tables.

**Tech Stack:** Unity (C#), TextMesh Pro, JsonUtility, PCG32 RNG

**Testing note:** This is a Unity project with no automated test framework. Each task includes manual verification steps to run in the Unity Editor (Play Mode). Verify by observing console logs, UI state, and gameplay behavior.

**Spec:** `docs/superpowers/specs/2026-03-16-monopoly-gameplay-mechanics-design.md`

---

## Chunk 1: Data Model & Config Foundation

### Task 1: Expand TileType enum and TileDef struct

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs:6-24`

- [ ] **Step 1: Update TileType enum**

Replace the existing `TileType` enum (lines 6-14) with:

```csharp
public enum TileType
{
    Go,
    Property,
    Tax,
    Chance,
    CommunityChest,
    GoToJail,
    Jail,
    Railroad,
    Utility
}
```

Note: The integer values change from the old enum. `Chest` (was 3) is replaced by `Chance` (3) and `CommunityChest` (4). `GoToJail` moves from 4→5, `Jail` from 5→6. New: `Railroad` (7), `Utility` (8). The JSON config will be fully rewritten in Task 3, so integer mapping changes are fine.

**IMPORTANT:** Also update `Main.Logic.cs` to fix compilation. Replace `case TileType.Chest:` (line 80) with a temporary placeholder that handles both Chance and CommunityChest:

```csharp
case TileType.Chance:
case TileType.CommunityChest:
    int delta = rng.Next(-50, 101);
    if (delta > 0) delta *= gainMultiplier;
    cash[p] += delta;
    break;
```

And in `ColorForTile` (line 165), replace `case TileType.Chest: return config.chestColor;` with:

```csharp
case TileType.Chance: return config.chestColor;
case TileType.CommunityChest: return config.chestColor;
```

These temporary stubs will be replaced with proper logic in Tasks 4 and 9.

- [ ] **Step 2: Add ColorGroup enum**

Add after the `TileType` enum:

```csharp
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
```

- [ ] **Step 3: Add CardType enum and CardDef struct**

Add after `ColorGroup`:

```csharp
public enum CardType
{
    GainMoney,
    LoseMoney,
    GoToTile,
    GoToJail,
    RepairCosts,
    GainPerProperty
}

[Serializable]
public struct CardDef
{
    public CardType type;
    public string description;
    public int amount;
    public int tileIndex;
    public int perHouse;
    public int perHotel;
}
```

- [ ] **Step 4: Expand TileDef struct**

Replace the existing `TileDef` struct (lines 17-24) with:

```csharp
[Serializable]
public struct TileDef
{
    public string name;
    public TileType type;
    public int price;
    public int baseRent;
    public int taxAmount;
    public ColorGroup colorGroup;
    public int[] rentTable;
    public int houseCost;
    public int hotelCost;
}
```

`baseRent` is kept for backward compatibility and used as `rentTable[0]` fallback. For Railroad/Utility tiles, `baseRent`, `colorGroup`, `rentTable`, `houseCost`, `hotelCost` are unused (default values).

- [ ] **Step 5: Verify compilation**

Open Unity Editor. Check Console for compilation errors. The game will NOT run correctly yet (JSON config uses old enum integers), but it must compile.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Shared/GameConfig.cs
git commit -m "feat: expand TileType enum, add ColorGroup, CardType, CardDef, expand TileDef"
```

---

### Task 2: Expand GameConfig and GameConfigJson with new fields

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs:27-67`

- [ ] **Step 1: Add new fields to GameConfig ScriptableObject**

Add the following fields to the `GameConfig` class after `chargeInterval` (line 42):

```csharp
public int[] railroadRentTiers = { 25, 50, 100, 200 };
public int[] utilityRentFactors = { 4, 10 };
public CardDef[] chanceCards;
public CardDef[] communityChestCards;
```

Add new tile colors after `gotoJailColor` (line 48):

```csharp
public Color railroadColor = new(0.50f, 0.50f, 0.50f);
public Color utilityColor = new(0.85f, 0.75f, 0.35f);
public Color chanceColor = new(0.95f, 0.55f, 0.20f);
public Color communityChestColor = new(0.30f, 0.70f, 0.90f);
```

Add color group colors:

```csharp
public Color brownGroup = new(0.55f, 0.33f, 0.16f);
public Color lightBlueGroup = new(0.60f, 0.82f, 0.95f);
public Color pinkGroup = new(0.85f, 0.30f, 0.65f);
public Color orangeGroup = new(0.95f, 0.60f, 0.15f);
public Color redGroup = new(0.90f, 0.20f, 0.20f);
public Color yellowGroup = new(0.95f, 0.90f, 0.25f);
public Color greenGroup = new(0.15f, 0.70f, 0.30f);
public Color blueGroup = new(0.10f, 0.20f, 0.70f);
```

- [ ] **Step 2: Add new fields to GameConfigJson**

Add to the `GameConfigJson` class after `chargeInterval` (line 66):

```csharp
public int[] railroadRentTiers;
public int[] utilityRentFactors;
public CardDef[] chanceCards;
public CardDef[] communityChestCards;
```

- [ ] **Step 3: Update ConfigLoader.LoadOrDefault to copy new fields**

In the `LoadOrDefault` method, after `so.chargeInterval = j.chargeInterval;` (line 94), add:

```csharp
if (j.railroadRentTiers != null) so.railroadRentTiers = j.railroadRentTiers;
if (j.utilityRentFactors != null) so.utilityRentFactors = j.utilityRentFactors;
so.chanceCards = j.chanceCards;
so.communityChestCards = j.communityChestCards;
```

- [ ] **Step 4: Update BuildDefault to use new TileType values**

In the `BuildDefault` method, update the tile array to use the new enum values. The key change is `TileType.Chest` no longer exists — replace with `TileType.CommunityChest` and `TileType.Chance` respectively:

```csharp
private static GameConfig BuildDefault()
{
    GameConfig so = ScriptableObject.CreateInstance<GameConfig>();
    so.tiles = new TileDef[]
    {
        new() { name = "GO", type = TileType.Go },
        new() { name = "Mediterranean Ave", type = TileType.Property, price = 60, baseRent = 2,
                colorGroup = ColorGroup.Brown, rentTable = new[] { 2, 10, 30, 90, 160, 250 }, houseCost = 50, hotelCost = 50 },
        new() { name = "Community Chest", type = TileType.CommunityChest },
        new() { name = "Baltic Ave", type = TileType.Property, price = 60, baseRent = 4,
                colorGroup = ColorGroup.Brown, rentTable = new[] { 4, 20, 60, 180, 320, 450 }, houseCost = 50, hotelCost = 50 },
        new() { name = "Income Tax", type = TileType.Tax, taxAmount = 200 },
        new() { name = "Reading Railroad", type = TileType.Railroad, price = 200 },
        new() { name = "Jail", type = TileType.Jail },
        new() { name = "Oriental Ave", type = TileType.Property, price = 100, baseRent = 6,
                colorGroup = ColorGroup.LightBlue, rentTable = new[] { 6, 30, 90, 270, 400, 550 }, houseCost = 50, hotelCost = 50 },
        new() { name = "Chance", type = TileType.Chance },
        new() { name = "Vermont Ave", type = TileType.Property, price = 100, baseRent = 6,
                colorGroup = ColorGroup.LightBlue, rentTable = new[] { 6, 30, 90, 270, 400, 550 }, houseCost = 50, hotelCost = 50 },
        new() { name = "Connecticut Ave", type = TileType.Property, price = 120, baseRent = 8,
                colorGroup = ColorGroup.LightBlue, rentTable = new[] { 8, 40, 100, 300, 450, 600 }, houseCost = 50, hotelCost = 50 },
        new() { name = "Go To Jail", type = TileType.GoToJail }
    };
    so.chanceCards = DefaultChanceCards();
    so.communityChestCards = DefaultCommunityChestCards();
    return so;
}
```

- [ ] **Step 5: Add default card deck methods**

Add to the `ConfigLoader` class:

```csharp
private static CardDef[] DefaultChanceCards()
{
    return new CardDef[]
    {
        new() { type = CardType.GainMoney, description = "Bank pays you dividend of $50", amount = 50 },
        new() { type = CardType.GoToTile, description = "Advance to GO", tileIndex = 0 },
        new() { type = CardType.GoToJail, description = "Go directly to Jail" },
        new() { type = CardType.LoseMoney, description = "Speeding fine $15", amount = 15 },
        new() { type = CardType.GainMoney, description = "Building loan matures — collect $150", amount = 150 },
        new() { type = CardType.LoseMoney, description = "Pay poor tax of $15", amount = 15 },
        new() { type = CardType.RepairCosts, description = "Make general repairs: $25/house, $100/hotel", perHouse = 25, perHotel = 100 },
        new() { type = CardType.GainPerProperty, description = "You are elected chairman — collect $50 per property", amount = 50 },
        new() { type = CardType.GainMoney, description = "Your investment matures — collect $100", amount = 100 },
        new() { type = CardType.LoseMoney, description = "Pay hospital fees of $100", amount = 100 }
    };
}

private static CardDef[] DefaultCommunityChestCards()
{
    return new CardDef[]
    {
        new() { type = CardType.GainMoney, description = "Bank error in your favor — collect $200", amount = 200 },
        new() { type = CardType.LoseMoney, description = "Doctor's fees — pay $50", amount = 50 },
        new() { type = CardType.GainMoney, description = "From sale of stock you get $50", amount = 50 },
        new() { type = CardType.GoToJail, description = "Go to Jail" },
        new() { type = CardType.GainMoney, description = "Holiday fund matures — receive $100", amount = 100 },
        new() { type = CardType.GainMoney, description = "Income tax refund — collect $20", amount = 20 },
        new() { type = CardType.GainMoney, description = "Life insurance matures — collect $100", amount = 100 },
        new() { type = CardType.LoseMoney, description = "Pay school fees of $50", amount = 50 },
        new() { type = CardType.GainMoney, description = "Receive $25 consultancy fee", amount = 25 },
        new() { type = CardType.RepairCosts, description = "You are assessed for street repairs: $40/house, $115/hotel", perHouse = 40, perHotel = 115 },
        new() { type = CardType.GainMoney, description = "You have won second prize in a beauty contest — collect $10", amount = 10 },
        new() { type = CardType.GainMoney, description = "You inherit $100", amount = 100 }
    };
}
```

- [ ] **Step 6: Verify compilation**

Open Unity Editor. Check Console for zero errors.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Shared/GameConfig.cs
git commit -m "feat: add config fields for railroads, utilities, card decks, color group colors"
```

---

### Task 3: Write expanded gameconfig.json (39 tiles + card decks)

**Files:**
- Modify: `Assets/Resources/gameconfig.json`

- [ ] **Step 1: Write the full expanded JSON config**

Replace the entire `gameconfig.json` with the 39-tile board, card decks, and new config fields. Key points:
- `TileType` integer values match the new enum: Go=0, Property=1, Tax=2, Chance=3, CommunityChest=4, GoToJail=5, Jail=6, Railroad=7, Utility=8
- `ColorGroup` integers: None=0, Brown=1, LightBlue=2, Pink=3, Orange=4, Red=5, Yellow=6, Green=7, Blue=8
- `CardType` integers: GainMoney=0, LoseMoney=1, GoToTile=2, GoToJail=3, RepairCosts=4, GainPerProperty=5
- Each property tile includes `colorGroup`, `rentTable` (6 values), `houseCost`, `hotelCost`
- Railroad tiles use `type: 7`, `price` set, other fields default
- Utility tiles use `type: 8`, `price` set, other fields default
- `sideLength` increased from 12.0 to 20.0 for 39 tiles
- `tileSize` decreased from 1.8 to 1.2 for tighter fit
- `jailTileIndex` updated from 6 to 10 (new Jail position)

The full JSON contains all 39 tiles from the spec's tile layout table, plus `chanceCards` (10 cards) and `communityChestCards` (12 cards), plus `railroadRentTiers` and `utilityRentFactors`.

```json
{
  "sideLength": 20.0,
  "tileSize": 1.2,
  "tiles": [
    { "name": "GO", "type": 0, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Mediterranean Ave", "type": 1, "price": 60, "baseRent": 2, "taxAmount": 0, "colorGroup": 1, "rentTable": [2,10,30,90,160,250], "houseCost": 50, "hotelCost": 50 },
    { "name": "Community Chest", "type": 4, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Baltic Ave", "type": 1, "price": 60, "baseRent": 4, "taxAmount": 0, "colorGroup": 1, "rentTable": [4,20,60,180,320,450], "houseCost": 50, "hotelCost": 50 },
    { "name": "Income Tax", "type": 2, "price": 0, "baseRent": 0, "taxAmount": 200, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Reading Railroad", "type": 7, "price": 200, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Oriental Ave", "type": 1, "price": 100, "baseRent": 6, "taxAmount": 0, "colorGroup": 2, "rentTable": [6,30,90,270,400,550], "houseCost": 50, "hotelCost": 50 },
    { "name": "Chance", "type": 3, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Vermont Ave", "type": 1, "price": 100, "baseRent": 6, "taxAmount": 0, "colorGroup": 2, "rentTable": [6,30,90,270,400,550], "houseCost": 50, "hotelCost": 50 },
    { "name": "Connecticut Ave", "type": 1, "price": 120, "baseRent": 8, "taxAmount": 0, "colorGroup": 2, "rentTable": [8,40,100,300,450,600], "houseCost": 50, "hotelCost": 50 },
    { "name": "Jail", "type": 6, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "St. Charles Place", "type": 1, "price": 140, "baseRent": 10, "taxAmount": 0, "colorGroup": 3, "rentTable": [10,50,150,450,625,750], "houseCost": 100, "hotelCost": 100 },
    { "name": "Electric Company", "type": 8, "price": 150, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "States Ave", "type": 1, "price": 140, "baseRent": 10, "taxAmount": 0, "colorGroup": 3, "rentTable": [10,50,150,450,625,750], "houseCost": 100, "hotelCost": 100 },
    { "name": "Virginia Ave", "type": 1, "price": 160, "baseRent": 12, "taxAmount": 0, "colorGroup": 3, "rentTable": [12,60,180,500,700,900], "houseCost": 100, "hotelCost": 100 },
    { "name": "Pennsylvania Railroad", "type": 7, "price": 200, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "St. James Place", "type": 1, "price": 180, "baseRent": 14, "taxAmount": 0, "colorGroup": 4, "rentTable": [14,70,200,550,750,950], "houseCost": 100, "hotelCost": 100 },
    { "name": "Community Chest", "type": 4, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Tennessee Ave", "type": 1, "price": 180, "baseRent": 14, "taxAmount": 0, "colorGroup": 4, "rentTable": [14,70,200,550,750,950], "houseCost": 100, "hotelCost": 100 },
    { "name": "New York Ave", "type": 1, "price": 200, "baseRent": 16, "taxAmount": 0, "colorGroup": 4, "rentTable": [16,80,220,600,800,1000], "houseCost": 100, "hotelCost": 100 },
    { "name": "Go To Jail", "type": 5, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Kentucky Ave", "type": 1, "price": 220, "baseRent": 18, "taxAmount": 0, "colorGroup": 5, "rentTable": [18,90,250,700,875,1050], "houseCost": 150, "hotelCost": 150 },
    { "name": "Chance", "type": 3, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Indiana Ave", "type": 1, "price": 220, "baseRent": 18, "taxAmount": 0, "colorGroup": 5, "rentTable": [18,90,250,700,875,1050], "houseCost": 150, "hotelCost": 150 },
    { "name": "Illinois Ave", "type": 1, "price": 240, "baseRent": 20, "taxAmount": 0, "colorGroup": 5, "rentTable": [20,100,300,750,925,1100], "houseCost": 150, "hotelCost": 150 },
    { "name": "B&O Railroad", "type": 7, "price": 200, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Atlantic Ave", "type": 1, "price": 260, "baseRent": 22, "taxAmount": 0, "colorGroup": 6, "rentTable": [22,110,330,800,975,1150], "houseCost": 150, "hotelCost": 150 },
    { "name": "Ventnor Ave", "type": 1, "price": 260, "baseRent": 22, "taxAmount": 0, "colorGroup": 6, "rentTable": [22,110,330,800,975,1150], "houseCost": 150, "hotelCost": 150 },
    { "name": "Water Works", "type": 8, "price": 150, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Marvin Gardens", "type": 1, "price": 280, "baseRent": 24, "taxAmount": 0, "colorGroup": 6, "rentTable": [24,120,360,850,1025,1200], "houseCost": 150, "hotelCost": 150 },
    { "name": "Pacific Ave", "type": 1, "price": 300, "baseRent": 26, "taxAmount": 0, "colorGroup": 7, "rentTable": [26,130,390,900,1100,1275], "houseCost": 200, "hotelCost": 200 },
    { "name": "North Carolina Ave", "type": 1, "price": 300, "baseRent": 26, "taxAmount": 0, "colorGroup": 7, "rentTable": [26,130,390,900,1100,1275], "houseCost": 200, "hotelCost": 200 },
    { "name": "Community Chest", "type": 4, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Pennsylvania Ave", "type": 1, "price": 320, "baseRent": 28, "taxAmount": 0, "colorGroup": 7, "rentTable": [28,150,450,1000,1200,1400], "houseCost": 200, "hotelCost": 200 },
    { "name": "Short Line Railroad", "type": 7, "price": 200, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Chance", "type": 3, "price": 0, "baseRent": 0, "taxAmount": 0, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Park Place", "type": 1, "price": 350, "baseRent": 35, "taxAmount": 0, "colorGroup": 8, "rentTable": [35,175,500,1100,1300,1500], "houseCost": 200, "hotelCost": 200 },
    { "name": "Luxury Tax", "type": 2, "price": 0, "baseRent": 0, "taxAmount": 100, "colorGroup": 0, "rentTable": [], "houseCost": 0, "hotelCost": 0 },
    { "name": "Boardwalk", "type": 1, "price": 400, "baseRent": 50, "taxAmount": 0, "colorGroup": 8, "rentTable": [50,200,600,1400,1700,2000], "houseCost": 200, "hotelCost": 200 }
  ],
  "startingCash": 1500,
  "goPayout": 200,
  "jailTileIndex": 10,
  "seed": 12345,
  "ticksPerSecond": 30,
  "targetWidth": 1080,
  "targetHeight": 1920,
  "cameraMargin": 1.0,
  "initialCharges": 3,
  "chargeCap": 20,
  "chargeInterval": 3.0,
  "railroadRentTiers": [25, 50, 100, 200],
  "utilityRentFactors": [4, 10],
  "chanceCards": [
    { "type": 0, "description": "Bank pays you dividend of $50", "amount": 50, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 2, "description": "Advance to GO — collect $200", "amount": 0, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 3, "description": "Go directly to Jail", "amount": 0, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 1, "description": "Speeding fine $15", "amount": 15, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Building loan matures — collect $150", "amount": 150, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 1, "description": "Pay poor tax of $15", "amount": 15, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 4, "description": "Make general repairs: $25/house, $100/hotel", "amount": 0, "tileIndex": 0, "perHouse": 25, "perHotel": 100 },
    { "type": 5, "description": "Collect $50 per property owned", "amount": 50, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Your investment matures — collect $100", "amount": 100, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 1, "description": "Pay hospital fees of $100", "amount": 100, "tileIndex": 0, "perHouse": 0, "perHotel": 0 }
  ],
  "communityChestCards": [
    { "type": 0, "description": "Bank error in your favor — collect $200", "amount": 200, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 1, "description": "Doctor's fees — pay $50", "amount": 50, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "From sale of stock you get $50", "amount": 50, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 3, "description": "Go to Jail", "amount": 0, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Holiday fund matures — receive $100", "amount": 100, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Income tax refund — collect $20", "amount": 20, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Life insurance matures — collect $100", "amount": 100, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 1, "description": "Pay school fees of $50", "amount": 50, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "Receive $25 consultancy fee", "amount": 25, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 4, "description": "Street repairs: $40/house, $115/hotel", "amount": 0, "tileIndex": 0, "perHouse": 40, "perHotel": 115 },
    { "type": 0, "description": "Beauty contest — collect $10", "amount": 10, "tileIndex": 0, "perHouse": 0, "perHotel": 0 },
    { "type": 0, "description": "You inherit $100", "amount": 100, "tileIndex": 0, "perHouse": 0, "perHotel": 0 }
  ]
}
```

- [ ] **Step 2: Verify config loads**

Open Unity Editor, enter Play Mode. The board should render with 39 tiles. Tiles may all be one color (tile color logic not updated yet), but the layout should be visible without overlap.

- [ ] **Step 3: Commit**

```bash
git add Assets/Resources/gameconfig.json
git commit -m "feat: expand gameconfig.json to 39 tiles with card decks and rent tables"
```

---

### Task 4: Update ColorForTile to support new tile types and color groups

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs:158-170`

- [ ] **Step 1: Update ColorForTile method**

Replace the `ColorForTile` method with one that uses color group for properties:

```csharp
private Color ColorForTile(TileDef tile)
{
    switch (tile.type)
    {
        case TileType.Property:
            return ColorForGroup(tile.colorGroup);
        case TileType.Tax: return config.taxColor;
        case TileType.Chance: return config.chanceColor;
        case TileType.CommunityChest: return config.communityChestColor;
        case TileType.Go: return config.goColor;
        case TileType.Jail: return config.jailColor;
        case TileType.GoToJail: return config.gotoJailColor;
        case TileType.Railroad: return config.railroadColor;
        case TileType.Utility: return config.utilityColor;
        default: return Color.white;
    }
}

private Color ColorForGroup(ColorGroup g)
{
    switch (g)
    {
        case ColorGroup.Brown: return config.brownGroup;
        case ColorGroup.LightBlue: return config.lightBlueGroup;
        case ColorGroup.Pink: return config.pinkGroup;
        case ColorGroup.Orange: return config.orangeGroup;
        case ColorGroup.Red: return config.redGroup;
        case ColorGroup.Yellow: return config.yellowGroup;
        case ColorGroup.Green: return config.greenGroup;
        case ColorGroup.Blue: return config.blueGroup;
        default: return config.propertyColor;
    }
}
```

- [ ] **Step 2: Update BuildBoard call site**

In `Assets/Scripts/MonopolyLite/Core/Main.cs:122`, change:

```csharp
// Old:
sr.sprite = Sprites.Square(64, ColorForTile(t.type));
// New:
sr.sprite = Sprites.Square(64, ColorForTile(t));
```

This passes the full `TileDef` instead of just the `TileType`.

- [ ] **Step 3: Verify in Unity Editor**

Enter Play Mode. The board should show 39 tiles with distinct colors per color group — Brown tiles should be brown, Blue tiles blue, etc. Railroad tiles should be gray, Utility tiles should be yellow-brown.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs Assets/Scripts/MonopolyLite/Core/Main.cs
git commit -m "feat: color-code tiles by color group and tile type"
```

---

### Task 5: Update Recorder for expanded config

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Recorder.cs:8-12,82-88`

- [ ] **Step 1: Expand CmdType enum**

Replace the existing `CmdType` enum:

```csharp
public enum CmdType
{
    Roll,
    ToggleMultiplier,
    BuyProperty,
    DeclineProperty,
    BuildHouse
}
```

- [ ] **Step 2: Update ToJson method**

Add the new config fields to the `ToJson` method:

```csharp
private GameConfigJson ToJson(GameConfig c)
{
    return new GameConfigJson
    {
        sideLength = c.sideLength, tileSize = c.tileSize, tiles = c.tiles,
        startingCash = c.startingCash, goPayout = c.goPayout, jailTileIndex = c.jailTileIndex,
        seed = c.seed, ticksPerSecond = c.ticksPerSecond, targetWidth = c.targetWidth, targetHeight = c.targetHeight,
        cameraMargin = c.cameraMargin, initialCharges = c.initialCharges, chargeCap = c.chargeCap, chargeInterval = c.chargeInterval,
        railroadRentTiers = c.railroadRentTiers, utilityRentFactors = c.utilityRentFactors,
        chanceCards = c.chanceCards, communityChestCards = c.communityChestCards
    };
}
```

- [ ] **Step 3: Verify compilation**

Open Unity Editor. Zero compilation errors.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Recorder.cs
git commit -m "feat: expand Recorder with new CmdType entries and config fields"
```

---

## Chunk 2: Game State & Core Landing Logic

### Task 6: Add new game state fields to Main.cs

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.cs:23,77-103`

- [ ] **Step 1: Add new state fields**

After the existing field declarations (around line 23), add:

```csharp
private int[] developmentLevels;
private List<int> chanceDeckOrder, communityChestDeckOrder;
private int chanceDrawIndex, communityChestDrawIndex;
private HashSet<int> declinedProperties;
private bool gameOver;
private bool playerWon;
private int totalTurns;
private int totalMoneyEarned;
```

Add required using at top of file:

```csharp
using System.Collections.Generic;
```

- [ ] **Step 2: Initialize new state in InitState**

Add at the end of `InitState()`, after `gainMultiplier = 1;`:

```csharp
developmentLevels = new int[config.tiles.Length];
declinedProperties = new HashSet<int>();
gameOver = false;
playerWon = false;
totalTurns = 0;
totalMoneyEarned = 0;

// Shuffle card decks
chanceDeckOrder = ShuffleDeck(config.chanceCards != null ? config.chanceCards.Length : 0);
communityChestDeckOrder = ShuffleDeck(config.communityChestCards != null ? config.communityChestCards.Length : 0);
chanceDrawIndex = 0;
communityChestDrawIndex = 0;
```

- [ ] **Step 3: Add ShuffleDeck helper**

Add to `Main.cs`:

```csharp
private List<int> ShuffleDeck(int count)
{
    List<int> deck = new(count);
    for (int i = 0; i < count; i++) deck.Add(i);
    for (int i = count - 1; i > 0; i--)
    {
        int j = rng.Next(0, i + 1);
        (deck[i], deck[j]) = (deck[j], deck[i]);
    }
    return deck;
}
```

- [ ] **Step 4: Verify compilation**

Open Unity Editor. Zero compilation errors. Enter Play Mode — game should behave as before (new state initialized but not yet used).

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.cs
git commit -m "feat: add game state for development levels, card decks, win/lose tracking"
```

---

### Task 7: Update ResolveLanding for Property with rent tables

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs:57-87`

- [ ] **Step 0: Add required using directive**

At the top of `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`, add:

```csharp
using System.Collections.Generic;
```

This is needed for `List<int>` used in `GetBuildableTiles` (Task 10) and later tasks.

- [ ] **Step 1: Add helper methods for property logic**

Add to `Main.Logic.cs`:

```csharp
private int CountOwnedInGroup(int player, ColorGroup group)
{
    int count = 0;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (config.tiles[i].type == TileType.Property &&
            config.tiles[i].colorGroup == group &&
            tileOwner[i] == player)
            count++;
    }
    return count;
}

private int CountTilesInGroup(ColorGroup group)
{
    int count = 0;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (config.tiles[i].type == TileType.Property && config.tiles[i].colorGroup == group)
            count++;
    }
    return count;
}

private bool OwnsFullGroup(int player, ColorGroup group)
{
    return group != ColorGroup.None && CountOwnedInGroup(player, group) == CountTilesInGroup(group);
}

private int GetPropertyRent(int tileIndex)
{
    TileDef t = config.tiles[tileIndex];
    int level = developmentLevels[tileIndex];
    if (t.rentTable != null && t.rentTable.Length > level)
        return t.rentTable[level];
    return t.baseRent;
}
```

- [ ] **Step 2: Update the Property case in ResolveLanding**

Replace the `TileType.Property` case in `ResolveLanding`:

```csharp
case TileType.Property:
    int o = tileOwner[pos[p]];
    if (o == -1)
    {
        // Unowned — auto-buy if affordable (buy/decline UI comes in Task 11)
        if (cash[p] >= t.price)
        {
            cash[p] -= t.price;
            tileOwner[pos[p]] = p;
        }
    }
    else if (o != p)
    {
        int rent = GetPropertyRent(pos[p]);
        if (OwnsFullGroup(o, t.colorGroup) && developmentLevels[pos[p]] == 0)
            rent *= 2; // Double rent for full group with no houses
        cash[p] -= rent;
        cash[o] += rent;
    }
    break;
```

- [ ] **Step 3: Verify in Unity Editor**

Enter Play Mode. Auto-buy behavior should work as before. Rent is now based on `rentTable[0]` (same as old `baseRent`).

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: property rent uses rent tables with full-group double rent bonus"
```

---

### Task 8: Add Railroad and Utility landing logic

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs:57-87`

- [ ] **Step 1: Add railroad/utility helper methods**

Add to `Main.Logic.cs`:

```csharp
private int CountOwnedByType(int player, TileType type)
{
    int count = 0;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (config.tiles[i].type == type && tileOwner[i] == player)
            count++;
    }
    return count;
}
```

- [ ] **Step 2: Add Railroad and Utility cases to ResolveLanding**

Add these cases in the `switch` statement in `ResolveLanding`, before the `case TileType.GoToJail:` line:

```csharp
case TileType.Railroad:
{
    int ro = tileOwner[pos[p]];
    if (ro == -1)
    {
        if (cash[p] >= t.price)
        {
            cash[p] -= t.price;
            tileOwner[pos[p]] = p;
        }
    }
    else if (ro != p)
    {
        int owned = CountOwnedByType(ro, TileType.Railroad);
        int rent = config.railroadRentTiers[Mathf.Clamp(owned - 1, 0, config.railroadRentTiers.Length - 1)];
        cash[p] -= rent;
        cash[ro] += rent;
    }
    break;
}
case TileType.Utility:
{
    int uo = tileOwner[pos[p]];
    if (uo == -1)
    {
        if (cash[p] >= t.price)
        {
            cash[p] -= t.price;
            tileOwner[pos[p]] = p;
        }
    }
    else if (uo != p)
    {
        int owned = CountOwnedByType(uo, TileType.Utility);
        int factor = config.utilityRentFactors[Mathf.Clamp(owned - 1, 0, config.utilityRentFactors.Length - 1)];
        int rent = (lastD1 + lastD2) * factor;
        cash[p] -= rent;
        cash[uo] += rent;
    }
    break;
}
```

- [ ] **Step 3: Verify in Unity Editor**

Enter Play Mode. Land on a Railroad or Utility — should auto-buy. Verify no errors in Console.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add railroad and utility landing logic with count-based rent"
```

---

### Task 9: Add Chance and Community Chest card drawing logic

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`

- [ ] **Step 1: Add DrawCard method**

Add to `Main.Logic.cs`:

```csharp
private CardDef DrawCard(bool isChance)
{
    CardDef[] deck = isChance ? config.chanceCards : config.communityChestCards;
    List<int> order = isChance ? chanceDeckOrder : communityChestDeckOrder;
    ref int drawIndex = ref (isChance ? ref chanceDrawIndex : ref communityChestDrawIndex);

    if (deck == null || deck.Length == 0)
        return new CardDef { type = CardType.GainMoney, amount = 0, description = "Empty deck" };

    if (drawIndex >= order.Count)
    {
        // Reshuffle
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        drawIndex = 0;
    }

    CardDef card = deck[order[drawIndex]];
    drawIndex++;
    return card;
}
```

- [ ] **Step 2: Add ResolveCard method**

Add to `Main.Logic.cs`:

```csharp
private void ResolveCard(int p, CardDef card)
{
    switch (card.type)
    {
        case CardType.GainMoney:
            int gain = card.amount * gainMultiplier;
            cash[p] += gain;
            totalMoneyEarned += gain;
            break;

        case CardType.LoseMoney:
            cash[p] -= card.amount;
            break;

        case CardType.GoToTile:
        {
            int from = pos[p];
            int to = card.tileIndex;
            // Check if we pass GO (moving forward)
            if (to < from)
            {
                cash[p] += config.goPayout * gainMultiplier;
                totalMoneyEarned += config.goPayout * gainMultiplier;
            }
            pos[p] = to;
            ResolveLanding(p);
            break;
        }

        case CardType.GoToJail:
            SendToJail(p);
            break;

        case CardType.RepairCosts:
        {
            int cost = 0;
            for (int i = 0; i < developmentLevels.Length; i++)
            {
                if (tileOwner[i] == p)
                {
                    int level = developmentLevels[i];
                    if (level >= 5)
                        cost += card.perHotel;
                    else if (level > 0)
                        cost += card.perHouse * level;
                }
            }
            cash[p] -= cost;
            break;
        }

        case CardType.GainPerProperty:
        {
            int propCount = 0;
            for (int i = 0; i < tileOwner.Length; i++)
            {
                if (tileOwner[i] == p) propCount++;
            }
            int total = card.amount * propCount * gainMultiplier;
            cash[p] += total;
            totalMoneyEarned += total;
            break;
        }
    }
}
```

- [ ] **Step 3: Update ResolveLanding Chance/CommunityChest cases**

Replace the old `TileType.Chest` case with two new cases:

```csharp
case TileType.Chance:
{
    CardDef card = DrawCard(true);
    ResolveCard(p, card);
    break;
}
case TileType.CommunityChest:
{
    CardDef card = DrawCard(false);
    ResolveCard(p, card);
    break;
}
```

Remove the old `case TileType.Chest:` block entirely.

- [ ] **Step 4: Update TryRoll to track totalMoneyEarned for GO payout**

In `TryRoll()`, after the line `if (passed) cash[currentPlayer] += config.goPayout * gainMultiplier;`, add:

```csharp
if (passed) totalMoneyEarned += config.goPayout * gainMultiplier;
```

- [ ] **Step 5: Update TryRoll to cancel doubles after card movement**

In `TryRoll()`, after `ResolveLanding(currentPlayer);`, add a check: if the player's position changed due to a card (GoToTile or GoToJail), cancel the doubles extra turn:

```csharp
bool movedByCard = pos[currentPlayer] != next;
```

Then change the doubles logic:

```csharp
if (dbl && !movedByCard)
{
    doublesInRow[currentPlayer]++;
    if (doublesInRow[currentPlayer] >= 3)
    {
        SendToJail(currentPlayer);
        EndTurn();
    }
}
else
{
    doublesInRow[currentPlayer] = 0;
    EndTurn();
}
```

- [ ] **Step 6: Verify in Unity Editor**

Enter Play Mode. Land on a Chance or Community Chest tile. Observe cash changes in the stats line. Verify no Console errors.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add Chance/Community Chest card drawing and resolution logic"
```

---

## Chunk 3: Houses, Decline-to-Discount, Win Conditions & UI

### Task 10: Add house/hotel building logic

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`

- [ ] **Step 1: Add CanBuildOnTile and BuildHouse methods**

Add to `Main.Logic.cs`:

```csharp
private bool CanBuildOnTile(int player, int tileIndex)
{
    TileDef t = config.tiles[tileIndex];
    if (t.type != TileType.Property) return false;
    if (tileOwner[tileIndex] != player) return false;
    if (!OwnsFullGroup(player, t.colorGroup)) return false;
    int level = developmentLevels[tileIndex];
    if (level >= 5) return false; // Already hotel
    int cost = level < 4 ? t.houseCost : t.hotelCost;
    return cash[player] >= cost;
}

private void BuildHouse(int player, int tileIndex)
{
    if (!CanBuildOnTile(player, tileIndex)) return;
    TileDef t = config.tiles[tileIndex];
    int level = developmentLevels[tileIndex];
    int cost = level < 4 ? t.houseCost : t.hotelCost;
    cash[player] -= cost;
    developmentLevels[tileIndex]++;
}
```

- [ ] **Step 2: Add method to find buildable properties for current player**

```csharp
private List<int> GetBuildableTiles(int player)
{
    List<int> result = new();
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (CanBuildOnTile(player, i)) result.Add(i);
    }
    return result;
}
```

- [ ] **Step 3: Verify compilation**

Open Unity Editor. Zero errors.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add house/hotel building logic with full-group ownership check"
```

---

### Task 11: Add decline-to-discount logic

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`

- [ ] **Step 1: Add buy/decline methods**

Add to `Main.Logic.cs`:

```csharp
private int GetPropertyPrice(int tileIndex)
{
    int basePrice = config.tiles[tileIndex].price;
    if (declinedProperties.Contains(tileIndex))
        return Mathf.RoundToInt(basePrice * 0.8f); // 20% discount
    return basePrice;
}

private void BuyProperty(int player, int tileIndex)
{
    int price = GetPropertyPrice(tileIndex);
    if (cash[player] < price) return;
    cash[player] -= price;
    tileOwner[tileIndex] = player;
    declinedProperties.Remove(tileIndex);
}

private void DeclineProperty(int tileIndex)
{
    if (declinedProperties.Contains(tileIndex))
        declinedProperties.Remove(tileIndex); // Was already declined, discount offered and declined again — reset
    else
        declinedProperties.Add(tileIndex);
}
```

- [ ] **Step 2: Update ResolveLanding Property/Railroad/Utility to NOT auto-buy**

Change the unowned property handling in all three purchasable tile cases. Instead of auto-buying, set a flag to show the buy/decline prompt. Add a field to `Main.cs`:

In `Assets/Scripts/MonopolyLite/Core/Main.cs`, add after the other state fields:

```csharp
private int pendingBuyTileIndex = -1;
```

Also add `pendingBuyTileIndex = -1;` at the end of `InitState()` (for game restart safety).

Then in `ResolveLanding` in `Main.Logic.cs`, update the unowned branch for Property:

```csharp
case TileType.Property:
{
    int o = tileOwner[pos[p]];
    if (o == -1)
    {
        int price = GetPropertyPrice(pos[p]);
        if (cash[p] >= price)
            pendingBuyTileIndex = pos[p]; // Show buy/decline UI
        // If can't afford, nothing happens
    }
    else if (o != p)
    {
        int rent = GetPropertyRent(pos[p]);
        if (OwnsFullGroup(o, t.colorGroup) && developmentLevels[pos[p]] == 0)
            rent *= 2;
        cash[p] -= rent;
        cash[o] += rent;
    }
    break;
}
```

Apply the same pattern for Railroad and Utility unowned branches — set `pendingBuyTileIndex = pos[p]` instead of auto-buying.

- [ ] **Step 3: Block dice rolling while buy prompt is pending**

In `TryRoll()`, add at the very top:

```csharp
if (pendingBuyTileIndex >= 0) return;
if (gameOver) return;
```

- [ ] **Step 4: Verify compilation**

Open Unity Editor. Zero errors. Note: the player can no longer buy — buy UI comes in Task 13.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.cs Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add decline-to-discount mechanism and pending buy prompt state"
```

---

### Task 12: Add win/lose condition checks

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`

- [ ] **Step 1: Add win/lose check methods**

Add to `Main.Logic.cs`:

```csharp
private void CheckWinLose(int player)
{
    // Lose: bankruptcy
    if (cash[player] < 0)
    {
        // Revert payment — spec says cash stays at pre-payment value
        // Note: the caller should check BEFORE applying payment.
        // This is a safety net.
        gameOver = true;
        playerWon = false;
        return;
    }

    // Win: own all purchasable properties
    bool ownsAll = true;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        TileType tt = config.tiles[i].type;
        if (tt == TileType.Property || tt == TileType.Railroad || tt == TileType.Utility)
        {
            if (tileOwner[i] != player)
            {
                ownsAll = false;
                break;
            }
        }
    }
    if (ownsAll)
    {
        gameOver = true;
        playerWon = true;
    }
}
```

- [ ] **Step 2: Add bankruptcy-safe payment helper**

```csharp
private bool TryDeductCash(int player, int amount)
{
    if (cash[player] < amount)
    {
        gameOver = true;
        playerWon = false;
        return false;
    }
    cash[player] -= amount;
    return true;
}
```

- [ ] **Step 3: Update all cash deductions to use TryDeductCash**

In `ResolveLanding`, update:
- Tax case: `case TileType.Tax: TryDeductCash(p, t.taxAmount); break;`
- Property rent: replace `cash[p] -= rent;` with `TryDeductCash(p, rent);`
- Railroad rent: replace `cash[p] -= rent;` with `TryDeductCash(p, rent);`
- Utility rent: replace `cash[p] -= rent;` with `TryDeductCash(p, rent);`

In `ResolveCard`, update:
- `LoseMoney`: replace `cash[p] -= card.amount;` with `TryDeductCash(p, card.amount);`
- `RepairCosts`: replace `cash[p] -= cost;` with `TryDeductCash(p, cost);`

In `BuyProperty`: replace `cash[player] -= price;` with `if (!TryDeductCash(player, price)) return;`

In `BuildHouse`: replace `cash[player] -= cost;` with `if (!TryDeductCash(player, cost)) return;`

- [ ] **Step 4: Call CheckWinLose after property purchase**

In `BuyProperty`, after `tileOwner[tileIndex] = player;`:

```csharp
CheckWinLose(player);
```

- [ ] **Step 5: Call CheckWinLose at end of ResolveLanding**

At the end of the `ResolveLanding` method, add:

```csharp
if (!gameOver) CheckWinLose(p);
```

- [ ] **Step 6: Update EndTurn to increment totalTurns**

In `EndTurn()`:

```csharp
private void EndTurn()
{
    totalTurns++;
    currentPlayer = (currentPlayer + 1) % playerCount;
}
```

- [ ] **Step 7: Verify compilation**

Open Unity Editor. Zero errors.

- [ ] **Step 8: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add win/lose condition checks with bankruptcy-safe payments"
```

---

### Task 13: Add Buy/Decline UI and Build Houses button

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Input.cs`
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.cs`

- [ ] **Step 1: Add UI elements to Main.cs**

Add fields to `Main.cs` after the existing UI fields:

```csharp
private Transform buyBtn, declineBtn, buildBtn;
private TextMeshPro buyBtnLabel, declineBtnLabel, buildBtnLabel;
private SpriteRenderer buyBtnSR, declineBtnSR, buildBtnSR;
private TextMeshPro cardRevealText;
private TextMeshPro gameOverText;
```

- [ ] **Step 2: Build the new UI elements in BuildUI**

At the end of `BuildUI()` in `Main.cs`, add:

```csharp
// Buy button
buyBtn = new GameObject("BuyButton").transform;
buyBtn.SetParent(transform, false);
buyBtnSR = buyBtn.gameObject.AddComponent<SpriteRenderer>();
buyBtnSR.sprite = Sprites.Square(96, new Color(0.15f, 0.75f, 0.30f));
buyBtn.localScale = Vector3.one * 1.2f;
buyBtnLabel = new GameObject("Label").AddComponent<TextMeshPro>();
buyBtnLabel.transform.SetParent(buyBtn, false);
buyBtnLabel.alignment = TextAlignmentOptions.Center;
buyBtnLabel.fontSize = 3;
buyBtn.gameObject.SetActive(false);

// Decline button
declineBtn = new GameObject("DeclineButton").transform;
declineBtn.SetParent(transform, false);
declineBtnSR = declineBtn.gameObject.AddComponent<SpriteRenderer>();
declineBtnSR.sprite = Sprites.Square(96, new Color(0.85f, 0.25f, 0.25f));
declineBtn.localScale = Vector3.one * 1.2f;
declineBtnLabel = new GameObject("Label").AddComponent<TextMeshPro>();
declineBtnLabel.transform.SetParent(declineBtn, false);
declineBtnLabel.alignment = TextAlignmentOptions.Center;
declineBtnLabel.fontSize = 3;
declineBtn.gameObject.SetActive(false);

// Build button
buildBtn = new GameObject("BuildButton").transform;
buildBtn.SetParent(transform, false);
buildBtnSR = buildBtn.gameObject.AddComponent<SpriteRenderer>();
buildBtnSR.sprite = Sprites.Square(96, new Color(0.90f, 0.70f, 0.15f));
buildBtn.localScale = Vector3.one * 1.0f;
buildBtnLabel = new GameObject("Label").AddComponent<TextMeshPro>();
buildBtnLabel.transform.SetParent(buildBtn, false);
buildBtnLabel.alignment = TextAlignmentOptions.Center;
buildBtnLabel.fontSize = 2.5f;
buildBtn.gameObject.SetActive(false);

// Card reveal text
GameObject cardGo = new("CardReveal");
cardGo.transform.SetParent(transform, false);
cardRevealText = cardGo.AddComponent<TextMeshPro>();
cardRevealText.fontSize = 3;
cardRevealText.alignment = TextAlignmentOptions.Center;
cardRevealText.rectTransform.sizeDelta = new Vector2(10, 2);
cardRevealText.gameObject.SetActive(false);

// Game over text
GameObject goGo = new("GameOver");
goGo.transform.SetParent(transform, false);
gameOverText = goGo.AddComponent<TextMeshPro>();
gameOverText.fontSize = 4;
gameOverText.alignment = TextAlignmentOptions.Center;
gameOverText.rectTransform.sizeDelta = new Vector2(14, 8);
gameOverText.gameObject.SetActive(false);
```

- [ ] **Step 3: Update UpdateUIPositions with buy/decline/build buttons**

In `Main.Input.cs`, add after the dice position update (before the tap detection code):

```csharp
// Buy/Decline buttons
if (pendingBuyTileIndex >= 0)
{
    buyBtn.gameObject.SetActive(true);
    declineBtn.gameObject.SetActive(true);
    int price = GetPropertyPrice(pendingBuyTileIndex);
    bool isDiscount = declinedProperties.Contains(pendingBuyTileIndex);
    string tileName = config.tiles[pendingBuyTileIndex].name;
    buyBtnLabel.text = $"BUY\n${price}" + (isDiscount ? "\n20% OFF" : "");
    declineBtnLabel.text = "PASS";
    buyBtn.position = new Vector3(c.x - 1.5f, Mathf.Lerp(bottom, c.y, 0.45f), 0);
    declineBtn.position = new Vector3(c.x + 1.5f, Mathf.Lerp(bottom, c.y, 0.45f), 0);
    buyBtnLabel.rectTransform.sizeDelta = new Vector2(4, 3);
    declineBtnLabel.rectTransform.sizeDelta = new Vector2(4, 3);
    buyBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
    declineBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
    buyBtnLabel.sortingOrder = 101;
    declineBtnLabel.sortingOrder = 101;
}
else
{
    buyBtn.gameObject.SetActive(false);
    declineBtn.gameObject.SetActive(false);
}

// Build button — show when player has buildable tiles
bool canBuild = !gameOver && pendingBuyTileIndex < 0 && GetBuildableTiles(currentPlayer).Count > 0;
buildBtn.gameObject.SetActive(canBuild);
if (canBuild)
{
    buildBtn.position = rollBtn.position + new Vector3(-1.2f, 1.2f, 0);
    buildBtnLabel.text = "BUILD";
    buildBtnLabel.rectTransform.sizeDelta = new Vector2(4, 2);
    buildBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
    buildBtnLabel.sortingOrder = 101;
}

// Game over screen
if (gameOver)
{
    gameOverText.gameObject.SetActive(true);
    gameOverText.transform.position = c + new Vector3(0, 0, 0);
    gameOverText.sortingOrder = 200;
    int propsOwned = 0;
    int housesBuilt = 0;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (tileOwner[i] == 0)
        {
            propsOwned++;
            housesBuilt += developmentLevels[i];
        }
    }
    string result = playerWon ? "YOU WIN!" : "BANKRUPT!";
    gameOverText.text = $"{result}\n\nTurns: {totalTurns}\nMoney Earned: ${totalMoneyEarned}\nProperties: {propsOwned}\nDevelopment: {housesBuilt}";
    rollBtn.gameObject.SetActive(false);
    multBtn.gameObject.SetActive(false);
}
```

- [ ] **Step 4: Add tap handling for Buy/Decline/Build buttons**

In the `if (pressed)` block in `UpdateUIPositions`, add before the existing roll button check:

```csharp
if (gameOver) { /* ignore taps */ }
else if (pendingBuyTileIndex >= 0)
{
    if (Vector2.Distance(tap, new Vector2(buyBtn.position.x, buyBtn.position.y)) <= 1.2f)
    {
        BuyProperty(currentPlayer, pendingBuyTileIndex);
        pendingBuyTileIndex = -1;
    }
    else if (Vector2.Distance(tap, new Vector2(declineBtn.position.x, declineBtn.position.y)) <= 1.2f)
    {
        DeclineProperty(pendingBuyTileIndex);
        pendingBuyTileIndex = -1;
    }
}
else
{
```

Close the `else` block after the existing roll/multiplier tap code with `}`.

Add build button tap handling inside the `else` block, after the multiplier check:

```csharp
else if (buildBtn.gameObject.activeSelf && Vector2.Distance(tap, new Vector2(buildBtn.position.x, buildBtn.position.y)) <= 1.0f)
{
    // Build on first available tile (simple: auto-pick lowest index buildable tile)
    List<int> buildable = GetBuildableTiles(currentPlayer);
    if (buildable.Count > 0)
        BuildHouse(currentPlayer, buildable[0]);
}
```

Add required using at top of `Main.Input.cs`:

```csharp
using System.Collections.Generic;
```

- [ ] **Step 5: Update UpdateStats to show development levels**

In `Main.Logic.cs`, update the per-player stats line to include development info:

```csharp
for (int p = 0; p < playerCount; p++)
{
    int tile = pos[p];
    int devTotal = 0;
    int propsOwned = 0;
    for (int i = 0; i < config.tiles.Length; i++)
    {
        if (tileOwner[i] == p) { propsOwned++; devTotal += developmentLevels[i]; }
    }
    statsLines[2 + p].text = $"P{p} ${cash[p]} Props:{propsOwned} Dev:{devTotal} T{tile}";
}
```

- [ ] **Step 6: Verify in Unity Editor**

Enter Play Mode:
1. Roll dice and land on a property — Buy/Decline buttons should appear
2. Tap BUY — property is purchased, buttons disappear
3. Own all properties in a color group — BUILD button should appear
4. Tap BUILD — development level increases, cash decreases
5. Eventually go bankrupt or own everything — game over screen appears

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.cs Assets/Scripts/MonopolyLite/Core/Main.Input.cs Assets/Scripts/MonopolyLite/Core/Main.Logic.cs
git commit -m "feat: add buy/decline UI, build houses button, and game over screen"
```

---

### Task 14: Add card reveal display

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs`
- Modify: `Assets/Scripts/MonopolyLite/Core/Main.cs`

- [ ] **Step 1: Add card reveal state**

In `Main.cs`, add field:

```csharp
private float cardRevealTimer;
private string lastCardDescription = "";
```

Also add these resets at the end of `InitState()`:

```csharp
cardRevealTimer = 0f;
lastCardDescription = "";
```

- [ ] **Step 2: Show card text when drawn**

In `ResolveLanding`, update the Chance and CommunityChest cases to set the card reveal:

```csharp
case TileType.Chance:
{
    CardDef card = DrawCard(true);
    lastCardDescription = "CHANCE: " + card.description;
    cardRevealTimer = 2f;
    ResolveCard(p, card);
    break;
}
case TileType.CommunityChest:
{
    CardDef card = DrawCard(false);
    lastCardDescription = "CHEST: " + card.description;
    cardRevealTimer = 2f;
    ResolveCard(p, card);
    break;
}
```

- [ ] **Step 3: Update card reveal display in UpdateUIPositions**

In `Main.Input.cs`, add in `UpdateUIPositions` (before the tap detection):

```csharp
// Card reveal
if (cardRevealTimer > 0)
{
    cardRevealTimer -= Time.deltaTime;
    cardRevealText.gameObject.SetActive(true);
    cardRevealText.text = lastCardDescription;
    cardRevealText.transform.position = c + new Vector3(0, 2f, 0);
    cardRevealText.sortingOrder = 150;
}
else
{
    cardRevealText.gameObject.SetActive(false);
}
```

- [ ] **Step 4: Verify in Unity Editor**

Enter Play Mode. Land on Chance or Community Chest — card text appears for 2 seconds.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/Main.cs Assets/Scripts/MonopolyLite/Core/Main.Logic.cs Assets/Scripts/MonopolyLite/Core/Main.Input.cs
git commit -m "feat: show card reveal text when drawing Chance/Community Chest"
```

---

### Task 15: Final integration verification

**Files:** None (verification only)

- [ ] **Step 1: Full playthrough test**

Enter Play Mode in Unity Editor. Play through a complete game verifying:
1. Board renders 39 tiles with correct color groups
2. Rolling dice moves the token around the full board
3. Passing GO grants salary (affected by multiplier)
4. Landing on unowned property shows Buy/Decline prompt
5. Buying a property works, cash decreases
6. Declining a property marks it — next landing shows 20% discount
7. Landing on Chance/Community Chest draws a card, shows description
8. Card effects work: GainMoney, LoseMoney, GoToTile, GoToJail, RepairCosts, GainPerProperty
9. GainMoney/GainPerProperty cards are affected by multiplier
10. Tax tiles deduct correct amounts
11. Railroad rent scales with number owned
12. Utility rent = dice roll × factor
13. Owning all properties in a color group enables BUILD button
14. Building increases development level, rent increases
15. Full group with no houses doubles base rent
16. Going bankrupt shows BANKRUPT game over screen
17. Owning all properties shows YOU WIN game over screen
18. Stats display shows properties owned and development total
19. Jail still works (3-turn lockout)
20. Triple doubles still send to jail

- [ ] **Step 2: Commit final state if any tweaks needed**

```bash
git add -A
git commit -m "fix: integration tweaks from playthrough testing"
```

(Skip this commit if no changes were needed.)
