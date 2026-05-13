# Unity C# Coding Conventions

## Rule Type
By file patterns - Applied to *.cs files in Assets/Scripts/**

## Naming Conventions

Classes/Structs: PascalCase (example: PlayerController)
Methods: PascalCase (example: UpdatePlayerHealth)
Private fields: camelCase with _ prefix (example: _playerHealth)
Serialized fields: [SerializeField] private + camelCase with _ (example: [SerializeField] private int _playerSpeed)
Public properties: PascalCase (example: public int MaxHealth { get; private set; })
Constants: UPPER_SNAKE_CASE (example: MAX_PLAYER_COUNT)

## Unity Lifecycle Order (8 methods in order)

1. Awake() - Reference setup
2. OnEnable() - Called when object becomes active
3. Start() - Initialization that may depend on other objects
4. Update() - Frame-rate dependent logic (avoid heavy operations)
5. FixedUpdate() - Physics calculations
6. LateUpdate() - Camera following
7. OnDisable() - Cleanup when object deactivates
8. OnDestroy() - Final cleanup

## Performance Rules - DO THESE

- Cache GetComponent() in Awake() or Start(), never in Update()
- Use ObjectPool for frequently instantiated objects
- Use string interpolation: $"Player {name} scored {score}"
- Avoid GameObject.Find() in Update()
- Use TryGetComponent() over GetComponent() + null check
- Use "is null" / "is not null" for null checks (not == null)

## Component Communication - USE EVENTS

Good pattern:
public static event Action<int> OnPlayerDamaged;
private void TakeDamage(int damage) => OnPlayerDamaged?.Invoke(damage);

Bad pattern (avoid):
private void TakeDamage(int damage) {
    var ui = GameObject.Find("HealthBar").GetComponent<HealthUI>();
}

## Coroutines - CACHE AND STOP

Good pattern:
private Coroutine _flashRoutine;
private void StartFlash() {
    if (_flashRoutine != null) StopCoroutine(_flashRoutine);
    _flashRoutine = StartCoroutine(FlashEffect());
}

Bad pattern (avoid):
private void StartFlash() => StartCoroutine(FlashEffect());

## File Organization Order (within each .cs file)

1. using statements
2. namespace
3. Public events/delegates
4. Serialized fields [SerializeField]
5. Public properties
6. Private fields
7. Unity lifecycle methods (Awake through OnDestroy)
8. Public methods
9. Private methods

## Exceptions

- Editor scripts (Assets/Editor/**) may differ
- Third-party code in Assets/Plugins/ is excluded
- Legacy code exemption: add comment "// LEGACY: Refactor when possible"

## Rule Application

- File patterns: **/*.cs, Assets/Scripts/**/*.cs
- Exclude: Assets/Plugins/**, Assets/ThirdParty/**
- Priority: High