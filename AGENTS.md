# AGENTS.md - AI Assistant Guide for Vanguard Project

## Project Overview

This is a Unity game project featuring a hex-based world generation system with NPC simulation, pathfinding, event-driven architecture, and dependency injection using VContainer. The codebase is organized using AI-First Development principles - code is grouped by cohesive systems rather than strict one-class-per-file rules to optimize for AI collaboration.

## Architecture Principles

### AI-First Development

- Engineer as Curator: The developer defines intent, architecture, and constraints while AI agents handle implementation
- Cohesive Systems: Related classes live together in single files (e.g., complete NPC system in NpcSystem.cs)
- Pure C# over MonoBehaviour: Logic is extracted to pure C# classes where possible, only using MonoBehaviours for Unity-specific functionality
- Event-Driven Communication: Cross-system communication uses GameEventBus, not direct references

### File Organization

| File | Purpose | Type |
|------|---------|------|
| GameEventBus.cs | All event definitions and event bus engine | Mixed (Event definitions + static bus + base classes) |
| GridSystem.cs | Hex and cube grid logic, pathfinding, tile data | Pure C# (except GridSystem MonoBehaviour) |
| NpcSystem.cs | NPC simulation, job system, visual registry | Pure C# (except NpcManager MonoBehaviour) |
| WorldGeneratorCoordinator.cs | World generation flow, passes, progress tracking | Mixed |
| BootloaderScope.cs | VContainer DI configuration | MonoBehaviour |

## Core Systems

### 1. Event System (GameEventBus.cs)

Purpose: Central communication bus for all decoupled systems.

Key Components:
- GameEvent - Base class with source tracking (Source, SourceMember, Timestamp)
- GameEventBus - Static class with Subscribe/Unsubscribe/Publish
- EventBusSubscriber - Base class for MonoBehaviours needing event subscription
- EventBusSubscriberPure - Base class for pure C# classes needing event subscription

Usage Pattern:

Subscriber (MonoBehaviour):
public class MySystem : EventBusSubscriber
{
    private void Start() => Subscribe<WorldGenerationFinishedEvent>(OnWorldReady);
    private void OnWorldReady(WorldGenerationFinishedEvent e) { }
}

Subscriber (Pure C#):
public class MyPureSystem : EventBusSubscriberPure
{
    public MyPureSystem() => Subscribe<WorldGenerationFinishedEvent>(OnWorldReady);
}

Publisher:
Publish(new WorldGenerationFinishedEvent());

### 2. Grid System (GridSystem.cs)

Purpose: Manages hex and cube grid data, pathfinding, and tile queries.

Key Classes:
- GridSystem - MonoBehaviour entry point (only MonoBehaviour in file)
- HexGrid - Pure C# hex grid logic
- CubeGrid - Pure C# cube grid logic
- TileData - Hex tile data (serializable)
- AStarPathfinding - Pathfinding algorithm (pure C#)
- PathVisualizer - Visual path rendering (pure C#)

Key Properties:
- GridSystem.Instance - Singleton access
- HexGrid.Tiles - Dictionary of tile data by axial coordinates
- TileData.IsWalkable - Determines if tile can be traversed

API Examples:
var grid = GridSystem.Instance;
var tile = grid.GetTile(new Vector3Int(q, r, 0));
if (grid.IsWalkable(position)) { }
var tiles = grid.GetTilesInRadius(center, radius);

### 3. NPC System (NpcSystem.cs)

Purpose: Manages NPC spawning, simulation (Job System), and visual representation.

Key Classes:
- NpcManager - MonoBehaviour facade (only MonoBehaviour in file)
- NpcSimulationSystem - Manages job system and native arrays
- NpcVisualRegistry - Handles GameObject instantiation and pooling
- NpcVisibilityTracker - Calculates visible NPCs for UI
- NpcData - Struct for job system data transfer

Dependencies:
- Requires GridSystem for tile data and world positions
- Requires WorldDecorator for vision queries

### 4. World Generation (WorldGeneratorCoordinator.cs)

Purpose: Orchestrates world generation with pass-based pipeline.

Key Components:
- WorldGeneratorCoordinator - MonoBehaviour orchestrator
- GridGenerator - Pure C# for tile data and neighbor creation
- GenerationProgressTracker - Pure C# for UI progress updates (uses EventBus)
- IGridGenerationPass / IGridAlterationPass - Pass interfaces

Generation Flow:
1. Publish WorldGenerationStartedEvent
2. Create tile data (CreateDataRoutine)
3. Build neighbor connections (BuildNeighborsRoutine)
4. Execute generation passes (elevation, moisture, biomes)
5. Execute alteration passes (rotation, smoothing, variations)
6. Publish GridInitializationFinishedEvent
7. Wait for NPC completion
8. Publish WorldGenerationFinishedEvent

### 5. Dependency Injection (BootloaderScope.cs)

Purpose: Configures VContainer for dependency injection.

Registered Services:
- ScriptableObjects (GameSettings, PlayerSettings)
- GridSystem, NpcSystem, WorldDecorator, WorldGeneratorCoordinator
- UI systems (UiManager, UIController, etc.)
- AudioManager, VanguardController

Key Pattern:
- Only register top-level MonoBehaviours
- Pure C# classes are instantiated directly (not injected)
- Event subscriptions use base classes, not DI

## Event Catalog

### World Generation
- WorldGenerationStartedEvent - Generation begins
- WorldGenerationFinishedEvent - Generation complete
- WorldCleanupEvent - Reset or cancellation signal
- GridStructuralDataReadyEvent - Basic tile data ready (NPCs can start)
- GridInitializationFinishedEvent - Full grid ready (decorators can start)
- GenerationProgressInitializedEvent - Progress bar setup
- GenerationProgressUpdatedEvent - Progress bar update

### Player Movement
- PlayerMovedEvent - Player changes tile
- PlayerDestinationReachedEvent - Player arrives at target
- DrawPathRequest - Request path calculation
- PathCreatedEvent - Path calculated
- PathClearedEvent - Path hidden

### NPC System
- NpcSimulationCompleteEvent - NPC spawning complete
- NpcVisibilityUpdateRequest - Request visibility update
- NpcVisibleAgentsCountChangedEvent - UI count update

### UI and Flow
- GameStateChangedEvent - Game state changes (Initializing, CharacterSelection, Playing)
- WorldGenerationRequest - User requests new world
- RespawnRequest - User requests respawn
- ResetWorldRequest - Full reset
- CharacterSelectedRequest - Character chosen
- InputLockRequest - Lock or unlock input

### Settings
- VolumeChangedRequest
- GridRadiusChangedRequest
- PopulationSizeChangedRequest
- VisionRadiusChangedRequest
- FpsToggleRequest

### Input
- MouseScrollEvent
- TilePointerDownEvent
- TilePointerUpEvent
- TileDragEvent

## Development Guidelines

### Adding a New System

1. Create pure C# logic first (no MonoBehaviour)
2. Use EventBusSubscriberPure for event subscriptions
3. Create MonoBehaviour facade if Unity lifecycle needed
4. Register only facade in BootloaderScope
5. Publish events for other systems to react

### Event Bus Rules

- Always unsubscribe (base classes handle this with Dispose or OnDestroy)
- Use specific event types (no string-based events)
- Keep events immutable (properties only via constructor)
- Source tracking is automatic when using Publish() from base classes

### Grid System Rules

- Never modify Tiles dictionary directly - use provided methods
- Pathfinding is pure C# - call GridSystem.DrawPath()
- Hex coordinates are axial (q, r) stored as Vector2Int
- Cube coordinates are stored as Vector3Int with q+r+s=0 constraint

### NPC System Rules

- NPC data uses Job System - be aware of NativeArray lifecycle
- Dispose is critical - call Dispose() on pure C# classes
- Visual prefab requires Animator with "IsMoving" parameter

## Debugging Tips

### Event Tracing
- Event source tracking shows which class or method published each event
- Set EventBusLogLevel.Verbose in Inspector for detailed logs

### Grid Debugging
- Use Tools -> Grid -> Show Tile Count menu item
- PathVisualizer shows calculated paths in scene

### NPC Debugging
- ToggleNpcDebugRequest toggles visibility of all NPCs
- Check NpcVisibleAgentsCountChangedEvent for UI updates

## Common Patterns

### Pure C# Class with Events
public class MyPureClass : EventBusSubscriberPure
{
    public MyPureClass()
    {
        Subscribe<SomeEvent>(OnSomeEvent);
    }
    
    private void OnSomeEvent(SomeEvent e)
    {
        // Handle event
    }
}

### MonoBehaviour with Events
public class MyBehaviour : EventBusSubscriber
{
    private void Start()
    {
        Subscribe<SomeEvent>(OnSomeEvent);
    }
}

### Publishing Events
In MonoBehaviour: Publish(new MyEvent());
In pure C#: Publish(new MyEvent());

### Grid Query
var tile = GridSystem.Instance.GetTile(new Vector3Int(q, r, 0));
if (tile?.IsWalkable == true)
{
    var neighbors = grid.GetTilesInRadius(position, radius);
}

## File Dependencies

GameEventBus.cs (no dependencies - base)
    |
    v
GridSystem.cs (depends on GameEventBus, Core.Components)
    |
    v
NpcSystem.cs (depends on GridSystem, GameEventBus)
    |
    v
WorldGeneratorCoordinator.cs (depends on GridSystem, GameEventBus)
    |
    v
BootloaderScope.cs (depends on everything - DI configuration)

## Key References

- VContainer Documentation: https://vcontainer.hadashikick.jp/
- Unity Job System: For NPC simulation performance
- Hex Grid Math: Axial coordinates (q,r) with pointy-top orientation
- A* Pathfinding: Standard algorithm with tile-based cost

---

Last updated: 2025-01-06
Maintained for AI assistant collaboration