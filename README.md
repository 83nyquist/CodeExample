# CodeExample - Hex Grid Terrain Generation

This is a Unity demonstration project featuring a top-down axial hex grid system supporting 3,000,000 tiles with Perlin noise terrain generation and 10,000 independent agents with independent roaming.

<img src="images/map.png" alt="Hex grid terrain map" width="300" height="300">


## License

This project is licensed under **CC BY-NC-ND 4.0** (see LICENSE file).

© 2025 Cato Aleksander Goffeng

### For recruiters, employers, and portfolio reviewers:

**You MAY:**
- View, clone, fork, and run this code to evaluate my technical skills
- Share this repository link in interview contexts
- Use this code for educational reference

**You MAY NOT:**
- Use this code for commercial purposes (including in employer products)
- Modify this code and claim modified versions as your own
- Remove my name, this license, or copyright notices
- Submit this code as part of another person's job application or portfolio

**Any questions?**  
Contact me at 83nyquist@gmail.com.



## Technical Highlights
- **Multithreading using the Job System** - NPC system supporting a large number of individual agents (Systems/NPC)
- **Hex Grid System** - Complete top-down hex grid implementation with cell navigation (Systems/Grid)
- **A-Star Pathfinding** - Optimal route calculation across hex grid (Systems/Grid)
- **Perlin Noise Generation** - Procedural terrain height and feature mapping (Systems/Grid)
- **Coroutine & Object Pooling** - Optimized generation of tile decoration for smooth performance (Systems/Decorator)
- **Dependency Injection** - VContainer for service management and decoupling (Systems/DependencyInjection/BootloaderScope)
- **Event-Driven Architecture** - Centralized `GameEventBus` with source tracking for debugging (Systems/EventBus)
- **UI Toolkit and UGUI Integration** - Hybrid UI system combining modern Toolkit with legacy UGUI for maximum compatibility and flexibility (UserInterface)
- **AI-Assisted Development Workflow** - Integration of AI coding assistant with version-controlled agent configuration (`.aiassistant/`)

### AI-Assisted Development Workflow

This project uses a structured AI-assisted development workflow where the AI assistant is configured as three specialized agents:

| Agent | Responsibility | Configuration |
|-------|----------------|----------------|
| **Planner** | Feature breakdown, technical specifications, task estimation | `.aiassistant/agents/planner/` |
| **Coder** | Code generation, Unity best practices, performance optimization | `.aiassistant/agents/coder/` |
| **Reviewer** | Code quality, design doc alignment, standards enforcement | `.aiassistant/agents/reviewer/` |

#### Key Features of the AI Workflow

- **Version-Controlled Agent Configuration** - All agent instructions live in `.aiassistant/` and are tracked in Git, enabling consistent AI behavior across team members
- **Living Design Documentation** - Design specs and Architecture Decision Records (ADRs) stored as markdown in `.aiassistant/design/`, editable by both humans and AI
- **Obsidian Integration** - The `.aiassistant/` folder can be opened as an Obsidian vault, providing rich markdown editing, backlinks, and graph visualization
- **Rider AI Assistant Integration** - `AGENTS.md` in project root provides automatic AI configuration discovery
- **Rule-Based Chat Behavior** - Project-specific rules (coding conventions, code review standards, testing guidelines, Git commits) in `.aiassistant/rules/`

#### Project-Specific Rules

| Rule File | Purpose |
|-----------|---------|
| `unity-conventions.md` | Unity C# naming, performance, and architecture standards |
| `code-review.md` | Code review checklist and severity guidelines |
| `testing-guidelines.md` | Unit test structure and requirements |
| `git-commits.md` | Commit message format and branching strategy |

#### How to Use This Workflow

1. **In JetBrains Rider**: The AI Assistant automatically detects `AGENTS.md` and loads the agent configurations
2. **In Obsidian**: Open this repository as a vault to view/edit agent configs and design docs with full markdown support
3. **In Any Editor**: All agent files are plain markdown - readable and editable by humans

#### Repository Structure for AI Workflow
```
├── AGENTS.md # Rider AI Assistant entry point  
├── .aiassistant/ # Complete agent system  
│ ├── agents/ # Planner, Coder, Reviewer personas  
│ ├── memory/ # Long-term and session memory  
│ ├── design/ # Living design documentation  
│ ├── rules/ # Chat behavior rules  
│ └── shared/ # Templates and standards  
├── Assets/ # Unity project  
└── ...
```


#### Benefits Demonstrated

- **Reproducible AI Behavior** - Agent configurations are version-controlled, not ephemeral chat history
- **Team Collaboration** - Everyone uses the same agent rules and design docs
- **Observability** - Design decisions and agent memories are human-readable markdown
- **Tool-Agnostic** - Same files work with Rider AI Assistant, Claude, or any markdown-capable AI tool

## How to Run the Build

### Windows Build
1. Download `WindowsBuild-Windows.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive to a folder
3. Double-click `CodeExample.exe` to launch the game

### Linux Build
1. Download `LinuxBuild-Linux.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive
3. Make the executable runnable: `chmod +x CodeExample.x86_64`
4. Run the executable: `./CodeExample.x86_64`