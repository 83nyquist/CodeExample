# ADR-001: EventBus SOLID Refactor

## Status
Accepted — 2026-05-13

## Context
The EventBus system was a single monolithic file (`EventBusSystem.cs`, 531 lines) containing:
- Enum definitions
- Abstract base event class (`GameEvent`)
- 30+ concrete event classes
- Static event engine (`EventBusSystem`)
- Two subscriber base classes (`EventBusSubscriber`, `EventBusSubscriberPure`)

This violated nearly every SOLID principle:
- **S** — File had 6+ responsibilities (enum, base event, engine, all events, two subscriber bases)
- **O** — No interface to allow alternative implementations
- **I** — No interface segregation
- **D** — Base classes depended directly on the static `EventBusSystem` concrete class

Additionally, all 30+ event types lived in a single namespace (`Systems.EventBus`) alongside the engine and base classes, making it hard to navigate.

## Decision
Split the monolith into 13 files organized by concern under `Systems.EventBus/`:

```
BaseClasses/
  EventBusSubscriber.cs
  EventBusSubscriberPure.cs
Components/
  EventBusSystem.cs          (static engine + IEventBus singleton)
Enums/
  EventBusLogLevel.cs
Events/
  GameEvent.cs               (abstract base)
  GridEvents.cs
  NpcEvents.cs
  WorldGenerationEvents.cs
  MovementEvents.cs          (player movement + path drawing combined)
  UiFlowEvents.cs
  SettingsEvents.cs
Interfaces/
  IEventBus.cs               (new)
```

### Key Design Changes
1. **IEventBus interface** — `Subscribe<T>`, `Unsubscribe<T>`, `Publish<T>` for dependency inversion
2. **EventBusSystem singleton** — static methods delegate to `Instance` (implements `IEventBus`)
3. **Namespace-per-folder** — `Systems.EventBus.Events`, `.BaseClasses`, `.Components`, `.Enums`, `.Interfaces`
4. **Movement events merged** — `PlayerMovedEvent`, `PlayerDestinationReachedEvent`, `DrawPathRequest`, `PathCreatedEvent`, `PathClearedEvent` combined into `MovementEvents.cs`

### What Stayed the Same
- All class names, method signatures, and behavior
- Consumer code only needed `using` statement updates (no logic changes)
- Static `EventBusSystem.Subscribe/Unsubscribe/Publish` still work (delegate to instance)

## Consequences

### Positive
- Each file has a single, clear responsibility
- New event types can be added by creating new files in `Events/` — no modification to existing code
- `IEventBus` enables testing with mock event buses
- Base classes can accept `IEventBus` in constructor for DI (falls back to singleton)
- Consumer files only import the namespaces they actually need
- 21 consumer files updated, 0 logic changes required

### Negative
- More files to navigate (13 vs 1)
- Consumers now need multiple `using` statements instead of one
- Event bus internals (dictionary, locks) are still in the `EventBusEngine` private class

### Neutral
- `.csproj` had to be updated with new file references
- Unity .meta files will be regenerated on next import

## Alternatives Considered

### Keep monolithic file
Rejected — violated SRP, hard to navigate, no testability

### Move to ScriptableObject event channels
Rejected — would require rewriting all consumers. Current EventBus is appropriate for this project's scale.

### True DI (inject IEventBus everywhere)
Deferred — base classes currently default to `EventBusSystem.Instance`. Constructor injection support is ready if needed.
