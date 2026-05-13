# Shared Coding Standards

## C# Coding Standards

### Language Features

#### Use var when type is obvious
Good: var player = new PlayerController();
Good: var health = 100;
Bad: PlayerController player = new PlayerController();

#### Use string interpolation
Good: $"Player {name} has {health} health"
Bad: "Player " + name + " has " + health + " health"

#### Use expression-bodied members for simple methods
Good: public int GetHealth() => _health;
Bad: public int GetHealth() { return _health; }

#### Use null-conditional operator
Good: OnDamageTaken?.Invoke(damage);
Bad: if (OnDamageTaken != null) OnDamageTaken(damage);

#### Use pattern matching for null checks
Good: if (target is null) return;
Good: if (health is not null) health.Heal();
Bad: if (target == null) return;

### File Organization

Order within file:
1. using System;
2. using UnityEngine;
3. using YourProject.Namespace;
4. namespace YourProject.Scripts.Category
5. {
6.     public class ClassName : MonoBehaviour
7.     {
8.         // Public events
9.         // Serialized fields
10.        // Public properties
11.        // Private fields
12.        // Unity lifecycle (Awake -> OnDestroy)
13.        // Public methods
14.        // Private methods
15.    }
16.}

### Commenting

XML comments for all public methods:
/// <summary>
/// Applies damage to the health system.
/// </summary>
/// <param name="damage">Amount of damage to apply (must be positive)</param>
public void TakeDamage(int damage) { }

Inline comments explain WHY, not WHAT:
Good: // Cache reference to avoid per-frame GetComponent
Bad: // Get the rigidbody component

## Unity-Specific Standards

### Component References
Always cache in Awake or Start:
private Rigidbody _rb;
private void Awake() => _rb = GetComponent<Rigidbody>();

### Null Checking with Unity Objects
Use "is null" for UnityEngine.Object derived types:
if (gameObject is null) return;

### Coroutines
Always cache and stop:
private Coroutine _routine;
private void Start() { _routine = StartCoroutine(MyRoutine()); }
private void OnDisable() { if (_routine != null) StopCoroutine(_routine); }

### Event Handling
Always unsubscribe:
private void OnEnable() { Health.OnDeath += HandleDeath; }
private void OnDisable() { Health.OnDeath -= HandleDeath; }

### Inspector Exposure
Use SerializeField for private fields that need inspector access:
[SerializeField] private int _speed = 10;

Use Header and Tooltip attributes:
[Header("Movement")]
[SerializeField] [Tooltip("Units per second")] private float _speed = 5f;

## Performance Standards

### Update Method Rules
Allowed in Update:
- Simple float operations
- Input checks
- Transform position reads

Not allowed in Update:
- GetComponent calls (cache them)
- GameObject.Find calls
- Instantiate/Destroy
- Camera.main property
- LINQ queries
- String concatenation

### Memory Allocation
Zero GC allocation per frame is the target
Use object pooling for frequent spawns
Use StringBuilder for complex string operations
Cache frequently used references

## Testing Standards

### Test Naming
MethodName_Scenario_ExpectedResult
Example: TakeDamage_WhenDamageIsPositive_ReducesHealth

### Test Structure
Arrange, Act, Assert pattern:
// Arrange (setup)
// Act (execute)
// Assert (verify)

### What to Test
- Happy path (normal operation)
- Edge cases (0, null, empty, max)
- Error cases (invalid inputs)
- Performance (no GC allocation)

## Git Standards

### Commit Message Format
<type>(<scope>): <subject>

Types: feat, fix, docs, style, refactor, perf, test, chore

Subject rules:
- Present tense ("add" not "added")
- No period at end
- Max 50 characters

### Commit Size
One commit = one logical change
If you can't summarize in 50 chars, split it

## Code Review Standards

### Critical Issues (Block Merge)
- Null reference possibilities
- Memory leaks (unsubscribed events)
- Infinite loops
- Hardcoded values

### Major Issues (Should Fix)
- Performance problems in Update
- Missing null checks
- Magic numbers
- Violates coding standards

### Minor Issues (Nice to Fix)
- Inconsistent naming
- Missing comments
- Code duplication

## Exception Handling

### When to Catch Exceptions
- File I/O operations
- Network operations
- Third-party API calls
- Deserialization

### When NOT to Catch
- NullReferenceException (prevent with checks instead)
- ArgumentException (validate parameters instead)

### Logging
Use Debug.LogWarning for unexpected but recoverable states
Use Debug.LogError for unrecoverable errors
Use Debug.Log for development only (wrap in #if UNITY_EDITOR)