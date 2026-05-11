# AGENTS.md - AI Assistant Guide for Vanguard Project

This document serves as a comprehensive guide for AI assistants collaborating on the Vanguard Unity project. Its purpose is to provide a clear understanding of the project's architecture, core systems, and development guidelines to facilitate effective and autonomous contributions.

## Project Overview

Vanguard is a Unity game project featuring a hex-based world generation system with NPC simulation, pathfinding, event-driven architecture, and dependency injection using Zenject. The codebase is organized using AI-First Development principles, grouping related code by cohesive systems rather than strict one-class-per-file rules to optimize for AI collaboration.

## Architecture Principles

### AI-First Development - Your Role

As an AI assistant, your primary role is that of an **implementer and curator**. The human developer defines the intent, architecture, and constraints; you handle the implementation details, ensuring consistency and adherence to established patterns.

-   **Cohesive Systems**: Understand that related classes are intentionally grouped within single files (e.g., `NpcSystem.cs` contains the complete NPC system). When making changes, prioritize maintaining this cohesion.
-   **Pure C# over MonoBehaviour**: Logic is primarily in pure C# classes. Only use MonoBehaviours for Unity-specific lifecycle or component needs.
-   **Event-Driven Communication**: All cross-system communication **must** use the `GameEventBus`. Avoid direct references between systems to maintain decoupling.

### File Organization

| File                       | Purpose                                         | Type                                        |
| :------------------------- | :---------------------------------------------- | :------------------------------------------ |
| `GameEventBus.cs`          | All event definitions and the event bus engine  | Mixed (Event definitions + static bus + base classes) |
| `GridSystem.cs`            | Hex and cube grid logic, pathfinding, tile data | Pure C# (except `AxialHexGrid` MonoBehaviour) |
| `NpcSystem.cs`             | NPC simulation, job system, visual registry     | Pure C# (except `NpcManager` MonoBehaviour) |
| `WorldGeneratorCoordinator.cs` | World generation flow, passes, progress tracking | Mixed                                       |
| `MonoInstaller.cs`         | Zenject Dependency Injection configuration      | MonoBehaviour                               |

## Core Systems - Detailed Understanding

### 1. Event System (`GameEventBus.cs`)

**Purpose**: Central communication bus for all decoupled systems.

**Key Components**:
-   `GameEvent`: Base class for all events, includes source tracking (`Source`, `SourceMember`, `Timestamp`).
-   `EventBusSystem`: Static class for `Subscribe`, `Unsubscribe`, and `Publish` operations.
-   `EventBusSubscriber`: Base class for MonoBehaviours requiring event subscription.
-   `EventBusSubscriberPure`: Base class for pure C# classes requiring event subscription.

**Usage Pattern**:

**Subscriber (MonoBehaviour)**:
```csharp
public class MySystem : EventBusSubscriber
{
    private void Start() => Subscribe<WorldGenerationFinishedEvent>(OnWorldReady);
    private void OnWorldReady(WorldGenerationFinishedEvent e) { /* Handle event */ }
}
```

**Subscriber (Pure C#)**:
```csharp
public class MyPureSystem : EventBusSubscriberPure
{
    public MyPureSystem() => Subscribe<WorldGenerationFinishedEvent>(OnWorldReady);
    private void OnWorldReady(WorldGenerationFinishedEvent e) { /* Handle event */ }
}
```

**Publisher**:
```csharp
Publish(new WorldGenerationFinishedEvent());
```

### 2. Grid System (`GridSystem.cs`)

**Purpose**: Manages hex and cube grid data, pathfinding, and tile queries.

**Key Classes**:
-   `AxialHexGrid`: MonoBehaviour entry point (the only MonoBehaviour in this file).
-   `TileData`: Hex tile data (serializable).
-   `AStarPathfinding`: Pathfinding algorithm.
-   `PathVisualizer`: Visual path rendering.

**Key Properties**:
-   `Tiles`: Dictionary of tile data by axial coordinates.
-   `TileData.IsWalkable`: Determines if a tile can be traversed.

**API Examples**:
```csharp
var tile = grid.GetTile(new Vector3Int(q, r, 0));
if (grid.IsWalkable(position)) { /* ... */ }
var tiles = grid.GetTilesInRadius(center, radius);
```

### 3. NPC System (`NpcSystem.cs`)

**Purpose**: Manages NPC spawning, simulation (Unity Job System), and visual representation.

**Key Classes**:
-   `NpcManager`: MonoBehaviour facade (the only MonoBehaviour in this file).
-   `NpcSimulationSystem`: Manages job system and native arrays for NPC logic.
-   `NpcVisualRegistry`: Handles GameObject instantiation and pooling for NPC visuals.
-   `NpcVisibilityTracker`: Calculates visible NPCs for UI updates.
-   `NpcData`: Struct for job system data transfer.

**Dependencies**:
-   Requires `GridSystem` for tile data and world positions.
-   Requires `WorldDecorator` for vision queries.

### 4. World Generation (`WorldGeneratorCoordinator.cs`)

**Purpose**: Orchestrates world generation using a pass-based pipeline.

**Key Components**:
-   `WorldGeneratorCoordinator`: MonoBehaviour orchestrator.
-   `GridGenerator`: Pure C# class for initial tile data and neighbor creation.
-   `GenerationProgressTracker`: Pure C# class for UI progress updates (uses `EventBus`).
-   `IGridGenerationPass` / `IGridAlterationPass`: Interfaces for defining generation steps.

**Generation Flow**:
1.  `WorldGenerationStartedEvent` published.
2.  `CreateDataRoutine` (tile data creation).
3.  `BuildNeighborsRoutine` (neighbor connections).
4.  Execute generation passes (elevation, moisture, biomes).
5.  Execute alteration passes (rotation, smoothing, variations).
6.  `GridInitializationFinishedEvent` published.
7.  Wait for NPC completion.
8.  `WorldGenerationFinishedEvent` published.

### 5. Dependency Injection (`MonoInstaller.cs`)

**Purpose**: Configures Zenject for dependency injection.

**Registered Services**:
-   ScriptableObjects (`GameSettings`, `PlayerSettings`).
-   `AxialHexGrid`, `NpcManager`, `WorldDecorator`, `WorldGeneratorCoordinator`.
-   UI systems (`UiManager`, `UIController`, etc.).
-   `AudioManager`, `VanguardController`.

**Key Pattern**:
-   Only register top-level MonoBehaviours in `MonoInstaller.cs` (or other relevant installers).
-   Pure C# classes are instantiated directly (not injected) and manage their own dependencies or receive them via constructor.
-   Event subscriptions use base classes (`EventBusSubscriber`, `EventBusSubscriberPure`), not DI.

## Event Catalog - For AI Reference

This section lists key events for understanding system interactions.

### World Generation
-   `WorldGenerationStartedEvent`: Generation begins.
-   `WorldGenerationFinishedEvent`: Generation complete.
-   `WorldCleanupEvent`: Reset or cancellation signal.
-   `GridStructuralDataReadyEvent`: Basic tile data ready (NPCs can start).
-   `GridInitializationFinishedEvent`: Full grid ready (decorators can start).
-   `GenerationProgressInitializedEvent`: Progress bar setup.
-   `GenerationProgressUpdatedEvent`: Progress bar update.

### Player Movement
-   `PlayerMovedEvent`: Player changes tile.
-   `PlayerDestinationReachedEvent`: Player arrives at target.
-   `DrawPathRequest`: Request path calculation.
-   `PathCreatedEvent`: Path calculated.
-   `PathClearedEvent`: Path hidden.

### NPC System
-   `NpcSimulationCompleteEvent`: NPC spawning complete.
-   `NpcVisibilityUpdateRequest`: Request visibility update.
-   `NpcVisibleAgentsCountChangedEvent`: UI count update.

### UI and Flow
-   `GameStateChangedEvent`: Game state changes (Initializing, CharacterSelection, Playing).
-   `WorldGenerationRequest`: User requests new world.
-   `RespawnRequest`: User requests respawn.
-   `ResetWorldRequest`: Full reset.
-   `CharacterSelectedRequest`: Character chosen.
-   `InputLockRequest`: Lock or unlock input.

### Settings
-   `VolumeChangedRequest`
-   `GridRadiusChangedRequest`
-   `PopulationSizeChangedRequest`
-   `VisionRadiusChangedEvent`
-   `FpsToggleRequest`

### Input
-   `MouseScrollEvent`
-   `TilePointerDownEvent`
-   `TilePointerUpEvent`
-   `TileDragEvent`

## Development Guidelines - How to Contribute

### Adding a New System

When implementing a new feature or system:
1.  **Start with Pure C#**: Develop the core logic as pure C# classes.
2.  **Event Subscription**: Use `EventBusSubscriberPure` for event handling in pure C# classes.
3.  **MonoBehaviour Facade (if needed)**: If Unity lifecycle methods or components are required, create a minimal MonoBehaviour facade.
4.  **DI Registration**: Only register the MonoBehaviour facade (if created) in `MonoInstaller.cs` (or other relevant Zenject installers).
5.  **Communicate via Events**: Publish events for other systems to react to, maintaining decoupling.

### Event Bus Rules

-   **Always Unsubscribe**: Ensure proper unsubscription to prevent memory leaks. Base classes (`EventBusSubscriber`, `EventBusSubscriberPure`) handle this automatically via `Dispose` or `OnDestroy`.
-   **Specific Event Types**: Use distinct event types; avoid generic string-based events.
-   **Immutable Events**: Events should be immutable (properties set only via constructor).
-   **Source Tracking**: `Publish()` from base classes automatically tracks the event source.

### Grid System Rules

-   **API Usage**: Never modify the `Tiles` dictionary directly. Always use provided `GridSystem` methods.
-   **Pathfinding**: Pathfinding is pure C#. Call `GridSystem.DrawPath()` to visualize paths.
-   **Hex Coordinates**: Use axial coordinates (q, r) as `Vector2Int` for pointy-top orientation.
-   **Cube Coordinates**: Use `Vector3Int` with the `q + r + s = 0` constraint.

### NPC System Rules

-   **Job System Data**: Be mindful of `NativeArray` lifecycle when working with NPC data in the Job System.
-   **Dispose Criticality**: Call `Dispose()` on pure C# classes that manage unmanaged resources.
-   **Visual Prefab**: NPC visual prefabs require an Animator with an "IsMoving" parameter.

## Debugging Tips - For AI Troubleshooting

### Event Tracing
-   **Source Tracking**: Event source tracking helps identify which class or method published an event.
-   **Verbose Logging**: Set `EventBusLogLevel.Verbose` in the Inspector for detailed event logs.

### Grid Debugging
-   **Tile Count**: Use `Tools -> Grid -> Show Tile Count` menu item to verify grid generation.
-   **Path Visualization**: `PathVisualizer` shows calculated paths in the scene.

### NPC Debugging
-   **Toggle Visibility**: `ToggleNpcDebugRequest` toggles the visibility of all NPCs.
-   **UI Updates**: Monitor `NpcVisibleAgentsCountChangedEvent` for UI-related NPC count updates.

## Common Patterns - Quick Reference

### Pure C# Class with Events
```csharp
public class MyPureClass : EventBusSubscriberPure
{
    public MyPureClass()
    {
        Subscribe<SomeEvent>(OnSomeEvent);
    }
    
    private void OnSomeEvent(SomeEvent e)
    {
        // Handle event logic
    }
}
```

### MonoBehaviour with Events
```csharp
public class MyBehaviour : EventBusSubscriber
{
    private void Start()
    {
        Subscribe<SomeEvent>(OnSomeEvent);
    }
    // OnDestroy is handled by EventBusSubscriber base class
}
```

### Publishing Events
-   **In MonoBehaviour**: `Publish(new MyEvent());`
-   **In Pure C#**: `Publish(new MyEvent());`

### Grid Query
```csharp
var tile = GridSystem.Instance.GetTile(new Vector3Int(q, r, 0));
if (tile?.IsWalkable == true)
{
    var neighbors = grid.GetTilesInRadius(position, radius);
}
```

## File Dependencies - System Relationships

```
GameEventBus.cs (no dependencies - foundational)
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
MonoInstaller.cs (depends on everything - DI configuration)
```

## Key References - External Documentation

-   **Zenject Documentation**: `https://github.com/svermeulen/Zenject`
-   **Unity Job System**: For understanding high-performance NPC simulation.
-   **Hex Grid Math**: Axial coordinates (q,r) with pointy-top orientation.
-   **A\* Pathfinding**: Standard algorithm for tile-based movement.

---

Last updated: 2025-01-06
Maintained for AI assistant collaboration
