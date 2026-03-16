# MonopolyGoLite — Gameplay Mechanics Expansion

**Date:** 2026-03-16
**Status:** Approved
**Goal:** Add classic Monopoly mechanics with a casual/mobile-friendly (Monopoly Go-style) twist to the existing single-player game.

## Overview

Expand MonopolyGoLite from a basic 12-tile board with simple buy/rent mechanics into a full-featured Monopoly experience with color groups, houses/hotels, Chance/Community Chest card decks, railroads/utilities, a decline-to-discount mechanism, and target-based win/lose conditions. All features integrate with the existing charge and multiplier systems.

## Design Principles

- **Monopoly Go-style casual:** No even-building rule, unlimited house supply, simplified rules
- **Multiplier stays income-only:** The 1x/2x/3x multiplier affects GO payout and chest rewards only — rent (both paid and received) is fixed based on development level
- **Config-driven:** All tile definitions, rent tables, card decks, and costs live in `gameconfig.json`
- **Single-player:** Target-based goals, no AI opponents, no traditional auctions

## Section 1: Data Model Changes

### Color Groups

- New `ColorGroup` enum: Brown, LightBlue, Pink, Orange, Red, Yellow, Green, Blue
- Each property tile gets:
  - `colorGroup` — which group it belongs to
  - `rentTable` — array of 6 values: [base rent, 1 house, 2 houses, 3 houses, 4 houses, hotel]
  - `houseCost` — cost to build one house on this property
  - `hotelCost` — cost to upgrade from 4 houses to hotel
- `groupSize` per color (Brown = 2, Blue = 2, others = 3) defined at the group level so the game knows when a player owns a full set

### Tile Types

Expand the tile type system to include:
- `Go`, `Property`, `Railroad`, `Utility`, `Chance`, `CommunityChest`, `Tax`, `Jail`, `GoToJail`

### Railroads

- Rent based on how many railroads the player owns:
  - 1 owned = $25
  - 2 owned = $50
  - 3 owned = $100
  - 4 owned = $200

### Utilities

- Rent = dice roll total × multiplier based on utilities owned:
  - 1 owned = 4× dice roll
  - 2 owned = 10× dice roll

### Houses/Hotels

- Each owned property tracks `developmentLevel`: 0 (empty), 1-4 (houses), 5 (hotel)
- No even-building rule — build on any property in a completed color group
- No limited house supply — unlimited (casual mobile style)
- Player must own all properties in a color group before building

## Section 2: Chance/Community Chest Cards

### Card System

Two separate decks (Chance and Community Chest), each defined in config JSON.

### Card Types

| Type | Parameters | Effect |
|------|-----------|--------|
| `GainMoney` | `amount` | Collect a fixed amount |
| `LoseMoney` | `amount` | Pay a fixed amount |
| `GoToTile` | `tileIndex` | Move to a specific tile (collect GO if passing) |
| `GoToJail` | — | Sent directly to jail |
| `RepairCosts` | `perHouse`, `perHotel` | Pay per house/hotel owned |
| `GainPerProperty` | `amount` | Earn money × number of properties owned |

### Deck Behavior

- ~10-12 cards per deck (configurable)
- Shuffled at game start using the existing PCG32 RNG
- Drawn in order from top
- Reshuffled when exhausted
- Replaces the existing random chest/chance amount logic

## Section 3: Win Conditions & Game Flow

### Win Condition

- **Win:** Own all purchasable properties on the board
- **Lose:** Cash drops to $0 or below (bankruptcy)

### Game Flow Changes

- Game ends immediately on win or loss
- Results screen shows stats: turns taken, total money earned, properties owned, houses/hotels built
- No negative cash — bankruptcy triggers on any payment that would drop cash below 0
- Track total turns as a running stat

## Section 4: Auctions (Single-Player Adaptation)

### Auto-Discount Mechanism

Traditional auctions don't apply in single-player. Instead:

- Landing on an unowned property: player gets option to buy at full price
- If player declines: property stays unowned, marked as "declined"
- Next landing on a declined property: offered at **20% discount** (one-time markdown)
- After the discount offer (accepted or declined again), property returns to full price on subsequent landings

### Rationale

Without a decline mechanic, the only strategy is "always buy if affordable." The discount creates meaningful choice: skip now and hope for a cheaper price later, but risk wasting turns.

## Section 5: Board Expansion

### Tile Count

Expand from 12 to **24 tiles** to accommodate all property types and special tiles.

### Tile Layout

| Index | Tile | Type |
|-------|------|------|
| 0 | GO | Go |
| 1 | Mediterranean Ave | Property (Brown) |
| 2 | Community Chest | CommunityChest |
| 3 | Baltic Ave | Property (Brown) |
| 4 | Income Tax | Tax |
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
| 21 | Pacific Ave | Property (Green) |
| 22 | Chance | Chance |
| 23 | Boardwalk | Property (Blue) |

### Config-Driven

All tile definitions live in `gameconfig.json`. The existing `Helpers.LayoutBoardPositions` already calculates positions dynamically based on tile count, so expanding to 24 tiles requires no layout code changes.

## Section 6: Implementation Impact

### Files to Modify

| File | Changes |
|------|---------|
| `GameConfig.cs` | New data structures: ColorGroup enum, TileType enum, rent tables, card deck definitions, development level tracking |
| `gameconfig.json` | Expanded tile array (24 tiles), card deck definitions, house/hotel costs per group, rent tables |
| `Main.cs` | New state: development levels per property, card deck state (shuffled order, draw index), declined properties set, win/lose flag, turn counter |
| `Main.Logic.cs` | Updated landing logic for all tile types, house building logic, card drawing, rent calculation (color group bonus, railroad count, utility dice-based), win/lose checks, discount tracking |
| `Main.Input.cs` | New UI: build houses button, buy/decline property prompt, card reveal display, win/lose results screen |
| `Helpers.cs` | Minor updates for new tile type visuals (color-coded tiles by group) |

### Files Unchanged

- `Pool/` — no changes needed
- `Systems/Services/` — no changes needed
- `Bootstrap.cs` — no changes needed
- `Recorder.cs` — no changes needed
- `Logger.cs`, `Yield.cs`, `Extensions.cs` — no changes needed

### No New Files

All changes fit within the existing file structure.

## Build Order

1. **Color groups** — foundational data model, enum, config structure
2. **Houses/Hotels** — depends on color groups for ownership checks
3. **Chance/Community Chest cards** — independent, can reference houses for RepairCosts
4. **Railroads/Utilities** — independent special property types
5. **Auctions (decline-to-discount)** — needs property system complete
6. **Win conditions** — needs all financial mechanics in place
