# Coder Agent - User Preferences

## User Identity
- Name: [User name or role]
- Experience level: [Beginner / Intermediate / Expert Unity developer]
- Primary coding language: C#
- Secondary languages: [None / ShaderLab / HLSL / Other]

## Coding Style Preferences

### Syntax Preferences
- Var usage: [Always when type is obvious / Only for complex types / Never]
- Braces: [Same line / New line]
- Line length preferred: [80 / 100 / 120 / No limit]
- Using statements: [Inside namespace / Outside namespace]

### Naming Preferences
- Private fields: [camelCase with underscore / camelCase without underscore]
- Serialized fields: [Same as private / PascalCase]
- Public properties: [PascalCase]
- Methods: [PascalCase]

### Comment Preferences
- XML comments for: [All public methods / Only complex methods / None]
- Inline comments: [Explain why not what / Explain both / Minimal]
- Comment density: [High / Medium / Low]

## Performance Preferences

### Optimization Priority
When performance conflicts with readability:
- [Always prioritize performance / Prioritize readability first / Balance both]

### Acceptable Performance
- Update method complexity: [Simple operations only / Mild complexity okay / Anything goes]
- GC allocation per frame: [Zero / Under 1KB / Under 10KB / No limit]
- Frame time budget: [16.6ms (60fps) / 33.3ms (30fps) / No constraint]

### Pooling Preference
- Use object pooling for: [Bullets, enemies, particles / Only high-frequency objects / Never use pooling]

## Architecture Preferences

### Pattern Preferences
- Prefer: [MVC / MVVM / Component-based / ECS]
- Avoid: [Singleton overuse / Deep inheritance / God classes]

### Communication Style
- Between components: [Events / Direct references / Both / Service locator]
- Between scenes: [Persistent objects / Scene loading events / PlayerPrefs]

### Data Management
- Configuration data: [ScriptableObjects / JSON files / PlayerPrefs / Hardcoded]
- Save system: [Binary serialization / JSON / PlayerPrefs / Custom]

## Testing Preferences

### Test Writing
- Write tests: [Before code (TDD) / After code / Only for critical systems / Never]
- Test coverage target: [High (90%+) / Medium (70%) / Low (50%) / None]

### Test Types to Write
- Unit tests: [Always / Sometimes / Never]
- Integration tests: [Always / Sometimes / Never]
- Performance tests: [Always / Sometimes / Never]

## Code Review Preferences

### Review Focus
- What to prioritize: [Performance / Readability / Correctness / All equally]
- What to ignore: [Formatting / Comments / Minor style issues]

### Feedback Style
- Prefers: [Direct criticism / Gentle suggestions / Balanced praise and critique]
- Detail level: [Line by line / Summary only / Specific examples]

## Communication Preferences

### Response Style
- Code explanations: [Detailed comments / Brief summary / Code only]
- Error explanations: [Root cause and fix / Just the fix / Link to documentation]

### When Unclear
- Ask for clarification: [Immediately / Guess and mention assumption / Implement both options]

### Code Output
- Include using statements: [Yes / No (assume they exist)]
- Include namespace: [Yes / No]
- Include XML comments: [Yes / No]

## Learning Preferences

### New Pattern Introduction
- When suggesting new patterns: [Explain thoroughly / Show example / Just implement]
- Documentation preference: [Link to official docs / Provide summary / Both]

### Code Reuse
- Prefer: [DRY (Don't Repeat Yourself) / Some duplication is fine / Maximize reuse]
- Refactor frequency: [Eager refactoring / When necessary / Rarely]

## Project-Specific Preferences

### Unity Settings
- Scripting runtime version: [.NET Standard 2.1 / .NET Framework / .NET Core]
- Api compatibility level: [.NET Standard 2.1 / .NET 4.x]

### Version Control
- Commit frequency: [Each logical change / End of day / When feature complete]
- Commit message style: [Detailed / Brief / Standard format]

### Documentation
- Code documentation location: [In code comments / Separate docs folder / Both]
- Update frequency: [With each change / Weekly / Before release]