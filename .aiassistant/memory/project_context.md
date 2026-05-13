# Project Context

## Engine & Environment
- **Engine:** Unity 6000.4.6f1 LTS
- **Language:** C# (.NET Standard 2.1, LangVersion 9)
- **IDE:** JetBrains Rider 2024+
- **DI Framework:** Zenject (Extenject) — used for all dependency injection
- **Version Control:** Git (GitHub)

## Project Type
Procedurally generated hexagonal tile-based world with NPC simulation, fog-of-war, and player movement.

## Key Systems

### EventBus (`Systems.EventBus`)
- Central pub/sub event system for decoupled communication
- Static `EventBusSystem` engine with `IEventBus` interface
- Two base classes: `EventBusSubscriber` (MonoBehaviour) and `EventBusSubscriberPure` (pure C#)
- Both base classes provide `Subscribe<T>()`, `Unsubscribe<T>()`, `Publish<T>()` with automatic subscription tracking and cleanup
- All events derive from `GameEvent` (provides source tracking, timestamp)
- ~30 event types grouped by domain (Grid, NPC, WorldGen, Movement, UI/Flow, Settings)
- Editor tools: `EventBusDebugTools` for bulk log-level changes

### Grid System (`Systems.Grid`)
- **AxialHexGrid** — coordinate system using `Vector2Int` (axial q,r)
- **GridGenerator** — multi-frame batched tile generation with per-frame budget (`_maxMsPerFrame`)
- **Generation passes** (`IGridGenerationPass`) — abstraction for alteration pipeline
- **TileData** — per-tile data (biome, height, walkability)
- **Pathfinding** — A* implementation (`AStarPathfinding`), `PathVisualizer` for rendering
- World generation coordinated by `WorldGeneratorCoordinator`

### NPC System (`Systems.NonPlayerCharacters`)
- **NpcManager** — spawns, initializes, and manages NPC agent lifecycle
- Jobs-based simulation using Unity Jobs system (`NpcJob` structs)
- **GenerationProgressTracker** — tracks NPC initialization progress via EventBus
- **NpcVisualRegistry** — manages visual representation of NPCs
- Agent data stored in `BlittableTileData` structs for job compatibility

### Decoration System (`Systems.Decoration`)
- **WorldDecorator** — places `TileDecorator` instances on grid tiles at generation time
- **DecorationScheduler** — multi-frame decorator spawning with per-frame budget
- **DecoratorFactory** — creates decorator instances by tile biome/type
- **IDecorationScheduler** / **IDecorationFactory** interfaces for testability

### Vanguard (Player) (`Vanguard`)
- **VanguardController** — player entity lifecycle, responds to world state events
- **VanguardMover** — handles player movement along paths, publishes movement events
- Movement is event-driven: receives `PlayerMoveRequest`, publishes `PlayerDestinationReachedEvent`

### UI System (`UserInterface`)
- Hybrid approach: **UIToolkit** (main HUD — `UIController`, `UIActionsController`, `UiLabels`) and **UGUI** (character profiles, loading panel)
- **UiManager** — top-level UI coordinator
- All UI is EventBus-driven (listens for game state changes, generation progress, etc.)

### Input (`Input`)
- **InputSystem** — Unity Input System wrapper with input locking
- Publishes `InputLockRequest` / `InputUnlockRequest` for modal blocking

### Coordination (`Coordinators`)
- **GameFlowCoordinator** — top-level game state machine (Initializing → Loading → Playing)
- **WorldGeneratorCoordinator** — orchestrates world generation pipeline
- **SettingsSyncHandler** — syncs UI settings changes with ScriptableObject data

## Data Layer
- **ScriptableObjects:** `PlayerSettings`, `GameSettings`, character definitions (`CharacterItem`, `CharacterAnimationEvents`)
- Layout stored at `Assets/Data/` via `Resources.Load` + Zenject bindings

## Code Organization
```
Assets/Scripts/
├── Character/           — CharacterItem, animation events
├── Coordinators/        — GameFlow, WorldGen, Settings coordinators
├── Core/                — Attributes, Collections, Components, Editor
├── Input/               — InputSystem
├── Systems/
│   ├── Decoration/      — WorldDecorator, DecorationScheduler
│   ├── EventBus/        — Event engine, events, base classes, interfaces
│   ├── Grid/            — Hex grid, generation passes, pathfinding
│   └── NonPlayerCharacters/ — NPC manager, jobs, components
├── UserInterface/       — UIToolkit + UGUI controllers
└── Vanguard/            — Player controller, mover
```
