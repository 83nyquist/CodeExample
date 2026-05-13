# Reviewer Agent - Long Term Memory

## Past Review Findings

### Critical Issue Pattern 1: Missing Unsubscription
Found in: Multiple files over time
Pattern: Event subscriptions in OnEnable without OnDisable
Impact: Memory leaks, unexpected behavior after scene changes
Standard fix: Always pair OnEnable subscription with OnDisable unsubscription
Detection method: Search for += in OnEnable, check for matching -=

### Critical Issue Pattern 2: NullReferenceException after Destroy
Found in: Coroutine-heavy code
Pattern: Coroutine accessing GetComponent after object destroyed
Impact: Crashes, errors in console
Standard fix: Check gameObject.activeInHierarchy or add null check each loop
Detection method: Look for GetComponent in while loops or coroutines

### Critical Issue Pattern 3: Infinite Loop Risk
Found in: Recursive methods
Pattern: Method calling itself without proper base case
Impact: Stack overflow, application freeze
Standard fix: Add depth limit or ensure base case is reachable
Detection method: Look for methods calling themselves or mutual recursion

## Major Issue Patterns

### Pattern 1: GameObject.Find in Update
Frequency: Common in beginner code
Standard fix: Cache reference in Awake or Start
Review comment template: "Cache this reference in Awake to avoid per-frame Find calls"

### Pattern 2: Magic Numbers
Frequency: Very common
Standard fix: Extract to named constant or SerializeField
Review comment template: "Extract [number] to a named constant with explanation"

### Pattern 3: Large Monolith Methods
Frequency: Common in refactoring candidates
Standard fix: Break into smaller methods with clear responsibilities
Review comment template: "This method is [X] lines. Consider breaking into smaller methods"

### Pattern 4: Missing Null Checks
Frequency: Common for public methods
Standard fix: Add guard clauses at method start
Review comment template: "Add null check for [parameter] at start of method"

## Performance Anti-Patterns

### Anti-Pattern 1: Camera.main in Update
Why it's bad: Camera.main uses FindObjectWithTag internally
Standard fix: Cache Camera.main in Start
Example:
private Camera _mainCamera;
private void Start() { _mainCamera = Camera.main; }

### Anti-Pattern 2: GetComponent in Update
Why it's bad: Allocates and searches each frame
Standard fix: Cache in Awake
Example:
private Rigidbody _rb;
private void Awake() { _rb = GetComponent<Rigidbody>(); }

### Anti-Pattern 3: String Concatenation in Update
Why it's bad: Creates garbage collection allocations
Standard fix: Use string interpolation or StringBuilder
Example: $"Health: {health}" not "Health: " + health

## Unity-Specific Issues

### Issue 1: Comparing Tags with ==
Better approach: Use CompareTag method
Why: CompareTag is faster and error-handled
Example: if (other.CompareTag("Player")) not if (other.tag == "Player")

### Issue 2: transform.position vs localPosition
Common confusion: Using wrong position space
Check: Is object parented? Does modification need to be relative?
Standard fix: Use localPosition for objects with parents, position for world space

### Issue 3: Destroy vs DestroyImmediate
Common mistake: Using DestroyImmediate in production code
Why: Can cause editor crashes and inconsistent state
Rule: Use Destroy for runtime, DestroyImmediate only in Editor scripts

## Design Doc Alignment Issues

### Common Deviation 1: Architecture mismatch
What to look for: Implementation doesn't match design doc structure
Action: Flag for design doc update or code change

### Common Deviation 2: Missing feature requirements
What to look for: Design doc says feature does X but code doesn't
Action: Request clarification or implementation

### Common Deviation 3: Extra unplanned features
What to look for: Code does something not in design doc
Action: Ask if design doc should be updated

## Learning from Approved Reviews

### What Good Code Looks Like

Example 1: Clean event handling
public class GoodExample : MonoBehaviour
{
    private void OnEnable() => Health.OnDamage += HandleDamage;
    private void OnDisable() => Health.OnDamage -= HandleDamage;
    private void HandleDamage(int damage) { }
}

Why this is good: Proper subscription management, clear responsibility.

Example 2: Efficient Update method
public class GoodUpdate : MonoBehaviour
{
    private Transform _cachedTransform;
    private void Awake() { _cachedTransform = transform; }
    private void Update() 
    { 
        _cachedTransform.position += Vector3.right * Time.deltaTime; 
    }
}

Why this is good: Cached references, no per-frame allocations.

## Review Metrics History

### Average Issues per Review
- Critical: [Number]
- Major: [Number]
- Minor: [Number]

### Most Common Issues (Last 10 Reviews)
1. Missing null checks - [Count]
2. Magic numbers - [Count]
3. No event unsubscription - [Count]
4. Update performance issues - [Count]

### Improvement Trends
- [Positive trend or area needing attention]
- [Positive trend or area needing attention]

## Review Style Preferences Learned

### User Prefers
- Review depth: [Deep / Medium / Shallow]
- Tone: [Direct / Gentle / Mixed]
- Code examples: [Always / When needed / Rarely]

### Team Standards
- Required reviewers: [Number]
- Merge conditions: [All issues fixed / No criticals / Any issues okay]
- Review deadline: [Timeframe]