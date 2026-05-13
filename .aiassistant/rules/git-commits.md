# Git Commit Guidelines

## Rule Type
Manual - Invoke with @rule:git-commits when writing commit messages

## Commit Message Format

<type>(<scope>): <subject>

[optional body]

[optional footer(s)]

## Commit Types

feat: New feature (example: feat(combat): add critical hit system)
fix: Bug fix (example: fix(player): prevent double jump while stunned)
docs: Documentation only (example: docs(api): update weapon damage formulas)
style: Code style formatting only (example: style(ui): format health bar code)
refactor: Code change no feature/fix (example: refactor(inventory): extract item validation)
perf: Performance improvement (example: perf(ai): cache transform lookups)
test: Adding missing tests (example: test(combat): add damage calculation tests)
chore: Maintenance tasks (example: chore: update Unity to 2022.3.15)
revert: Revert a commit (example: revert: undo player movement changes)

## Scope Options (Unity-specific)

player - Player-related changes
combat - Combat system
enemy - Enemy AI
ui - User interface
inventory - Inventory system
audio - Sound effects/music
networking - Multiplayer code
editor - Unity Editor tools
build - Build pipeline
shaders - Shader/rendering changes

## Subject Rules

- Use imperative present tense ("add" not "added" or "adds")
- No capitalization (first letter lowercase)
- No period at the end
- Max 50 characters

## Valid Examples

Simple commit:
feat(combat): add damage over time effect

Commit with body:
fix(player): correct jump height calculation

The jump height was inconsistent when framerate varied. Now using
Time.deltaTime properly in FixedUpdate to ensure consistent height.

Fixes #123

Breaking change:
refactor(inventory)!: change item ID from int to string

BREAKING CHANGE: All saved games must be migrated. Use
InventoryMigrationTool to convert old saves.

Closes #456

With co-authors:
feat(ai): implement enemy patrol behavior

- Add waypoint system
- Add detection radius
- Add state machine for patrol/chase

Co-authored-by: Jane Doe <jane@example.com>

With design doc reference:
feat(combat): implement damage types

Implements damage types (physical, fire, cold) as specified in
design/features/combat_system.md

See: .aiassistant/design/features/combat_system.md#damage-types

## Commit Size Guidelines

GOOD (one logical change):
feat(player): add sprint ability
- Changes only player sprint code
- About 50 lines changed
- Easy to review and revert

BAD (multiple unrelated changes):
fix player and enemy bugs, update docs, refactor UI
- Should be 3+ separate commits
- Hard to review
- Difficult to revert partially

Rule of thumb:
- One commit = one logical change
- If you cannot summarize in 50 characters, split it
- If the commit fixes multiple bugs, split it
- If unrelated files changed, it was not one change

## When to Commit

DO commit:
- After implementing a single feature/fix
- After passing all tests
- Before switching branches
- At the end of a work session

DO NOT commit:
- Multiple unrelated changes together
- Code that breaks tests
- Half-finished features (use branches instead)
- Debug code or commented-out blocks

## Special Cases

Work in Progress (WIP):
chore(wip): checkpoint before refactoring player movement
[skip ci]

Note: Use [skip ci] to avoid CI/CD runs. Should be squashed before merging.

Reverting a commit:
revert: revert "feat(combat): add critical hits"

This reverts commit abc123def456 because it broke damage calculation.

Reason: Critical hits were applying twice due to event duplication.

Minor fixes (typos, comments):
fix(ui): fix typo in health bar tooltip

## Commit Message Checklist

Before committing, verify:
- [ ] Subject line describes the change clearly
- [ ] Type is appropriate (feat/fix/docs/etc.)
- [ ] Scope is specified and accurate
- [ ] No period at end of subject
- [ ] Subject under 50 characters
- [ ] Body explains why (if non-obvious)
- [ ] References issues or design docs where applicable
- [ ] Breaking changes noted with ! and BREAKING CHANGE footer

## PR Commit Requirements

- [ ] All commit messages follow format
- [ ] No WIP commits (squash before merge)
- [ ] Commit history is clean (no "fix typo" follow-up commits)
- [ ] Breaking changes are highlighted
- [ ] Design docs referenced when applicable