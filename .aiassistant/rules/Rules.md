---
apply: always
---

PROJECT RULES FOR AI ASSISTANCE

ARCHITECTURE RULES

Rule 1: Pure C# Over MonoBehaviour
- Logic goes in pure C# classes, not MonoBehaviours
- Only use MonoBehaviour for scene entry points, Unity lifecycle needs, or Editor tooling
- Reason: Avoids Unity component discovery issues and memory leaks

Rule 2: Monolith File Organization
- Related classes live in single files (NpcSystem.cs, GridSystem.cs)
- Each monolith has one MonoBehaviour facade at the top
- Supporting classes go below in region blocks
- Reason: AI context window efficiency, keeps related code together

Rule 3: Event-Driven Communication
- Cross-system communication uses GameEventBus, not direct references
- Logic systems publish events, UI systems subscribe
- Never inject UI into logic or logic into UI
- Reason: Prevents circular dependencies, decouples systems

Rule 4: Event Subscription Base Classes
- MonoBehaviour subscribers inherit EventBusSubscriber
- Pure C# subscribers inherit EventBusSubscriberPure
- Always call Dispose() on pure C# subscribers in OnDestroy
- Reason: Automatic cleanup prevents memory leaks

Rule 5: Dependency Injection with VContainer
- Only register top-level MonoBehaviours in BootloaderScope
- Pure C# classes are instantiated directly
- ScriptableObjects use RegisterInstance
- Reason: Reduces DI complexity, avoids injection of non-MonoBehaviours

GRID SYSTEM RULES

Rule 6: Single Grid Entry Point
- All grid queries go through GridSystem.Instance
- Never access HexGrid or CubeGrid directly from other systems
- Reason: Unified API supports both hex and cube grids

Rule 7: Hex Coordinates
- Hex coordinates are axial (q, r) stored as Vector2Int
- World position uses X and Z axes (Y is elevation)
- Cube coordinates are Vector3Int with q + r + s = 0 constraint
- Reason: Consistent coordinate system throughout codebase

Rule 8: Pathfinding
- Pathfinding is pure C# (AStarPathfinding)
- Call GridSystem.DrawPath() not pathfinding directly
- Reason: GridSystem manages the pathfinding lifecycle

NPC SYSTEM RULES

Rule 9: Job System Memory Management
- NativeArray data must be disposed
- Call Dispose() on NpcSimulationSystem and NpcVisualRegistry
- Reason: Prevents native memory leaks

Rule 10: NPC Visibility
- NPC visibility uses Job System with Burst compilation
- NpcVisibilityUpdateRequest triggers visibility updates
- Debug mode available via ToggleNpcDebugRequest
- Reason: Performance for thousands of NPCs

WORLD GENERATION RULES

Rule 11: Pass-Based Pipeline
- Generation passes (IGridGenerationPass) modify tile properties
- Alteration passes (IGridAlterationPass) modify visuals
- Passes receive GridSystem, not individual tile collections
- Reason: Consistent pass interface, supports both grid types

Rule 12: Generation Flow
- Step 1: Publish WorldGenerationStartedEvent
- Step 2: Create tile data and neighbors (GridGenerator)
- Step 3: Run generation passes (elevation, moisture, biomes)
- Step 4: Run alteration passes (rotation, smoothing, variations)
- Step 5: Publish GridInitializationFinishedEvent
- Step 6: Wait for NpcSimulationCompleteEvent
- Step 7: Publish WorldGenerationFinishedEvent
- Reason: Standardized generation sequence for all world types

FILE ORGANIZATION RULES

Rule 13: Monolith File Structure
- Usings at top
- Main MonoBehaviour in its own region block
- Pure C# logic in separate region block
- Data structures in separate region block
- Helpers in separate region block
- Reason: Consistent file navigation for AI and humans

Rule 14: Editor Scripts
- Editor scripts go in separate Editor folders
- Not included in monolith files
- Name convention: ClassNameEditor.cs
- Reason: Editor code excluded from runtime builds

EVENT BUS RULES

Rule 15: Event Naming
- Events are classes ending with Event or Request
- Examples: WorldGenerationFinishedEvent, DrawPathRequest
- Reason: Clear distinction between events (past) and requests (future)

Rule 16: Event Properties
- Event properties are read-only (get only)
- Set via constructor only
- Reason: Events are immutable, represent past occurrences

Rule 17: Publishing Events
- Use Publish() from base class, not GameEventBus.Publish directly
- Source tracking is automatic
- Reason: Debugging information (Source, SourceMember, Timestamp)

DEBUGGING RULES

Rule 18: Event Tracing
- Set EventBusLogLevel.Verbose in Inspector for event logs
- Event.ToString() shows source and timestamp
- Reason: Trace event flow through systems

Rule 19: Grid Debugging
- Use Tools -> Grid -> Show Tile Count menu item
- Reason: Quick grid state inspection

Rule 20: NPC Debugging
- ToggleNpcDebugRequest shows all NPCs bypassing visibility
- Reason: Debug NPC behavior without vision constraints

CODING STANDARDS

Rule 21: Null Safety
- Use null-conditional operator ?. and null-coalescing operator ??
- Check for null before accessing tile properties
- Reason: Prevents NullReferenceException in Editor

Rule 22: Dispose Pattern
- Pure C# classes with event subscriptions implement IDisposable
- Call Dispose() in MonoBehaviour.OnDestroy
- Reason: Clean up event subscriptions, prevent leaks

Rule 23: Constructor over Awake
- Pure C# classes initialize in constructor
- Only MonoBehaviour uses Awake or Start
- Reason: Separation of concerns, testability

WHAT NOT TO DO

Rule 24: No Direct Grid Access
- Avoid: FindObjectOfType<AxialHexGrid>()
- Use: GridSystem.Instance.Hex
- Reason: GridSystem is the single source of truth

Rule 25: No UI in Logic
- Avoid: Inject UiManager into WorldGeneratorCoordinator
- Use: Publish events, let UI subscribe
- Reason: Prevents circular dependencies

Rule 26: No Logic in UI
- Avoid: Put generation logic in UiManager
- Use: Publish WorldGenerationRequest
- Reason: UI should only handle presentation

Rule 27: No Constructor Injection in MonoBehaviours
- Avoid: public MyBehaviour(IService service)
- Use: [Inject] private IService _service
- Reason: Unity doesn't support constructor injection for MonoBehaviours

Rule 28: No Direct NativeArray Access
- Avoid: Modify NativeArray outside NpcSimulationSystem
- Use: Provided methods
- Reason: Job System safety
