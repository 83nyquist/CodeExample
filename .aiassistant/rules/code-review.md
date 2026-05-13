# Code Review Standards

## Rule Type
Manual - Invoke with @rule:code-review in chat

## Critical Issues (Must Fix - Blocks Merge)

- Potential null reference exceptions
- Memory leaks (unsubscribed events, unstoppable coroutines)
- Infinite loops or recursion
- Hardcoded values that should be configurable
- Security issues (exposed sensitive data)

## Major Issues (Should Fix)

- Violates unity-conventions rule
- Poor performance in Update() (heavy operations, allocations)
- Magic numbers (use constants or SerializeField)
- Missing null checks
- No error handling for edge cases

## Minor Issues (Nice to Fix)

- Inconsistent naming
- Missing XML comments for public APIs (summary tags)
- Dead code or commented blocks
- Inefficient LINQ usage
- Not using 'var' where type is obvious

## Review Process - 4 STEPS

Step 1: Understand the Change
- What feature/bug does this address?
- Check related design doc in .aiassistant/design/features/
- Check related ADR in .aiassistant/design/decisions/

Step 2: Code Quality Check
Look for guard clauses, null checks, proper error handling.

Step 3: Performance Check
Flag these if found in Update(), FixedUpdate(), or LateUpdate():
- GameObject.Find() or GameObject.FindWithTag()
- GetComponent() (unless cached)
- Instantiate() / Destroy() (use pooling instead)
- Camera.main property (cache it instead)
- LINQ queries that allocate

Step 4: Unity-Specific Check
- MonoBehaviour methods not calling base.Method() unnecessarily
- Serialized fields have sensible defaults
- Prefabs referenced correctly (not by string path)
- Coroutines have a stop mechanism
- Events unsubscribed in OnDisable/OnDestroy

## Review Output Template - USE THIS FORMAT

# Code Review: [FileName.cs]

## Summary
- Lines reviewed: XX
- Critical issues: X
- Major issues: X
- Minor issues: X

## Critical Issues
(Only if any - these block merge)

**[File.cs:Line#]** Description of issue
- Why: Explanation
- Fix: Specific suggestion

## Major Issues

**[File.cs:Line#]** Description
- Why: Explanation
- Fix: Suggestion

## Minor Issues

**[File.cs:Line#]** Description
- Suggestion: Quick fix

## Positive Observations
- What was done well

## Design Doc Alignment
- Matches: design/features/combat_system.md
- Deviates: See suggestion

## Verdict
- [ ] Request changes (critical issues present)
- [ ] Approve with comments (major issues only)
- [ ] Approve (minor issues only)

## Self-Review Questions (answer before PR)

1. Did I test this in Unity Play Mode?
2. Did I check for null references?
3. Did I follow unity-conventions?
4. Did I update design docs if needed?
5. Is this PR focused on one thing only?