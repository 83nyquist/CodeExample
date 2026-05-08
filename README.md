# CodeExample - Hex Grid Terrain Generation

A Unity demonstration project featuring a top-down axial hex grid system supporting 3,000,000 tiles with Perlin noise terrain generation and 10,000 independent agents with independent roaming.

<img src="images/map.png" alt="Hex grid terrain map" width="300" height="300">

## Technical Highlights
- **Multithreading using the Job System** - NPC system supporting a large number of individual agents (Systems/NPC)
- **Hex Grid System** - Complete top-down hex grid implementation with cell navigation (Systems/Grid)
- **A-Star Pathfinding** - Optimal route calculation across hex grid (Systems/Grid)
- **Perlin Noise Generation** - Procedural terrain height and feature mapping (Systems/Grid)
- **Coroutine & Object Pooling** - Optimized generation of tile decoration for smooth performance (Systems/Decorator)
- **Dependency Injection** - VContainer for service management and decoupling (Systems/DependencyInjection/BootloaderScope)
- **Event-Driven Architecture** - Centralized `GameEventBus` with source tracking for debugging (Systems/EventBus)
- **UI Toolkit and UGUI Integration** - Hybrid UI system combining modern Toolkit with legacy UGUI for maximum compatibility and flexibility (UserInterface)

## Why This Code Is Organized This Way

**TL;DR:** I'm trading SOLID file boundaries for monoliths because AI context windows matter more than traditional organization for my workflow. It smells, but it works.

When I started designing this project, I did what I always do: built everything in one place. Then came refactoring time. I had tens of small classes scattered around. And I thought: how would I pass this to an AI as context?

I could:
- Give an agent unrestricted access to my codebase (hard pass - Unity component references break easily)
- Spend hours designing a RAG system to search my codebase
- Wait for 12M token context models
- Or just keep the entire system in one monolith with a facade

**I kept the monolith.** Every bone in my body says this is wrong. But I've realized: I'm not the primary reader anymore. My AI assistant is. When I need to fix something, I'll master my IDE's search and structure tab.

**Context is key. Trust but verify.** This isn't about abandoning standards - SOLID principles still apply inside each monolith. It's about recognizing that file boundaries are for humans, not machines. And in an AI-First workflow, context windows win.

*For small teams and AI-assisted development, this works great. For large teams? Invest in RAG or pay for 12M tokens.*

## AI-First Development

**This project uses a Monolith-by-System architecture** - related classes live in single files, not scattered across dozens of small files, contrary to what we usually associate with well-written and manageable code. This is a conscious, defensible trade-off for AI-assisted development. The principles remain intact. Only the file boundaries have changed. In this demo I have only refactored the `EventBusSystem` as an example, but am transitioning more in this direction on my actual project.

### The Facade Pattern Inside
The `EventBusSystem` monolith follows the **Facade pattern** - a single static class (`EventBusSystem`) acts as the public API, while supporting classes live in the same file but remain internally focused.
- **`EventBusSystem` (static)** - Public facade providing `Subscribe`, `Unsubscribe`, and `Publish` methods
- **`GameEvent`** - Base class for all events with source tracking (Source, SourceMember, Timestamp)
- **`EventBusSubscriber`** - Base class for MonoBehaviours needing automatic event cleanup
- **`EventBusSubscriberPure`** - Base class for pure C# classes implementing `IDisposable`

All event definitions (`GridClearedEvent`, `NpcSimulationCompleteEvent`, `WorldGenerationStartedEvent`, etc.) are also contained in this single file. The facade owns the public API. Everything else is internal infrastructure - ready for AI context, easy to reference, and self-contained.

### SOLID Inside the Monolith
Even within this single-file structure, SOLID principles are actively respected and visible:

| Principle | How It's Demonstrated in EventBusSystem |
|-----------|------------------------------------------|
| **S**ingle Responsibility | `EventBusSystem` handles ONLY subscription and publication; `GameEvent` handles ONLY event data and source tracking |
| **O**pen/Closed | New event types can be added without modifying the core `EventBusSystem` engine |
| **L**iskov Substitution | Any `GameEvent` subclass can be used wherever `GameEvent` is expected |
| **I**nterface Segregation | `EventBusSubscriber` and `EventBusSubscriberPure` are separate base classes for different use cases (MonoBehaviour vs pure C#) |
| **D**ependency Inversion | Event publishers depend on `GameEvent` abstraction, not concrete event types; subscribers depend on event type abstractions |

The principles remain intact. Only the file boundaries have changed.

### The Curator Role
To me, AI-First Development means the engineer acts as a **curator**—defining intent, architecture, and constraints—while AI agents handle implementation. The curator owns decisions, not keystrokes.
This project embraces AI-First Development - a pragmatic approach to code organization optimized for human-AI collaboration.

### The Traditional Approach (Why We Deviate)

Traditional software engineering teaches "one class per file" for good reasons:
- Minimize merge conflicts in team environments
- Improve source control diff readability
- Enable parallel development

However, these benefits come at a cost: **fragmented context**.

### The AI-First Reality

When working with AI coding assistants, the limiting factor is **context window**. A system spread across 11 files requires the AI to read 11 separate files to understand the complete system. This is inefficient, token-expensive, and increases the chance of missed connections.

### When This Works Well

| Good for                        | Not for                                   |
| :------------------------------ | :---------------------------------------- |
| Small teams (1-3 developers)    | Large teams (10+ developers)              |
| AI-assisted development         | Open source with many contributors        |
| Rapid iteration and prototyping | Projects requiring strict merge workflows |
| Personal/solo projects          | Enterprise with rigid coding standards    |

### The Bottom Line

> *"Break the rules when the rules don't serve you."*

This repository prioritizes **AI collaboration efficiency** over traditional file organization. The code is structured for machines (and AI) to read, not just for humans to navigate by filename.

If you're a human reviewing this code: use your IDE's search. If you're AI: enjoy the full context.

## How to Run the Build

### Windows Build
1. Download `WindowsBuild-Windows.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive to a folder
3. Double-click `CodeExample.exe` to launch the game

### Linux Build
1. Download `LinuxBuild-Linux.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive
3. Make the executable runnable:
   ```bash
chmod +x CodeExample.x86_64
   ```
4. Run the executable:
   ```bash
./CodeExample.x86_64
   ```

   

