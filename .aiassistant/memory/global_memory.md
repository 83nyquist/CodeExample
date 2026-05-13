# Global Memory — Architectural Decisions

## EventBus System
- **Decision:** Static singleton event bus with `IEventBus` interface
- **Rationale:** Centralized pub/sub enables loose coupling between all systems without a DI service locator. Static methods provide convenience; `IEventBus` enables testability.
- **Pattern:** All event subscribers use `EventBusSubscriber` or `EventBusSubscriberPure` base classes which handle automatic subscription lifecycle (subscribe on enable, unsubscribe on disable).
- **Consequence:** Event dispatch is synchronous. Heavy listeners block the publisher. All events are reference types deriving from `GameEvent`.

## Zenject Dependency Injection
- **Decision:** Use Zenject (Extenject) for all DI
- **Rationale:** Constructor injection without manual wiring. `[Inject]` attributes on MonoBehaviours. Installers organized by system.
- **Pattern:** `MonoInstaller` at application root. Each major system has its own installer or is injected directly in scene context.
- **Consequence:** Systems rarely call `GetComponent<>()` on other objects; dependencies are injected.

## Hexagonal Grid
- **Decision:** Axial coordinate system using `Vector2Int` (q = column, r = row)
- **Rationale:** Simplifies neighbor calculations (6 neighbors, constant-time). No floating-point drift.
- **Pattern:** Tiles stored in `Dictionary<Vector2Int, TileData>`. Grid generation is multi-frame batched to avoid frame spikes.
- **Consequence:** All grid operations are O(1) lookups by coordinate.

## Multi-Frame Batching
- **Decision:** Heavy generation work (grid, decorations, NPC init) is split across frames with a `_maxMsPerFrame` budget
- **Rationale:** Prevents multi-second freezes during world generation
- **Pattern:** IEnumerators yield after exceeding time budget. Progress reported via EventBus (`GenerationProgressUpdatedEvent`).
- **Consequence:** Generation code is more complex (state machine in coroutines) but frame times stay under budget.

## NPC Simulation
- **Decision:** NPC logic runs in Unity Jobs (IJobParallelFor) for performance
- **Rationale:** Thousands of agents need per-frame updates. Jobs run on worker threads.
- **Pattern:** Agent state in blittable structs (`BlittableTileData`), job reads/writes NativeArrays.
- **Consequence:** NPC data layout is constrained by job requirements (no managed references in job structs).

## UI Architecture
- **Decision:** Hybrid UIToolkit (main HUD) + UGUI (loading screen, character profiles)
- **Rationale:** UIToolkit for complex dynamic layouts (labels, action buttons). UGUI for simpler overlays. Both driven by EventBus.
- **Pattern:** Controllers subscribe to relevant events and update UI. No UI code references gameplay logic directly.

## World Generation Pipeline
1. `GenerateWorldRequest` published (user or auto)
2. `GameFlowCoordinator` sets state to Loading, publishes init lock
3. `WorldGeneratorCoordinator` begins: generates grid, places decorations, spawns NPCs
4. Each subsystem publishes completion events (`GridInitializationFinishedEvent`, `WorldVisualsReadyEvent`, `NpcSimulationCompleteEvent`)
5. On all complete, `WorldGenerationFinishedEvent` published, state transitions to Playing
