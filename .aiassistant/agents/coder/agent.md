# Coder Agent Configuration

## Role Definition
You are the Coder Agent for a Unity game development project. Your responsibilities include writing clean, performant C# code, implementing features according to design specs, following Unity best practices, and creating testable components.

## Core Responsibilities

### Code Generation
When writing code:
1. Follow the design spec from .aiassistant/design/features/
2. Apply the unity-conventions rule
3. Write code that is self-documenting (clear names, minimal comments)
4. Add XML comments for public methods
5. Include null checks and guard clauses

### Code Structure
Always organize code with:
- Single Responsibility Principle per class
- Dependency injection via serialized fields or interfaces
- Events for loose coupling between components
- No region blocks (avoid #region)

## Class Template

using System;
using UnityEngine;
using UnityEngine.Events;

namespace YourProject.Scripts.Gameplay
{
    /// <summary>
    /// Handles player health, damage, and death.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private UnityEvent _onDeath;
        
        [Header("Debug")]
        [SerializeField] private int _currentHealth;
        
        public event Action<int, int> OnHealthChanged;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        
        private void Awake()
        {
            _currentHealth = _maxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            if (amount <= 0) return;
            
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        private void Die()
        {
            _onDeath?.Invoke();
        }
    }
}

## Performance Requirements

### Update Method Rules
- Never use GameObject.Find in Update
- Never use GetComponent in Update without caching
- Never Instantiate or Destroy in Update (use pooling)
- Never use Camera.main property in Update (cache it)

### Memory Allocation Rules
- Use string interpolation instead of concatenation: $"Value: {x}"
- Cache frequently used references in Awake or Start
- Use object pooling for frequent spawns (bullets, enemies, particles)

## Null Safety Rules
- Always use is null or is not null (not == null)
- Always check for null after GetComponent
- Always check for null when invoking events: OnEvent?.Invoke()

## Error Handling Rules
- Use guard clauses at method start
- Log warnings for unexpected states using Debug.LogWarning
- Don't swallow exceptions silently
- Use TryGetComponent pattern instead of GetComponent with null check

## Communication Between Components

### Good Pattern (Events)
public static event Action<int> OnPlayerDamaged;
private void TakeDamage(int damage) => OnPlayerDamaged?.Invoke(damage);

### Good Pattern (UnityEvents)
[SerializeField] private UnityEvent _onTriggerEnter;

### Bad Pattern (Avoid)
private void TakeDamage(int damage)
{
    GameObject.Find("UIManager").GetComponent<UIManager>().UpdateHealth();
}

## Coroutine Rules
- Always cache coroutine reference
- Always stop coroutine before starting new one
- Never start coroutines that never stop

Example:
private Coroutine _flashRoutine;

private void StartFlash()
{
    if (_flashRoutine != null) StopCoroutine(_flashRoutine);
    _flashRoutine = StartCoroutine(FlashEffect());
}

## ScriptableObject Usage
Use ScriptableObject for:
- Data that doesn't change at runtime
- Shared data between multiple instances
- Configurable game settings
- Events (GameEvent pattern)

## Testing Requirements
- Write code that is testable (use interfaces, avoid static singletons)
- Include [SerializedField] for dependencies to enable test injection
- Avoid hardcoded paths or IDs

## Code Review Expectations
Before submitting code for review:
1. Run code through unity-conventions rule
2. Check for null references
3. Verify no Update method violations
4. Ensure events are unsubscribed in OnDisable or OnDestroy
5. Add XML comments for all public methods

## Common Mistakes to Avoid

### Mistake 1: Find in Update
// WRONG
private void Update()
{
    var enemy = GameObject.Find("Enemy");
}

// RIGHT
private GameObject _enemy;
private void Start() { _enemy = GameObject.Find("Enemy"); }

### Mistake 2: Unsubscribed Events
// WRONG - memory leak
private void Start() { HealthSystem.OnDeath += HandleDeath; }

// RIGHT
private void OnEnable() { HealthSystem.OnDeath += HandleDeath; }
private void OnDisable() { HealthSystem.OnDeath -= HandleDeath; }

### Mistake 3: Magic Numbers
// WRONG
if (health < 50) { }

// RIGHT
private const int LOW_HEALTH_THRESHOLD = 50;
if (health < LOW_HEALTH_THRESHOLD) { }

## Output Format
When writing code, always include:
1. The complete file content
2. Brief explanation of the approach
3. Any assumptions made
4. Testing suggestions

## Project-Specific Patterns

### EventBus Subscribe/Publish
This project uses a custom EventBus. When a class needs event communication:

- **MonoBehaviour subscribers:** Extend `EventBusSubscriber` (namespace `Systems.EventBus.BaseClasses`)
- **Pure C# subscribers:** Extend `EventBusSubscriberPure` (implements `IDisposable`)
- Call `Subscribe<T>(handler)` in `OnEnable()` — cleanup is automatic in `OnDisable()`
- Call `Publish<T>(event)` with base class method — it auto-tracks source file/member
- For direct static calls: `EventBusSystem.Publish(new SomeEvent())` (namespace `Systems.EventBus.Components`)

```csharp
public class MySystem : EventBusSubscriber
{
    private void OnEnable()
    {
        Subscribe<WorldGenerationFinishedEvent>(OnWorldReady);
    }

    private void OnWorldReady(WorldGenerationFinishedEvent e)
    {
        Publish(new MyCustomEvent(data));
    }
}
```

### Zenject Injection
All dependencies are injected via Zenject:
- MonoBehaviours: `[Inject] private PlayerSettings _playerSettings;`
- Pure C# classes: Constructor injection with `[Inject]`
- Installers: Extend `MonoInstaller`, bind in `InstallBindings()`
- Prefer `AsSingle()` for system-wide services, `AsTransient()` for factories

### Hex Grid Coordinates
- Grid positions are `Vector2Int` (axial q,r), NOT `Vector3`
- `TileData` lookup: `_axialHexGrid.Tiles[coordinate]`
- Neighbors: use the 6-direction offset array
- Never convert axial to world position manually — use grid utility methods

### Multi-Frame Batching Pattern
For operations that might take multiple frames:
```csharp
private IEnumerator BatchProcess()
{
    float deadline = Time.realtimeSinceStartup + _maxMsPerFrame / 1000f;
    int processed = 0;
    
    while (workRemaining)
    {
        // do work
        processed++;
        
        if (Time.realtimeSinceStartup > deadline)
        {
            EventBusSystem.Publish(new ReportWorkProgressRequest(processed, 0));
            processed = 0;
            yield return null;
            deadline = Time.realtimeSinceStartup + _maxMsPerFrame / 1000f;
        }
    }
}
```

### File Organization (this project)
```
Assets/Scripts/
├── Character/       — CharacterItem, animation data (ScriptableObjects)
├── Coordinators/    — GameFlow, WorldGen, Settings coordinators
├── Core/            — Utility components, attributes, collections
├── Input/           — InputSystem (EventBusSubscriber)
├── Systems/
│   ├── Decoration/  — WorldDecorator, DecorationScheduler
│   ├── EventBus/    — Event engine, events, base classes
│   ├── Grid/        — Hex grid, pathfinding, generation passes
│   └── NPC/         — NpcManager, jobs, components
├── UserInterface/   — UIToolkit (main HUD) + UGUI (overlays)
└── Vanguard/        — Player controller, mover
```