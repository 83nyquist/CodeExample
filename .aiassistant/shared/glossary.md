# Project Glossary

## Unity Terminology

### MonoBehaviour
Base class for all Unity scripts. Provides lifecycle methods like Awake, Start, Update.

### GameObject
Container for components. Every object in a Unity scene is a GameObject.

### Transform
Component that stores position, rotation, and scale of a GameObject.

### Prefab
Template of a GameObject that can be instantiated multiple times.

### ScriptableObject
Data container that exists outside of scenes. Useful for configuration, events, and shared data.

### Coroutine
Method that can pause execution and resume over multiple frames using yield return.

### Component
Piece of functionality attached to a GameObject. Examples: Transform, Renderer, Collider.

### Scene
Container for GameObjects. Represents a level or menu.

### Namespace
C# feature for organizing code. Example: YourProject.Scripts.Gameplay.

### SerializedField
Unity attribute that exposes private fields to the inspector.

### RequireComponent
Unity attribute that automatically adds required components when this component is added.

## Architecture Patterns

### Composition over Inheritance
Prefer adding small component behaviors to GameObjects rather than deep class hierarchies.

### Event-Driven Architecture
Components communicate via events/events channels rather than direct references.

### Object Pooling
Reusing objects instead of destroying and recreating them. Reduces GC allocation.

### Singleton
Class that ensures only one instance exists. Use sparingly - makes testing difficult.

### MVC (Model-View-Controller)
Separates data (Model), UI (View), and logic (Controller). Not native to Unity but can be implemented.

### Dependency Injection
Providing dependencies to a class rather than having it create them. Enables testing.

## Project-Specific Terms

### [Add your project-specific terms here]

#### Term 1: [Definition]

#### Term 2: [Definition]

#### Term 3: [Definition]

## Acronyms

### ADR
Architecture Decision Record - Document explaining a significant architectural decision.

### GC
Garbage Collection - Automatic memory management that can cause performance spikes.

### PR
Pull Request - Request to merge code changes into the main branch.

### CI/CD
Continuous Integration / Continuous Deployment - Automated testing and deployment pipeline.

### LOD
Level of Detail - Rendering optimization using lower-detail models at distance.

### DOD
Data-Oriented Design - Performance-focused design using structs and arrays.

### ECS
Entity Component System - Unity's high-performance data-oriented architecture system.

### UI
User Interface - Canvas-based elements for player interaction.

### HUD
Heads-Up Display - UI elements showing game state (health, ammo, score).

### VFX
Visual Effects - Particles, shaders, and other graphical effects.

### SFX
Sound Effects - Audio for gameplay actions.

## Commonly Used Terms

### Serialization
Process of converting data to a format that can be saved or transmitted.

### Deserialization
Process of converting serialized data back to live objects.

### Hot Path
Code that executes frequently (like Update method). Requires optimization.

### Technical Debt
Future work implied by quick-and-dirty implementation choices.

### Regression
Bug that appears in previously working functionality after a change.

### Edge Case
Unusual or extreme scenario that may not be handled by normal logic.

### Sandbox
Isolated environment for testing without affecting production.

## Roles

### Planner
Agent responsible for feature breakdown and technical specifications.

### Coder
Agent responsible for writing and implementing code.

### Reviewer
Agent responsible for code quality and design doc alignment.