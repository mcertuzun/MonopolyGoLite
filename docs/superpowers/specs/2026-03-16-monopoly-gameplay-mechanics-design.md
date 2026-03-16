# MonopolyGoLite — Gameplay Mechanics Expansion

**Date:** 2026-03-16
**Status:** Approved
**Goal:** Add classic Monopoly mechanics with a casual/mobile-friendly (Monopoly Go-style) twist to the existing single-player game.

## Overview

Expand MonopolyGoLite from a basic 12-tile board with simple buy/rent mechanics into a full-featured Monopoly experience with color groups, houses/hotels, Chance/Community Chest card decks, railroads/utilities, a decline-to-discount mechanism, and target-based win/lose conditions. All features integrate with the existing charge and multiplier systems.

## Design Principles

- **Monopoly Go-style casual:** No even-building rule, unlimited house supply, simplified rules
- **Multiplier stays income-only:** The 1x/2x/3x multiplier affects GO payout and `GainMoney`/`GainPerProperty` card rewards only — rent (both paid and received), taxes, `LoseMoney` cards, and `RepairCosts` are NOT affected
- **Config-driven:** All tile definitions, rent tables, card decks, and costs live in `gameconfig.json`
- **Single-player:** Target-based goals, no AI opponents, no traditional auctions
- **Existing jail mechanics unchanged:** 3-turn lockout, doubles escape — no fine-to-exit option added

## Section 1: Data Model Changes

### Color Groups

- New `ColorGroup` enum: `None`, `Brown`, `LightBlue`, `Pink`, `Orange`, `Red`, `Yellow`, `Green`, `Blue`
- `None` is used for non-property tiles (railroads, utilities, special tiles)
- Each property tile gets:
  - `colorGroup` — which group it belongs to
  - `rentTable` — array of 6 values: [base rent, 1 house, 2 houses, 3 houses, 4 houses, hotel]
  - `houseCost` — cost to build one house on this property
  - `hotelCost` — cost to upgrade from 4 houses to hotel
- Group sizes: Brown = 2, LightBlue = 3, Pink = 3, Orange = 3, Red = 3, Yellow = 3, Green = 3, Blue = 2
- Group size is derived at runtime by counting properties per group in the config — not hardcoded

### Tile Types

Expand the existing `TileType` enum to include:
- `Go`, `Property`, `Railroad`, `Utility`, `Chance`, `CommunityChest`, `Tax`, `Jail`, `GoToJail`

The existing `TileType.Chest` is replaced by two distinct types: `Chance` and `CommunityChest`. Each draws from its own deck.

### Railroads

- Rent based on how many railroads the player owns:
  - 1 owned = $25, 2 owned = $50, 3 owned = $100, 4 owned = $200
- Rent tiers defined as a top-level config array: `railroadRentTiers: [25, 50, 100, 200]`
- Railroad tiles have `tileType: "Railroad"` and `price` but no `colorGroup`, `rentTable`, or `houseCost`

### Utilities

- Rent = dice roll total × utility rent factor based on utilities owned:
  - 1 owned = 4× dice roll
  - 2 owned = 10× dice roll
- **Note:** The "utility rent factor" (4×/10×) is distinct from the global gain multiplier (1x/2x/3x). These do not interact.
- Utility rent factors defined as top-level config array: `utilityRentFactors: [4, 10]`
- Utility tiles have `tileType: "Utility"` and `price` but no `colorGroup`, `rentTable`, or `houseCost`

### Houses/Hotels

- Each owned property tracks `developmentLevel`: 0 (empty), 1-4 (houses), 5 (hotel)
- No even-building rule — build on any property in a completed color group
- No limited house supply — unlimited (casual mobile style)
- Player must own all properties in a color group before building

### JSON Schema for Expanded TileDef

```json
{
  "tileType": "Property",
  "name": "Mediterranean Ave",
  "price": 60,
  "colorGroup": "Brown",
  "rentTable": [2, 10, 30, 90, 160, 250],
  "houseCost": 50,
  "hotelCost": 50
}
```

For non-property tiles:
```json
{ "tileType": "Go", "name": "GO" }
{ "tileType": "Tax", "name": "Income Tax", "tax": 200 }
{ "tileType": "Chance", "name": "Chance" }
{ "tileType": "CommunityChest", "name": "Community Chest" }
{ "tileType": "Railroad", "name": "Reading Railroad", "price": 200 }
{ "tileType": "Utility", "name": "Electric Company", "price": 150 }
```

**Serialization note:** Unity's `JsonUtility` does not handle polymorphic types well. The expanded `TileDef` struct will include all possible fields (rentTable, houseCost, hotelCost, colorGroup, tax, price) with unused fields left at default values. The `tileType` field determines which fields are meaningful. String-based enums will be parsed manually since `JsonUtility` serializes enums as integers.

## Section 2: Chance/Community Chest Cards

### Card System

Two separate decks (Chance and Community Chest), each defined in config JSON under top-level arrays `chanceCards` and `communityChestCards`.

### Card Types

| Type | Parameters | Effect | Multiplier Applies? |
|------|-----------|--------|---------------------|
| `GainMoney` | `amount` | Collect a fixed amount | Yes |
| `LoseMoney` | `amount` | Pay a fixed amount | No |
| `GoToTile` | `tileIndex` | Move to a specific tile | N/A |
| `GoToJail` | — | Sent directly to jail | N/A |
| `RepairCosts` | `perHouse`, `perHotel` | Pay per house/hotel owned | No |
| `GainPerProperty` | `amount` | Earn money × number of properties owned | Yes |

### Card JSON Schema

```json
{
  "type": "GainMoney",
  "description": "Bank pays you dividend of $50",
  "amount": 50
}
```

```json
{
  "type": "RepairCosts",
  "description": "Make repairs on all your property",
  "perHouse": 25,
  "perHotel": 100
}
```

### Deck Behavior

- ~10-12 cards per deck (configurable)
- Shuffled at game start using the existing PCG32 RNG
- Drawn in order from top
- Reshuffled when exhausted
- Fully replaces the existing `rng.Next(-50, 101)` random chest/chance logic — that code is removed

### GoToTile Behavior

- When a `GoToTile` card moves the player, they resolve a normal landing on the destination tile (pay rent, get buy option, etc.)
- If movement passes GO (player moves forward past tile 0), they collect GO salary
- `GoToJail` cards do NOT collect GO salary (direct send, same as GoToJail tile)

### Doubles Interaction

- If a player rolls doubles, lands on Chance/CommunityChest, and the card moves them to another tile, they do NOT get an extra roll for the doubles. The card movement ends the turn's movement phase.

## Section 3: Win Conditions & Game Flow

### Win Condition

- **Win:** Own all purchasable properties on the board (properties, railroads, and utilities)
- **Lose:** Cash drops below 0 (bankruptcy) — when any payment would reduce cash below 0, the game ends immediately. The payment is NOT applied (cash stays at its pre-payment value for the results screen).

### Cash Drains (Single-Player Economy)

Since there are no other players to collect rent, the cash drains are:
- **Tax tiles** — fixed deductions
- **LoseMoney cards** — fixed deductions
- **RepairCosts cards** — scales with development (houses/hotels cost money to maintain)
- **Property purchases** — the main cash sink
- **House/hotel purchases** — significant cash investment

This creates a resource management challenge: buy properties to win, but don't over-extend or you'll go bankrupt from taxes and card penalties before completing the set.

### Game Flow Changes

- Game ends immediately on win or loss
- Results screen shows stats: turns taken, total money earned (new accumulator tracked in state), properties owned, houses/hotels built
- Track `totalMoneyEarned` as a running accumulator — incremented on GO payout, GainMoney cards, GainPerProperty cards
- Track `totalTurns` as a running counter

## Section 4: Auctions (Single-Player Adaptation)

### Auto-Discount Mechanism

Traditional auctions don't apply in single-player. Instead:

- Landing on an unowned property: player gets option to buy at full price
- If player declines: property stays unowned, tile index added to a `HashSet<int> declinedProperties`
- Next landing on a declined property: offered at **20% discount** (one-time markdown)
- After the discount landing resolves (regardless of whether player buys or declines), the tile index is removed from `declinedProperties` — subsequent landings revert to full price

### Rationale

Without a decline mechanic, the only strategy is "always buy if affordable." The discount creates meaningful choice: skip now and hope for a cheaper price later, but risk wasting turns.

## Section 5: Board Expansion

### Tile Count

Expand from 12 to **32 tiles** to accommodate all 8 color groups, 4 railroads, 2 utilities, and special tiles.

### Tile Layout

| Index | Tile | Type |
|-------|------|------|
| 0 | GO | Go |
| 1 | Mediterranean Ave | Property (Brown) |
| 2 | Community Chest | CommunityChest |
| 3 | Baltic Ave | Property (Brown) |
| 4 | Income Tax ($200) | Tax |
| 5 | Reading Railroad | Railroad |
| 6 | Oriental Ave | Property (LightBlue) |
| 7 | Chance | Chance |
| 8 | Vermont Ave | Property (LightBlue) |
| 9 | Connecticut Ave | Property (LightBlue) |
| 10 | Jail | Jail |
| 11 | St. Charles Place | Property (Pink) |
| 12 | Electric Company | Utility |
| 13 | States Ave | Property (Pink) |
| 14 | Virginia Ave | Property (Pink) |
| 15 | Pennsylvania Railroad | Railroad |
| 16 | St. James Place | Property (Orange) |
| 17 | Community Chest | CommunityChest |
| 18 | Tennessee Ave | Property (Orange) |
| 19 | New York Ave | Property (Orange) |
| 20 | Go To Jail | GoToJail |
| 21 | Kentucky Ave | Property (Red) |
| 22 | Chance | Chance |
| 23 | Indiana Ave | Property (Red) |
| 24 | Illinois Ave | Property (Red) |
| 25 | B&O Railroad | Railroad |
| 26 | Atlantic Ave | Property (Yellow) |
| 27 | Ventnor Ave | Property (Yellow) |
| 28 | Water Works | Utility |
| 29 | Marvin Gardens | Property (Yellow) |
| 30 | Pacific Ave | Property (Green) |
| 31 | North Carolina Ave | Property (Green) |
| 32 | Community Chest | CommunityChest |
| 33 | Pennsylvania Ave | Property (Green) |
| 34 | Short Line Railroad | Railroad |
| 35 | Chance | Chance |
| 36 | Park Place | Property (Blue) |
| 37 | Luxury Tax ($100) | Tax |
| 38 | Boardwalk | Property (Blue) |

**Total: 39 tiles** (22 properties across 8 color groups, 4 railroads, 2 utilities, 3 Chance, 3 Community Chest, 2 Tax, GO, Jail, Go To Jail)

### Config-Driven

All tile definitions live in `gameconfig.json`. The existing `Helpers.LayoutBoardPositions` already calculates positions dynamically based on tile count, so expanding to 39 tiles requires no layout code changes. The `sideLength` config value may need increasing (from 12.0) to prevent visual overlap with more tiles — this is a config-only change.

## Section 6: Implementation Impact

### Files to Modify

| File | Changes |
|------|---------|
| `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs` | New data structures: ColorGroup enum, expanded TileType enum, TileDef with rentTable/houseCost/hotelCost/colorGroup fields, CardDef struct, card deck arrays, railroad rent tiers, utility rent factors |
| `Assets/Resources/gameconfig.json` | Expanded tile array (39 tiles), card deck definitions, house/hotel costs per property, rent tables, railroad/utility config |
| `Assets/Scripts/MonopolyLite/Core/Main.cs` | New state: `int[] developmentLevels` per property, `List<int>` for each card deck (shuffled indices), `int` draw index per deck, `HashSet<int> declinedProperties`, `bool gameOver`, `bool playerWon`, `int totalTurns`, `int totalMoneyEarned` |
| `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs` | Updated landing logic for all tile types, house building logic, card drawing/resolution, rent calculation (color group full-set check, railroad count-based, utility dice-based), win/lose checks, discount tracking, GoToTile movement with GO-passing check |
| `Assets/Scripts/MonopolyLite/Core/Main.Input.cs` | New UI: build houses button (available when player owns a full color group), buy/decline property prompt with discount display, card reveal text, win/lose results screen with stats |
| `Assets/Scripts/MonopolyLite/Shared/Helpers.cs` | Color-coded tile sprites by color group |
| `Assets/Scripts/MonopolyLite/Core/Recorder.cs` | Updated `GameConfigJson` serialization for new fields, new `CmdType` entries for Buy, Decline, BuildHouse actions |

### Files Unchanged

- `Assets/Scripts/Pool/` — no changes needed
- `Assets/Scripts/Systems/Services/` — no changes needed
- `Assets/Scripts/Systems/Core/Bootstrap.cs` — no changes needed
- `Assets/Scripts/Systems/Runtime/Logger.cs`, `Yield.cs` — no changes needed
- `Assets/Scripts/Extensions.cs` — no changes needed

### New Files

No new files are expected, but if the card deck logic grows complex, a `CardDeck` helper class could be extracted into `GameConfig.cs` or a new file. This is an implementation-time decision.

## Build Order

1. **Color groups & expanded board** — foundational data model, enums, config structure, expanded gameconfig.json with 39 tiles
2. **Houses/Hotels** — depends on color groups for ownership checks
3. **Chance/Community Chest cards** — independent, can reference houses for RepairCosts
4. **Railroads/Utilities** — independent special property types
5. **Decline-to-discount** — needs property system complete
6. **Win conditions & game flow** — needs all financial mechanics in place, adds totalMoneyEarned/totalTurns tracking and results screen
