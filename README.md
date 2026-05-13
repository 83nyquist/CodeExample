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

   

