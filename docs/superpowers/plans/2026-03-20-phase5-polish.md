# Phase 5: Polish & Reusable Systems — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract reusable infrastructure: type-safe EventBus for decoupled communication, game event definitions, and AudioManager interface — following the spec's module pattern with interface-first design.

**Architecture:** `EventBus` is a generic type-safe pub/sub system replacing the string-based `GameEventService`. Game events are lightweight structs published through the bus. `AudioManager` provides interface + stub for future audio integration. All systems follow `Assets/Scripts/Systems/{Name}/` folder convention with their own assembly definitions.

**Tech Stack:** Unity 6.0.3, C# 9.0, NUnit

**Spec:** `docs/superpowers/specs/2026-03-19-monopoly-go-lite-redesign.md` (Phase 5: sections 5.1–5.4)

---

## File Structure

### New Files (Create)

```
Assets/
├── Scripts/
│   └── Systems/
│       ├── EventBus/
│       │   ├── IEventBus.cs                 # Interface: Subscribe<T>, Unsubscribe<T>, Publish<T>, Clear
│       │   └── EventBus.cs                  # Implementation: Dictionary<Type, Delegate> based
│       ├── Audio/
│       │   ├── IAudioManager.cs             # Interface: PlaySFX, PlayMusic, StopMusic, volumes
│       │   └── AudioManager.cs              # Stub implementation (logs, no actual audio)
│       └── Events/
│           └── GameEvents.cs                # All game event structs (Roll, Coin, Landmark, Heist, etc.)
└── Tests/
    └── EditMode/
        └── EventBusTests.cs
```

### Existing Files (Modify)

| File | Action | Changes |
|---|---|---|
| `Assets/Scripts/MonopolyLite/Core/GameController.cs` | MODIFY | Add EventBus field, publish events alongside existing delegates |

---

## Task 1: EventBus Interface + Implementation + Tests

**Files:**
- Create: `Assets/Scripts/Systems/EventBus/IEventBus.cs`
- Create: `Assets/Scripts/Systems/EventBus/EventBus.cs`
- Create: `Assets/Tests/EditMode/EventBusTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using System;
using NUnit.Framework;

namespace MonopolyLite.Tests
{
    public class EventBusTests
    {
        struct TestEvent { public int Value; }
        struct OtherEvent { public string Name; }

        IEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
        }

        [Test]
        public void Publish_InvokesSubscriber()
        {
            int received = 0;
            _bus.Subscribe<TestEvent>(e => received = e.Value);

            _bus.Publish(new TestEvent { Value = 42 });

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Publish_InvokesMultipleSubscribers()
        {
            int count = 0;
            _bus.Subscribe<TestEvent>(_ => count++);
            _bus.Subscribe<TestEvent>(_ => count++);

            _bus.Publish(new TestEvent { Value = 1 });

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Publish_DoesNotInvokeOtherEventSubscribers()
        {
            bool otherCalled = false;
            _bus.Subscribe<OtherEvent>(_ => otherCalled = true);

            _bus.Publish(new TestEvent { Value = 1 });

            Assert.IsFalse(otherCalled);
        }

        [Test]
        public void Unsubscribe_RemovesHandler()
        {
            int count = 0;
            Action<TestEvent> handler = _ => count++;
            _bus.Subscribe(handler);

            _bus.Publish(new TestEvent());
            Assert.AreEqual(1, count);

            _bus.Unsubscribe(handler);
            _bus.Publish(new TestEvent());
            Assert.AreEqual(1, count); // not incremented
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent { Value = 99 }));
        }

        [Test]
        public void Clear_RemovesAllSubscribers()
        {
            int count = 0;
            _bus.Subscribe<TestEvent>(_ => count++);
            _bus.Subscribe<OtherEvent>(_ => count++);

            _bus.Clear();
            _bus.Publish(new TestEvent());
            _bus.Publish(new OtherEvent());

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Subscribe_SameHandlerTwice_CalledTwice()
        {
            int count = 0;
            Action<TestEvent> handler = _ => count++;
            _bus.Subscribe(handler);
            _bus.Subscribe(handler);

            _bus.Publish(new TestEvent());

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Publish_PassesCorrectData()
        {
            string received = null;
            _bus.Subscribe<OtherEvent>(e => received = e.Name);

            _bus.Publish(new OtherEvent { Name = "hello" });

            Assert.AreEqual("hello", received);
        }
    }
}
```

- [ ] **Step 2: Create IEventBus.cs**

```csharp
using System;

public interface IEventBus
{
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
    void Publish<T>(T evt);
    void Clear();
}
```

- [ ] **Step 3: Implement EventBus.cs**

```csharp
using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    readonly Dictionary<Type, Delegate> _handlers = new();

    public void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var existing))
            _handlers[type] = Delegate.Combine(existing, handler);
        else
            _handlers[type] = handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var existing)) return;
        var result = Delegate.Remove(existing, handler);
        if (result == null) _handlers.Remove(type);
        else _handlers[type] = result;
    }

    public void Publish<T>(T evt)
    {
        if (_handlers.TryGetValue(typeof(T), out var handler))
            ((Action<T>)handler)?.Invoke(evt);
    }

    public void Clear()
    {
        _handlers.Clear();
    }
}
```

> **Note:** EventBus and IEventBus are in the global namespace intentionally — they are cross-game reusable and should not be in `MonopolyLite` namespace. The `Assets/Scripts/Systems/EventBus/` folder may need its own assembly definition if we want strict dependency control, but for now it compiles within the default assembly.

- [ ] **Step 4: Run tests, create .meta files, commit**

```bash
git add Assets/Scripts/Systems/EventBus/IEventBus.cs Assets/Scripts/Systems/EventBus/EventBus.cs Assets/Tests/EditMode/EventBusTests.cs
git commit -m "feat(phase5): add type-safe EventBus with generic Subscribe/Publish/Unsubscribe"
```

---

## Task 2: Game Event Definitions

**Files:**
- Create: `Assets/Scripts/Systems/Events/GameEvents.cs`

- [ ] **Step 1: Create GameEvents.cs**

All game events as lightweight structs. These replace the need for custom delegate signatures on GameController.

```csharp
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Events
{
    // Core loop
    public struct RollEvent
    {
        public int Die1;
        public int Die2;
        public int Total;
        public bool IsDoubles;
        public bool PassedGo;
    }

    public struct TileEvent
    {
        public TileResolveType Type;
        public int Amount;
        public CardDef? DrawnCard;
    }

    public struct LandmarkUpgradedEvent
    {
        public ColorGroup Group;
        public int NewLevel;
    }

    public struct BoardCompleteEvent { }

    public struct BoardTransitionEvent
    {
        public string NewBoardId;
        public string Theme;
    }

    // Economy
    public struct CoinChangeEvent
    {
        public int Amount;
        public bool IsGain;
        public string Source; // "property", "tax", "heist", "shutdown", "mission"
    }

    public struct DiceRegenEvent
    {
        public int Amount;
    }

    public struct MilestoneEvent
    {
        public int MilestoneIndex;
    }

    public struct DailyRewardEvent
    {
        public int Day;
        public int Coins;
        public int Dice;
    }

    // Social
    public struct HeistEvent
    {
        public HeistResult Result;
        public string TargetName;
    }

    public struct ShutdownStartEvent
    {
        public TargetProfile Target;
    }

    public struct ShutdownResolveEvent
    {
        public ShutdownResult Result;
    }

    // Meta
    public struct MissionCompleteEvent
    {
        public string Description;
        public int CoinReward;
        public int DiceReward;
    }

    public struct AllMissionsCompleteEvent { }

    public struct StickerGrantEvent
    {
        public int StickerId;
        public string StickerName;
    }

    // Save
    public struct GameSavedEvent { }
    public struct GameLoadedEvent { }
}
```

- [ ] **Step 2: Create .meta file, commit**

```bash
git add Assets/Scripts/Systems/Events/GameEvents.cs
git commit -m "feat(phase5): add game event struct definitions for EventBus"
```

---

## Task 3: AudioManager Interface + Stub

**Files:**
- Create: `Assets/Scripts/Systems/Audio/IAudioManager.cs`
- Create: `Assets/Scripts/Systems/Audio/AudioManager.cs`

- [ ] **Step 1: Create IAudioManager.cs**

```csharp
public interface IAudioManager
{
    void PlaySFX(string sfxId);
    void PlayMusic(string musicId);
    void StopMusic();
    void SetMusicVolume(float volume);
    void SetSFXVolume(float volume);
}
```

- [ ] **Step 2: Create AudioManager.cs (stub)**

```csharp
using UnityEngine;

public class AudioManager : MonoBehaviour, IAudioManager
{
    float _musicVolume = 1f;
    float _sfxVolume = 1f;

    public void PlaySFX(string sfxId)
    {
        Debug.Log($"[Audio] SFX: {sfxId}");
    }

    public void PlayMusic(string musicId)
    {
        Debug.Log($"[Audio] Music: {musicId}");
    }

    public void StopMusic()
    {
        Debug.Log("[Audio] Music stopped");
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }
}
```

- [ ] **Step 3: Create .meta files, commit**

```bash
git add Assets/Scripts/Systems/Audio/IAudioManager.cs Assets/Scripts/Systems/Audio/AudioManager.cs
git commit -m "feat(phase5): add IAudioManager interface and stub AudioManager"
```

---

## Task 4: GameController EventBus Integration

**Files:**
- Modify: `Assets/Scripts/MonopolyLite/Core/GameController.cs`

- [ ] **Step 1: Read current GameController.cs fully**

- [ ] **Step 2: Add EventBus field and initialization**

Add field after existing fields:

```csharp
public IEventBus Bus { get; private set; }
```

In `Initialize()`, at the very beginning (before anything else):

```csharp
Bus = new EventBus();
```

- [ ] **Step 3: Add EventBus publishing alongside existing events**

In `DoRoll()` non-jail path, after `OnRollComplete?.Invoke(roll, move);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.RollEvent
{
    Die1 = roll.Die1, Die2 = roll.Die2, Total = roll.Total,
    IsDoubles = roll.IsDoubles, PassedGo = move.PassedGo
});
```

After `OnTileResolved?.Invoke(resolve);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.TileEvent
{
    Type = resolve.Type, Amount = resolve.Amount, DrawnCard = resolve.DrawnCard
});
```

In `DoUpgradeLandmark()`, after `OnLandmarkUpgraded?.Invoke(group, level);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.LandmarkUpgradedEvent { Group = group, NewLevel = level });
```

After `OnBoardComplete?.Invoke();`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.BoardCompleteEvent());
```

In `HandleRailroadEvent()` heist branch, after `OnHeistResolved?.Invoke(result, target);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.HeistEvent { Result = result, TargetName = target.displayName });
```

In `HandleRailroadEvent()` shutdown branch, after `OnShutdownStarted?.Invoke(target);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.ShutdownStartEvent { Target = target });
```

In `DoShutdownAttack()`, after `OnShutdownResolved?.Invoke(result);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.ShutdownResolveEvent { Result = result });
```

In `TryTransitionToNextBoard()`, after `OnBoardTransition?.Invoke(nextBoardId);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.BoardTransitionEvent
{
    NewBoardId = nextBoardId, Theme = BoardDef.theme
});
```

In `AutoSave()`, after `_saveService.Save(data);`, add:

```csharp
Bus.Publish(new MonopolyLite.Events.GameSavedEvent());
```

- [ ] **Step 4: Verify compilation, commit**

```bash
git add Assets/Scripts/MonopolyLite/Core/GameController.cs
git commit -m "feat(phase5): wire EventBus into GameController, publish all game events"
```

---

## Summary

| Task | Files | Tests | Description |
|---|---|---|---|
| 1 | 2 new | 8 | EventBus — type-safe generic pub/sub |
| 2 | 1 new | — | Game event struct definitions (16 events) |
| 3 | 2 new | — | AudioManager interface + stub |
| 4 | 1 mod | — | GameController EventBus publishing |

**Total: 5 new files, 1 modified file, 8 new unit tests**
