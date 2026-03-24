# Monopoly Case

A lightweight Monopoly Go-inspired mobile board game prototype built with Unity 6.

## Overview

Monopoly Case is a casual board game where players roll dice to move around themed boards, collect landmarks, earn coins, and progress through increasingly challenging content. The game features heist/shutdown social mechanics, daily missions, sticker collection, and a milestone-based progression system.

## Tech Stack

- **Engine:** Unity 6 (6000.3.11f1)
- **Language:** C# (.NET Standard 2.1)
- **UI:** Unity UI + TextMeshPro
- **Testing:** Unity Test Framework (EditMode)
- **Backend:** Firebase (WIP)

## Project Structure

```
Assets/Scripts/
  MonopolyLite/          # Main game module
    Config/              # Board configs, save service, config loaders
    Core/                # Bootstrap, GameController
    Data/                # Data definitions (BoardDef, LandmarkDef, CardDef, etc.)
    Events/              # Game event structs
    Logic/               # Game systems (dice, movement, heist, missions, etc.)
    Shared/              # Shared enums and constants
    State/               # Player, board, progression, mission, sticker state
    View/                # UI views (HUD, panels, board/token renderers)
  Reusable/              # Cross-project reusable modules
    EventBus/            # Generic type-safe pub/sub event bus
    Audio/               # Audio manager interface + stub
  Systems/               # Application-level services
    Core/                # Object pool
    Services/            # Runtime service layer
Assets/Tests/
  EditMode/              # 22 unit test suites
```

## Core Systems

| System | Description |
|--------|-------------|
| **DiceSystem** | Dice rolling with configurable multipliers |
| **MovementSystem** | Board traversal with lap detection |
| **LandmarkSystem** | Property ownership, upgrades, rent collection |
| **CardSystem** | Chance/Community Chest card deck |
| **JailSystem** | Jail entry/exit with dice/card/fee options |
| **HeistSystem** | 3x4 symbol grid heist with tiered rewards |
| **ShutdownSystem** | Shield-based landmark attack/defense |
| **MilestoneSystem** | Net worth-gated unlocks (dice cap, regen, multipliers) |
| **MissionSystem** | Daily mission generation and progress tracking |
| **StickerSystem** | Sticker collection with set/album completion |
| **DailyLoginSystem** | 7-day streak rewards |
| **DiceRegenSystem** | Time-based dice regeneration with offline catchup |
| **BoardProgressionSystem** | Multi-board progression |
| **SaveAdapter** | GameState serialization for local persistence |

## Getting Started

### Requirements

- Unity 6 (6000.3.11f1 or compatible)
- TextMeshPro (included via Unity Package Manager)

### Setup

1. Clone the repository
2. Open the project in Unity Hub
3. Open `Assets/Scenes/MainScene.unity`
4. Press Play

### Running Tests

Open **Window > General > Test Runner** in Unity Editor and run all EditMode tests.

## Architecture Notes

- **Pure C# logic** — all game systems are plain C# classes with no MonoBehaviour dependencies, making them fully unit-testable
- **Config-driven** — board layouts, milestones, daily rewards, and missions are defined as data, not hardcoded
- **Interface-based services** — `ISaveService`, `ITargetProvider`, `IAudioManager` allow swapping implementations without touching game logic
- **EventBus** — decoupled communication between systems via generic `Subscribe<T>` / `Publish<T>`

## License

All rights reserved.
