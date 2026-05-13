# Game Architecture

## Overview
Procedurally generated hexagonal tile world with NPC simulation, fog-of-war, and player movement. All systems communicate via EventBus for loose coupling. Zenject handles dependency injection.

## System Dependency Graph
```
GameFlowCoordinator (state machine)
  │
  ├── WorldGeneratorCoordinator
  │     ├── GridGenerator (hex tiles, multi-frame)
  │     ├── DecorationScheduler (tile decorators, multi-frame)
  │     └── NpcManager (agent spawning, jobs-based)
  │
  ├── VanguardController (player entity)
  │     └── VanguardMover (path movement)
  │
  ├── InputSystem (input locking, tile interaction)
  │
  └── UiManager
        ├── UIController (UIToolkit main HUD)
        ├── UIActionsController (action buttons)
        ├── UiLabels (dynamic labels)
        ├── LoadingPanelController (UGUI loading bar)
        └── CharacterProfile (UGUI)
```

All systems communicate through **EventBus** (`Systems.EventBus`). No system holds a direct reference to another system's runtime state.

---

## EventBus Architecture

### Flow
```
Publisher → Publish<T>(event) → EventBusSystem → all Subscribers of T
```

### Key Classes
| Class | Responsibility |
|-------|---------------|
| `IEventBus` | Interface for subscribe/unsubscribe/publish |
| `EventBusSystem` | Static singleton engine. Static methods delegate to `Instance` (implements `IEventBus`) |
| `GameEvent` | Abstract base for all events (source tracking, timestamp) |
| `EventBusSubscriber` | MonoBehaviour base — auto subscribe on OnEnable, cleanup on OnDisable/OnDestroy |
| `EventBusSubscriberPure` | Pure C# base — implements IDisposable for manual cleanup |

### Event Categories
| Category | Events |
|----------|--------|
| **Grid** | `GridClearedEvent`, `GridInitializationFinishedEvent`, `VisibleTilesCountChangedEvent` |
| **NPC** | `NpcSimulationCompleteEvent`, `NpcVisibleAgentsCountChangedEvent` |
| **WorldGen** | `WorldGenerationStartedEvent`, `WorldGenerationFinishedEvent`, `WorldVisualsReadyEvent`, `GenerationProgressInitializedEvent`, `GenerationProgressUpdatedEvent`, `ReportWorkProgressRequest`, `WorldCleanupEvent` |
| **Movement** | `PlayerMovedEvent`, `PlayerDestinationReachedEvent`, `DrawPathRequest`, `PathCreatedEvent`, `PathClearedEvent` |
| **UI/Flow** | `GameStateChangedEvent`, `GenerateWorldRequest`, `RespawnRequest`, `ResetWorldRequest`, `CommanderSelectedRequest`, `PlayerMoveRequest`, `ClearPathRequest`, `GameFlowInitLockRequest/Unlock`, `InputLockRequest/Unlock` |
| **Settings** | `VolumeChangedRequest`, `GridRadiusChangedRequest`, `PopulationSizeChangedRequest`, `VisionRadiusChangedRequest`, `FpsToggleRequest`, `CharacterAnimationEventsChangedEvent` |

---

## Grid System

### Hex Coordinate System
- **Axial coordinates** stored as `Vector2Int` (q = column, r = row)
- 6 neighbor directions: (1,0), (1,-1), (0,-1), (-1,0), (-1,1), (0,1)
- Tiles stored in `Dictionary<Vector2Int, TileData>` — O(1) lookups

### Generation Pipeline
1. `GridGenerator` creates tile data in multi-frame batches
2. `IGridGenerationPass` implementations apply alterations (biome, height, features)
3. Each batch reports progress via `ReportWorkProgressRequest`
4. On completion, publishes `GridInitializationFinishedEvent` with tile dictionary

### Pathfinding
- **AStarPathfinding** — A* on hexagonal grid, subscribes to `DrawPathRequest`, `PlayerMovedEvent`, etc.
- **PathVisualizer** — renders path as GameObjects, subscribes to `PathCreatedEvent`, `PathClearedEvent`

---

## NPC System

### Architecture
- **NpcManager** — MonoBehaviour, subscribes to world generation events
- **NpcJob** — blittable struct for IJobParallelFor compatibility
- **BlittableTileData** — stripped-down TileData for job access
- **NpcVisualRegistry** — manages GameObject representations
- **GenerationProgressTracker** — (pure C#) tracks init progress, reports via EventBus

### Lifecycle
1. Subscribes to `WorldGenerationStartedEvent` → cleans up previous simulation
2. Subscribes to `GridInitializationFinishedEvent` → spawns NPCs using tile data
3. Publishes `NpcSimulationCompleteEvent` when done
4. Publishes `NpcVisibleAgentsCountChangedEvent` when visible count changes

---

## Vanguard (Player)

### Classes
- **VanguardController** — player lifecycle, subscribes to 8+ events (world state, movement, path, UI)
- **VanguardMover** — handles tile-by-tile movement along a path

### Movement Flow
1. User input → `PlayerMoveRequest` published
2. A* calculates path → `PathCreatedEvent` with `List<TileData>`
3. `VanguardMover` moves along path tile by tile
4. Each tile arrival → `PlayerMovedEvent`
5. Final tile → `PlayerDestinationReachedEvent`

---

## UI Architecture

### Layers
| Layer | Technology | Controller | Purpose |
|-------|-----------|------------|---------|
| Main HUD | UIToolkit | `UIController` | Visibility/state |
| Actions | UIToolkit | `UIActionsController` | World gen, reset buttons |
| Labels | UIToolkit | `UiLabels` | Tile info, NPC count |
| Loading | UGUI | `LoadingPanelController` | Progress bar |
| Profile | UGUI | `CharacterProfile` | Character display |

### UI Flow
- All UI controllers extend `EventBusSubscriber`
- `GameStateChangedEvent` drives visibility (Loading vs Playing)
- `GenerationProgressUpdatedEvent` drives loading bar
- User actions publish events (never call gameplay directly)

---

## Zenject Bindings
Project-wide bindings registered in `MonoInstaller`. Key bindings:
```
PlayerSettings — FromResources
GameSettings — FromResources
IDecorationScheduler → DecorationScheduler (AsSingle)
WorldGeneratorCoordinator (AsSingle)
GameFlowCoordinator (AsSingle)
VanguardController (AsSingle)
All UI controllers — FromComponentsInNewPrefab
```
