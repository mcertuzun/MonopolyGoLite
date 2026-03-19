# MonopolyGoLite — Monopoly Go Style Redesign

**Status**: Approved
**Date**: 2026-03-19
**Approach**: Clean rewrite (keep Systems/ infrastructure + Unity project structure)
**Backend**: Firebase/BaaS
**Orchestration**: agency-agents multi-agent pipeline

---

## Overview

MonopolyGoLite is being redesigned from a classic Monopoly clone to a Monopoly Go-style mobile game. The core shift: from turn-based competitive multiplayer to single-player dice-rolling with async social mechanics, landmark progression, and meta systems.

### Agent Team

| Role | Agent | Responsibility |
|---|---|---|
| Orchestrator | Agents Orchestrator | Coordinate all agents |
| Game Designer | Game Designer | Monopoly Go mechanics, economy balancing, GDD |
| System Architect | Unity Architect | ScriptableObject, data-driven architecture, modular design |
| Backend Architect | Backend Architect | Firebase structure, data model, API design |
| UI Designer | UI Designer | Game interface, component library |
| Developer | Senior Developer | Core implementation |
| Security | Security Engineer | Firebase rules, anti-cheat |
| Code Reviewer | Code Reviewer | Quality control |
| Behavior Engineer | Behavioral Nudge Engine | Engagement, retention mechanics |
| Performance | Performance Benchmarker | Mobile performance, optimization |

### Implementation Phases (Vertical Slices)

1. **Core Loop** — Dice, board movement, landmark building (playable MVP)
2. **Economy & Progression** — Net worth, board-to-board progression, multiplier
3. **Social Mechanics** — Shield, Bank Heist, Shutdown, Firebase integration
4. **Meta Systems** — Sticker album, events, leaderboard
5. **Polish & Reusable Systems** — Extract general systems for cross-game use

---

## Phase 1: Core Loop

### 1.1 Dice System

Dice is a finite resource, not unlimited.

- **Starting dice**: 100
- **Max cap**: 1000 (configurable, increases with Net Worth)
- **Recharge**: Time-based (1 dice / 5 min) + event rewards + daily login bonus
- **Multiplier**: 1x, 2x, 5x, 10x — each roll consumes `1 × multiplier` dice, rewards scale by `multiplier`
- **Single action**: Tap ROLL → dice consumed → token moves → tile event triggers

### 1.2 Board Structure

Classic Monopoly square layout with themed boards.

- Each board is a "city/theme" (Istanbul, Paris, Tokyo, etc.)
- Tile types per board:
  - **Property tiles** (color groups) — earn coins on landing
  - **Railroad tiles** (4) — trigger Bank Heist or Shutdown
  - **Chance** — draw a card
  - **Community Chest** — draw a card
  - **Tax tiles** — coin loss
  - **GO** — pass bonus
  - **Corners** (Jail, Free Parking, Go To Jail) — special effects
- Tile count: configurable (default 24-40)
- Board data loaded from JSON config

### 1.3 Landmark System

Replaces classic houses/hotels. Landmarks are the core progression mechanic.

- Each **color group** has **1 landmark** with **5 upgrade levels**
- Upgrade cost increases per level (level 1: cheap → level 5: expensive)
- **Board completion**: All landmarks reach level 5 → celebration screen → next board unlocks
- Landmarks are visually represented on the board (2D/3D prefabs)
- No property "ownership" — unlike classic Monopoly, landing on property earns coins directly

### 1.4 Tile Landing Logic

| Tile | Effect |
|---|---|
| **Property** | Earn coins: `baseReward × multiplier` |
| **Railroad** | Bank Heist or Shutdown minigame (Phase 3; placeholder: bonus coins) |
| **Chance** | Random card effect (gain/lose coins, move, etc.) |
| **Community Chest** | Random card effect |
| **Tax** | Coin loss (fixed or percentage) |
| **GO** | Pass bonus: `goBonus × multiplier` |
| **Go To Jail** | Go to Jail (wait turns or spend dice to exit) |

### 1.5 Core Game Loop

```
[ROLL] → Dice consumed → Token moves → Tile event → Coins earned/lost
                                                        ↓
                                        Enough coins? → [BUILD LANDMARK]
                                                        ↓
                                        All landmarks max? → [NEXT BOARD]
```

---

## Phase 2: Economy & Progression

### 2.1 Currency System

Single currency: **Coins**.

- **Earn from**: Property landing, GO pass, Chance/Community Chest cards, Bank Heist, event rewards
- **Spend on**: Landmark building/upgrade, Jail exit, special events
- **Inflation control**: Each new board scales earnings and costs proportionally (board level multiplier)

### 2.2 Net Worth System

Net Worth = total value of all landmarks ever built. It only increases, never decreases.

- Each landmark level grants fixed NW points (e.g., level 1: 100 NW, level 5: 2000 NW)
- **Milestone unlocks**:
  - Faster dice recharge rate
  - Higher max dice cap
  - Higher multiplier limit (start: max 5x → unlock 10x, 50x, 100x)
  - Cosmetic rewards (token skins, board themes)

### 2.3 Board Progression

The game is a sequence of themed boards.

- Each board has:
  - Unique theme/city name
  - Own landmark set (~8 landmarks, one per color group)
  - Escalating cost scale
  - Optional unique Chance/Community Chest decks
- **Board completion**: All landmarks level 5 → celebration → next board unlocks
- Board data loaded from JSON config (adding boards = adding JSON files)

### 2.4 Multiplier System

Risk/reward mechanic adjacent to the ROLL button.

- Values: 1x, 2x, 5x, 10x (higher values unlock with Net Worth)
- **Effects**:
  - Dice consumption: `1 × multiplier` per roll
  - Coin earnings: `baseReward × multiplier`
  - Coin losses (tax, jail): `baseCost × multiplier`
  - Landmark costs: **NOT affected** (fixed)
- High multiplier = faster progression but faster dice depletion

### 2.5 Daily & Idle Rewards

- **Daily login bonus**: Escalating dice + coins (7-day cycle, resets on miss)
- **Free dice links**: Configurable promotion system
- **Idle dice regen**: Dice accumulate while offline (up to cap)

---

## Phase 3: Social Mechanics

### 3.1 Shield System

Defensive mechanic against Shutdown attacks.

- Max **3 shields** per player
- **Sources**: Shield tile landing, daily reward, sticker reward
- **Effect**: When Shutdown hits, shield absorbs → attacker earns less coins
- **No shield**: Attacker earns large coin reward, defender gets notification
- **UI**: Shield icon on board (0/3, 1/3, 2/3, 3/3)

### 3.2 Bank Heist (Railroad Minigame A)

Triggered with ~50% chance on railroad tile landing.

- **Mechanic**: 3×4 grid, flip tiles, match 3 symbols
  - **Coin bag** → small reward
  - **Gold bar** → medium reward
  - **Diamond** → large reward
  - **No match** → minimum reward
- **Target**: Random player (from Firebase)
- Coins earned are system-generated (target doesn't lose real coins)

### 3.3 Shutdown (Railroad Minigame B)

Triggered with ~50% chance on railroad tile landing.

- **Mechanic**: Random player's board shown, pick a landmark to "destroy"
  - **Shield present** → small coin reward, landmark protected
  - **No shield** → large coin reward, target gets push notification
- Landmark level does NOT decrease (purely coin-based reward)

### 3.4 Firebase Data Model

```
users/
  {userId}/
    profile: { displayName, avatarId, createdAt }
    gameState: { currentBoard, coins, dice, shields, netWorth }
    landmarks: { [boardId]: { [colorGroup]: level } }
    stats: { totalRolls, totalCoinsEarned, boardsCompleted }

boards/
  {boardId}/
    config: { theme, tiles[], landmarks[], costs[] }

leaderboards/
  weekly/  { [userId]: netWorthGain }
  allTime/ { [userId]: totalNetWorth }

socialEvents/
  {eventId}/
    { type: "heist"|"shutdown", fromUser, toUser, result, timestamp }
```

### 3.5 Friend System

- Firebase Auth (Google/Apple sign-in)
- Add friends via invite link or userId
- Friend list: online status, net worth, current board
- **Heist/Shutdown priority**: Friends first, then random matchmaking

---

## Phase 4: Meta Systems

### 4.1 Sticker Album System

Collectible sticker albums that rotate seasonally.

- **Album**: ~12 sets, each set 9 stickers (~108 stickers per album)
- **Sources**: Board rewards, Chance cards, event rewards, landmark completion bonus
- **Rarity**: 1-star (common) → 5-star (gold/legendary)
- **Set completion reward**: Large dice pack + coins + special token skin
- **Album completion reward**: Mega reward (massive dice + rare cosmetic)
- **Duplicates**: "Stickers for Rewards" — duplicate star points accumulate, daily safe opening (3 tier rewards)
- **Trading**: Friend-to-friend sticker swap (gold stickers locked except during Golden Blitz events)
- **Rotation**: Each album lasts ~3 months, then replaced

### 4.2 Event System

- **Solo Events** (continuously rotating):
  - Dice Roll Tournament — most dice rolled in X time
  - Landmark Rush — fastest landmark builder
  - Coin Collector — most coins earned
- **Partner Events** (with friends):
  - Community Chest — co-op grid reveal with friends, keys increase reward multiplier
- **Seasonal Events**: Special themed boards, exclusive sticker albums, limited-time cosmetics
- Event data loaded from Firebase Remote Config (no client update required)

### 4.3 Leaderboard System

- **Weekly leaderboard**: Net Worth gain, resets Monday
- **Friend leaderboard**: Among friend list only
- **Event leaderboard**: Active during event duration
- **Tier rewards**: Top 1, Top 3, Top 10, Top 50, Top 100 → escalating rewards
- Firebase Firestore + Cloud Functions for computation

### 4.4 Daily Missions

- 3-5 missions per day:
  - "Roll dice 5 times"
  - "Build 1 landmark"
  - "Complete 1 Bank Heist"
  - "Earn 5000 coins total"
- All missions complete → bonus chest (dice + sticker pack)
- Mission definitions from Firebase Remote Config

### 4.5 Notification System

- **Push notifications** (Firebase Cloud Messaging):
  - "Your dice are full!"
  - "Your board got Shutdown!"
  - "New event started!"
  - "Collect your daily reward!"
- Configurable per player in settings

---

## Phase 5: Polish & Reusable Systems

### 5.1 Cross-Game Reusable Systems

Independent modules extracted from this project for use in future games.

| System | Description | Dependencies |
|---|---|---|
| **EventBus** | Publish/Subscribe event system, loosely coupled communication | None — pure C# |
| **ConfigManager** | JSON-based config loading, hot-reload support, validation | None — pure C# |
| **SaveSystem** | Local save (PlayerPrefs + JSON) + Firebase sync | Firebase optional |
| **EconomyFramework** | Currency definition, earn/spend rules, inflation control | EventBus |
| **InventorySystem** | Generic item/sticker/collection management, rarity, stacking | EventBus, SaveSystem |
| **MissionFramework** | Mission definition (JSON), progress tracking, reward distribution | EventBus, EconomyFramework |
| **LeaderboardService** | Firebase leaderboard CRUD, cache, pagination | Firebase |
| **NotificationManager** | Local + push notification, scheduling, cooldown | Firebase Cloud Messaging |
| **PoolManager** | Generic object pooling (extend existing Pool/ infrastructure) | None — Unity only |
| **UIFramework** | Screen management, transitions, popup stack, toast system | EventBus |

### 5.2 Module Structure

Each reusable system follows the same pattern:

```
Assets/Scripts/Systems/{SystemName}/
  ├── I{SystemName}.cs          // Interface
  ├── {SystemName}.cs           // Implementation
  ├── {SystemName}Config.cs     // ScriptableObject config (separate file)
  └── {SystemName}Events.cs     // Related event definitions
```

- Each module has its own **assembly definition** (dependency control)
- Interface-first design → testable, swappable
- Inter-module communication only via EventBus or interfaces

### 5.3 Firebase Abstraction Layer

- `IBackendService` interface → Firebase implementation
- Enables future backend swap (PlayFab, custom server)
- Offline-first: local cache → sync when online
- Retry logic, conflict resolution

### 5.4 Audio & VFX

- **SFX**: Dice roll, coin earn, landmark build, level up, heist/shutdown
- **Music**: Board-themed ambient music (looping)
- **VFX**: Coin particles, landmark build animation, shield break effect
- Reusable: AudioManager (pool-based, priority system)

### 5.5 Analytics

- Firebase Analytics integration
- Key events: `roll`, `landmark_build`, `board_complete`, `heist`, `shutdown`, `sticker_collect`
- Funnel tracking: onboard → first_landmark → first_board_complete → retention
