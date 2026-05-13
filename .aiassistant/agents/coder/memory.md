# Coder Agent - Long Term Memory

## Successful Code Patterns

### Pattern 1: Event-Based Health System
When to use: Any system that needs to notify multiple components about state changes

Example structure:
public class HealthSystem : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    
    private int _currentHealth;
    private int _maxHealth;
    
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        if (_currentHealth <= 0) OnDeath?.Invoke();
    }
}

Why it worked well: Loose coupling, easy to test, no Find references.

### Pattern 2: Object Pool for Bullets
When to use: Frequently spawned objects like bullets, enemies, particles

Example structure:
public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _poolSize = 20;
    private Queue<GameObject> _pool = new Queue<GameObject>();
    
    private void Awake()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject bullet = Instantiate(_bulletPrefab);
            bullet.SetActive(false);
            _pool.Enqueue(bullet);
        }
    }
    
    public GameObject Get()
    {
        if (_pool.Count > 0)
        {
            GameObject bullet = _pool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        return Instantiate(_bulletPrefab);
    }
    
    public void Return(GameObject bullet)
    {
        bullet.SetActive(false);
        _pool.Enqueue(bullet);
    }
}

Why it worked well: Eliminated GC spikes, consistent performance.

### Pattern 3: ScriptableObject Event Channel
When to use: Cross-system communication without direct references

Example structure:
[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Events/VoidEventChannel")]
public class VoidEventChannel : ScriptableObject
{
    public event Action OnEventRaised;
    
    public void RaiseEvent() => OnEventRaised?.Invoke();
}

Why it worked well: Decouples systems, easy to add listeners, works across scenes.

### Pattern 4: Component Cache on Awake
When to use: Any MonoBehaviour that needs component references

Example structure:
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;
    private Collider _collider;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
    }
}

Why it worked well: Single GetComponent call, cached for entire lifetime.

## Performance Optimizations Discovered

### Optimization 1: LayerMask over tag comparison
Problem: Comparing tags in Update was slow
Solution: Use LayerMask and compare layers
Gain: 30% faster collision checks

### Optimization 2: Dictionary over List for lookups
Problem: Searching List by ID was O(n)
Solution: Use Dictionary<int, T> for ID-based lookups
Gain: O(1) lookups instead of O(n)

### Optimization 3: Object pooling for audio sources
Problem: Instantiate/Destroy for sound effects caused GC
Solution: Pool of AudioSource components
Gain: Zero allocations for audio playback

## Common Bugs and Fixes

### Bug 1: NullReferenceException after scene load
Cause: Events not unsubscribed before scene change
Fix: Always unsubscribe in OnDisable
Prevention: Add OnDisable whenever OnEnable has subscriptions

### Bug 2: Coroutine continues after object destroyed
Cause: No stop mechanism for coroutines
Fix: Cache coroutine reference and stop in OnDisable
Prevention: Always pair StartCoroutine with ability to stop

### Bug 3: Double jump when colliding with multiple triggers
Cause: Multiple trigger events firing
Fix: Add cooldown flag or use OnTriggerEnter with state check
Prevention: Always check current state before acting on triggers

## Code Review Lessons

### Lesson 1: Public fields are dangerous
Learned from: Player health being modified from anywhere
Solution: Use properties with private setters
Rule: Never public fields, always properties or SerializeField private

### Lesson 2: Start is too late for references
Learned from: Objects referencing each other in Start caused race conditions
Solution: Use Awake for own reference setup, Start for cross-object initialization
Rule: Awake for this, Start for others

### Lesson 3: Coroutines need defensive programming
Learned from: Coroutines continuing after conditions changed
Solution: Check conditions inside loop, not just at start
Rule: Always re-validate state each iteration

## Architecture Decisions

### Decision 1: Use interfaces for damageable objects
- Made on: [Date]
- Why: Allows any object to be damageable without inheritance
- Implemented as: IDamageable interface with TakeDamage method

### Decision 2: Singleton only for truly global systems
- Made on: [Date]
- Why: Singletons make testing hard
- Exception: AudioManager, SceneLoader (only when truly single instance)

### Decision 3: No direct reference to UI from gameplay code
- Made on: [Date]
- Why: Separation of concerns
- Implementation: Events from gameplay, UI listens and updates

## Third-Party Integration Notes

### Integration 1: [Asset name]
- What it does: 
- How to use: 
- Common pitfalls: 
- Best practices: 

### Integration 2: [Asset name]
- What it does: 
- How to use: 
- Common pitfalls: 
- Best practices: