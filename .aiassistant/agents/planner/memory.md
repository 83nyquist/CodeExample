# Planner Agent - Long Term Memory

## Project Decisions

### Decision 1: [Date - Decision made]
- What was decided: 
- Why: 
- Alternatives considered: 
- Affected systems: 

## Architectural Preferences

### Composition Over Inheritance
Prefer component-based design over deep class hierarchies. Use MonoBehaviour components for behaviors rather than inheritance chains.

### ScriptableObject Usage
Use ScriptableObjects for:
- Game data that doesn't change at runtime
- Event channels
- Shared configurations

### Event-Driven Communication
Use C# events or UnityEvents for component communication. Avoid direct references that create tight coupling.

## Learned Patterns

### Pattern 1: [Pattern name]
When to use: 
Example: 
Why it worked well: 

### Pattern 2: [Pattern name]
When to use: 
Example: 
Why it worked well: 

## Project-Specific Knowledge

### Key Classes
- [Class name]: [What it does and why it matters]

### Important Files
- [File path]: [Why this file is important]

### Third-Party Integrations
- [Integration name]: [What it does, how to use it]

## Performance Learnings

### Bottleneck 1: [Description]
- Cause: 
- Fix applied: 
- Performance gain: 

### Bottleneck 2: [Description]
- Cause: 
- Fix applied: 
- Performance gain: 

## Mistakes to Remember

### Mistake 1: [Description]
- What happened: 
- Why it was wrong: 
- What to do instead: 

### Mistake 2: [Description]
- What happened: 
- Why it was wrong: 
- What to do instead: 

## User Preferences (Planning)

### Detail Level
- Prefers: [High level overview vs Detailed breakdown]
- Spec format: [Bullets vs Paragraphs vs Tables]

### Estimation Style
- Prefers: [Conservative vs Optimistic estimates]
- Buffer preference: [Percentage or specific hours]

### Communication Style
- Prefers: [Concise vs Explanatory]
- Format: [Plain text vs Checkboxes vs Tables]

## Success Metrics

### Metric 1: [Name]
- Target: 
- Current: 
- Last updated: 

### Metric 2: [Name]
- Target: 
- Current: 
- Last updated: