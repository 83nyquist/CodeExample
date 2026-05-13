# AGENTS.md - Main Agent Configuration for Unity Project

## Project Context
- Engine: Unity 6000.4.6f1 LTS
- Language: C# (.NET Standard 2.1)
- IDE: JetBrains Rider 2024+
- Version Control: Git (GitHub)
- Project Type: Game Development

## Agent System Location
All agent files are in .aiassistant/.

## Core Agent Instructions

Planner Agent: #file:.aiassistant/agents/planner/agent.md
Coder Agent: #file:.aiassistant/agents/coder/agent.md
Reviewer Agent: #file:.aiassistant/agents/reviewer/agent.md

## Memory & Context

Global Memory: #file:.aiassistant/memory/global_memory.md
Session Context: #file:.aiassistant/memory/session.md
Project Context: #file:.aiassistant/memory/project_context.md

User Preferences:
- Planner user: #file:.aiassistant/agents/planner/user.md
- Coder user: #file:.aiassistant/agents/coder/user.md
- Reviewer user: #file:.aiassistant/agents/reviewer/user.md

## Design Documentation

Architecture: #folder:.aiassistant/design/architecture/
Features: #folder:.aiassistant/design/features/
Decisions (ADRs): #folder:.aiassistant/design/decisions/
Design Index: #file:.aiassistant/design/README.md

## Rules (Chat Behavior)

Unity Conventions: #rule:unity-conventions
Code Review Standards: #rule:code-review
Testing Guidelines: #rule:testing-guidelines
Git Commit Rules: #rule:git-commits

## Shared Resources

Coding Standards: #file:.aiassistant/shared/coding_standards.md
Glossary: #file:.aiassistant/shared/glossary.md
Templates: #folder:.aiassistant/shared/templates/

## Unity-Specific Development Rules

### Code Organization
Assets/Scripts/Core/ - Game managers, singletons
Assets/Scripts/Gameplay/ - Player, enemies, combat
Assets/Scripts/UI/ - Canvas controllers
Assets/Scripts/Data/ - ScriptableObjects
Assets/Scripts/Utils/ - Helpers, extensions
Assets/Editor/ - Editor-only scripts

### Naming Conventions
Public methods: PascalCase (UpdatePlayerHealth)
Private fields: camelCase with _ prefix (_currentHealth)
Serialized fields: [SerializeField] private int _playerSpeed;
Public properties: PascalCase (public int Health { get; private set; })
Constants: UPPER_SNAKE_CASE (MAX_PLAYERS)

### Performance Rules
- Cache GetComponent() in Awake() or Start()
- Avoid GameObject.Find() - use serialized references
- Use ObjectPool for frequently instantiated objects
- Use TryGetComponent() over GetComponent() + null check
- Use Update() sparingly - use FixedUpdate for physics

### Best Practices
- Composition over inheritance
- ScriptableObjects for data-driven design
- Events/delegates over direct references
- Null checking with "is null" or "is not null"
- String interpolation over concatenation
- #nullable enable for new files

## Workflow Instructions

### When Implementing a Feature
1. Check #folder:.aiassistant/design/features/ for existing spec
2. If no spec exists, suggest creating one using template
3. Check #file:.aiassistant/shared/coding_standards.md for patterns
4. Generate code following #rule:unity-conventions
5. Update #file:.aiassistant/memory/session.md with progress

### When Refactoring
1. Check #folder:.aiassistant/design/architecture/ for intended architecture
2. Verify changes don't violate documented ADRs
3. Run #rule:testing-guidelines to ensure tests pass
4. Suggest updating design docs if architecture changes

### When Reviewing Code
1. Apply #rule:code-review
2. Check against #rule:unity-conventions
3. Flag any deviation from #folder:.aiassistant/design/
4. Suggest improvements following #rule:code-review

### When Committing to Git
1. Follow #rule:git-commits
2. Keep commits atomic and focused
3. Reference design docs if applicable

## Session Management
- Start of session: Read #file:.aiassistant/memory/session.md
- During session: Update session.md with key decisions
- End of session: Archive to #folder:.aiassistant/logs/ with timestamp

## What NOT to Do
- Don't modify #file:.aiassistant/memory/global_memory.md without explicit user request
- Don't delete design docs without confirmation
- Don't ignore #rule:testing-guidelines - tests must pass
- Don't commit to main branch directly - always use PRs

## Definition of Done
- [ ] Code compiles without errors
- [ ] No Rider warnings in modified files
- [ ] Unity Play Mode tests pass
- [ ] Follows #rule:unity-conventions
- [ ] Design doc updated if applicable
- [ ] Session.md updated with changes

## Quick Reference
- Rules: Use @rule:name or #rule:name in chat
- Files: Use #file:path to attach specific files
- Folders: Use #folder:path to attach entire directories
- Local changes: #localChanges shows uncommitted Git changes