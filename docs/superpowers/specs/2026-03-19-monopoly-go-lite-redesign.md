# MonopolyGoLite — Monopoly Go Style Redesign

**Status**: Approved
**Date**: 2026-03-19
**Supersedes**: `2026-03-16-monopoly-gameplay-mechanics-design.md` (classic Monopoly design — fully replaced)
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

## Migration & Deprecation

This spec **fully replaces** the prior design (`2026-03-16-monopoly-gameplay-mechanics-design.md`). The following systems from that spec are discarded: property ownership, rent tables, houses/hotels, railroads-as-property, utilities, decline-to-discount, bankruptcy win/lose conditions.

### File Disposition

| File/Directory | Action | Reason |
|---|---|---|
| `Assets/Scripts/MonopolyLite/Core/Main.cs` | **DELETE** | Classic Monopoly game loop, incompatible |
| `Assets/Scripts/MonopolyLite/Core/Main.Logic.cs` | **DELETE** | Rent/buy/sell/jail logic, fully replaced |
| `Assets/Scripts/MonopolyLite/Core/Main.Input.cs` | **DELETE** | Old UI input handling |
| `Assets/Scripts/MonopolyLite/Core/Bootstrap.cs` | **REWRITE** | Keep entry point pattern, rewire to new systems |
| `Assets/Scripts/MonopolyLite/Core/Recorder.cs` | **DELETE** | Replay system not applicable to new design |
| `Assets/Scripts/MonopolyLite/Shared/GameConfig.cs` | **DELETE** | Classic Monopoly fields, replaced by new config |
| `Assets/Scripts/MonopolyLite/Shared/TileDef.cs` | **REWRITE** | Keep struct concept, new fields for Monopoly Go tiles |
| `Assets/Scripts/MonopolyLite/Shared/TileType.cs` | **REWRITE** | New enum values for redesigned tile types |
| `Assets/Scripts/MonopolyLite/Shared/GameConfigJson.cs` | **REWRITE** | New JSON schema |
| `Assets/Scripts/MonopolyLite/Shared/ConfigLoader.cs` | **KEEP** | JSON loading pattern reusable |
| `Assets/Scripts/MonopolyLite/Shared/Helpers.cs` | **KEEP** | Sprites, Layout, RNG still useful |
| `Assets/Scripts/Systems/` | **KEEP** | Core infrastructure (GameSystem, Services, Logger, Yield) |
| `Assets/Scripts/Systems/Services/` | **EVALUATE** | Keep shells, rewrite internals per new design |
| `Assets/Scripts/Pool/` | **KEEP** | Extend into PoolManager reusable system |
| `Assets/Resources/gameconfig.json` | **REWRITE** | New board-based JSON schema |
| `feature/gameplay-expansion` branch | **ARCHIVE** | Do not merge; keep as reference only |

### Integration with Existing Services Architecture

The existing `Services` pattern (`GameSystem` base class + `Services.cs` singleton manager + UniTask) is **kept as the backbone**:

- New reusable modules (EventBus, EconomyFramework, etc.) are **standalone** — they do NOT extend `GameService`
- The game layer wraps them: e.g., `MonopolyEconomyService : GameService` wraps `IEconomyFramework`
- Existing services disposition:
  - `GameEventService` → replaced by EventBus
  - `InventoryService` → replaced by InventorySystem
  - `ProfileService` → replaced by Firebase Auth + ProfileService wrapper
  - `MonetizationService` → kept, extended with IAP integration
  - `NetworkService` → replaced by Firebase abstraction layer
  - `SocialService` → rewritten for friend system

---

## Authentication & Session Management

### First Launch Flow

1. App opens → **anonymous Firebase Auth** automatically (no sign-in screen)
2. Local save created immediately with anonymous UID
3. Player can play the full core loop without signing in
4. Social features (friends, trading, leaderboards) prompt sign-in

### Account Linking

- Anonymous account → link to Google/Apple at any time
- Linking preserves all progress (same UID)
- Prompt linking at: first friend add, first sticker trade, NW milestone

### Session Lifecycle

- Firebase Auth token auto-refreshes (Firebase SDK handles this)
- On token refresh failure → continue with local cache, retry on next app foreground
- Game state saves locally after every action, syncs to Firebase periodically (every 30s) and on app background/close

### Failure Handling

- Firebase write failure → queue locally, retry with exponential backoff (max 5 retries)
- Extended offline (>24h) → on reconnect, server timestamp wins for leaderboard/events, client state wins for game progress (coins, dice, landmarks)
- Conflict resolution: **last-write-wins** for game state, **server-authoritative** for social events and leaderboards

### Guest Play Scope

- Full core loop (roll, move, build landmarks, board progression) works offline
- Social features (heist, shutdown, friends, events, leaderboards) require connectivity
- Sticker collection works offline, trading requires connectivity

---

## Economy: Dual Resource System

The game has **two core resources**, not one:

### Coins (Soft Currency)
- **Earn from**: Property landing, GO pass, Chance/Community Chest, Bank Heist, events
- **Spend on**: Landmark building/upgrade, Jail exit
- **Inflation**: Scales per board level
- **Cannot be purchased** with real money directly

### Dice (Energy Currency)
- **Earn from**: Time regen, daily login, events, sticker rewards, free dice links
- **Spend on**: Rolling (1 × multiplier per roll)
- **Cap**: Starts at 1000, increases with Net Worth
- **Can be purchased** via IAP (monetization path)

### Premium Currency (Gems) — Future
- Not implemented in Phase 1-4
- Reserved for potential future monetization (cosmetic shop, premium sticker packs)
- The EconomyFramework supports arbitrary currency types for forward-compatibility

### EconomyFramework Coverage
The reusable EconomyFramework (Phase 5) manages **all resource types**: Coins, Dice, and any future currencies. Each currency is defined via config with: earn rules, spend rules, caps, regen rates.

### Board 1 Sample Balance (Istanbul) — To Be Tuned

| Item | Value |
|---|---|
| Starting coins | 0 |
| Starting dice | 100 |
| GO pass bonus | 200 coins |
| Property base reward (Brown) | 50 coins |
| Property base reward (Blue) | 200 coins |
| Tax tile | 150 coins |
| Landmark L1 cost (Brown) | 500 coins |
| Landmark L2 cost (Brown) | 1,200 coins |
| Landmark L3 cost (Brown) | 3,000 coins |
| Landmark L4 cost (Brown) | 7,000 coins |
| Landmark L5 cost (Brown) | 15,000 coins |
| Landmark L1 cost (Blue) | 2,000 coins |
| Landmark L5 cost (Blue) | 50,000 coins |
| Board 2 cost multiplier | 1.8x Board 1 |
| NW per landmark level | 100 / 300 / 600 / 1200 / 2500 |
| Jail exit cost | 50 dice |
| Dice regen rate | 1 dice / 5 min |

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

Dual resource system: **Coins** + **Dice** (see "Economy: Dual Resource System" section above for full details).

- **Coins**: Earned in-game, spent on landmarks. Scales with board progression.
- **Dice**: Energy resource, spent on rolling. Regens over time, purchasable via IAP.
- **Inflation control**: Each new board scales coin earnings and landmark costs proportionally (board level multiplier, ~1.8x per board)

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
    profile/
      displayName: string
      avatarId: string
      createdAt: timestamp
      lastLoginAt: timestamp
      loginStreak: number              // daily login streak (0-7)
      fcmToken: string                 // push notification token
      notificationPrefs: { shutdown: bool, diceFull: bool, events: bool, daily: bool }

    gameState/
      currentBoard: string             // boardId
      coins: number
      dice: number
      diceLastRegenAt: timestamp       // for offline regen calculation
      shields: number                  // 0-3
      netWorth: number
      unlockedMultipliers: number[]    // [1, 2, 5] — which multipliers available
      diceRegenRate: number            // seconds per dice (decreases with NW)
      diceMaxCap: number              // increases with NW

    landmarks/
      {boardId}/
        {colorGroup}: number           // level 0-5

    stats/
      totalRolls: number
      totalCoinsEarned: number
      boardsCompleted: number
      heistsCompleted: number
      shutdownsDealt: number

    stickers/
      {albumId}/
        {stickerId}: number            // count owned (1+ = owned, 2+ = duplicates)
      duplicateStars: number           // total star value of all duplicates
      lastSafeOpenAt: timestamp        // daily safe cooldown

    missions/
      {date}/                          // "2026-03-19"
        {missionId}/
          type: string
          target: number
          progress: number
          completed: bool
      allCompleted: bool               // bonus chest claimed

    friends/
      {friendUserId}/
        status: "pending" | "accepted"
        addedAt: timestamp

    events/
      {eventId}/
        score: number
        lastUpdatedAt: timestamp

boards/                                // bundled in client as JSON, cached in Firebase for hot-update
  {boardId}/
    config/
      theme: string
      tiles: TileDef[]
      landmarkCosts: { [colorGroup]: number[5] }  // cost per level
      landmarkNW: { [colorGroup]: number[5] }     // NW per level
      chanceCards: CardDef[]
      communityChestCards: CardDef[]
      boardMultiplier: number          // cost/reward scale factor

leaderboards/
  weekly/   { [userId]: { netWorthGain: number, displayName: string } }
  allTime/  { [userId]: { totalNetWorth: number, displayName: string } }
  event_{eventId}/ { [userId]: { score: number, displayName: string } }

socialEvents/
  {eventId}/
    type: "heist" | "shutdown"
    fromUser: string
    toUser: string
    result: object                     // heist: { symbol, reward }, shutdown: { shielded, reward }
    timestamp: timestamp

matchmakingPool/                       // for heist/shutdown random player selection
  {boardRange}/                        // "board_1_5", "board_6_10", etc.
    {userId}: { netWorth: number, shields: number, lastActive: timestamp }
```

### 3.4.1 Board Config Source

Board configs are **bundled in the client** as local JSON files (`Assets/Resources/Boards/`). Firebase holds a copy for hot-updates:
- On launch: load local JSON
- Check Firebase for newer version (version field comparison)
- If newer exists: download and cache locally
- This allows adding/rebalancing boards without client updates

### 3.4.2 Matchmaking Strategy

For Bank Heist / Shutdown target selection:
- Players are grouped by board range (boards 1-5, 6-10, etc.)
- Within a range, select random player from `matchmakingPool`
- **Friends first**: If friends exist in the pool, 70% chance to target a friend
- **Small pool fallback**: If <10 players in range, use bot profiles (system-generated fake players with realistic landmarks/shields)
- Firebase query: `matchmakingPool/{boardRange}` ordered by random field, limit 1

### 3.4.3 Security Rules & Validation

- **Server-side validation** (Cloud Functions) for:
  - Coin/dice mutations (prevent client-side tampering)
  - Landmark upgrades (verify sufficient coins before deducting)
  - Leaderboard writes (validate NW calculations)
- **Client-side operations** (direct Firestore):
  - Read own game state
  - Read board configs
  - Read leaderboards, friend lists
- **Rate limiting**: Cloud Functions enforce max 1 roll per second per user
- **Anti-cheat**: Server validates dice count matches expected regen + purchases

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

**Offline Strategy**:
- **Local-first**: All game state writes go to local JSON first, then sync to Firebase
- **Offline-capable**: Core loop (roll, move, build) works fully offline
- **Online-required**: Heist, Shutdown, friend actions, leaderboard, sticker trading, events
- **Sync granularity**: Per-document (entire `gameState`, entire `landmarks/{boardId}`)
- **Conflict resolution**: Last-write-wins for game state; server-authoritative for social/leaderboard
- **Retry**: Exponential backoff (1s, 2s, 4s, 8s, 16s), max 5 retries, then queue for next foreground
- **Offline dice regen**: On reconnect, calculate elapsed time × regen rate, cap at max

### 5.3.1 Monetization Model

- **Primary revenue**: IAP dice packs (small: 100 dice, medium: 500 dice, large: 2000 dice)
- **Secondary**: Ad-rewarded dice (watch ad → 20 dice, cooldown: 1 per 30 min)
- **No pay-to-win**: Coins cannot be purchased; landmarks require coins which require gameplay
- **Cosmetics**: Token skins, board themes (future — not in Phase 1-4)
- The existing `MonetizationService` is kept and extended with Unity IAP + AdMob integration

### 5.4 Audio & VFX

- **SFX**: Dice roll, coin earn, landmark build, level up, heist/shutdown
- **Music**: Board-themed ambient music (looping)
- **VFX**: Coin particles, landmark build animation, shield break effect
- Reusable: AudioManager (pool-based, priority system)

### 5.5 Analytics

- Firebase Analytics integration
- Key events: `roll`, `landmark_build`, `board_complete`, `heist`, `shutdown`, `sticker_collect`
- Funnel tracking: onboard → first_landmark → first_board_complete → retention
