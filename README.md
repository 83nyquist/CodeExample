# CodeExample - Hex Grid Terrain Generation

A Unity demonstration project featuring a top-down axial hex grid system supporting 3 000 000 tiles with Perlin noise terrain generation and 10 000 independent agents with simple pathfinding.

Keywords:
- UI Toolkit and UGUI integration.
- Object pooling to utilizes memory efficiently by reusing instances instead of allocating and garbage collecting them repeatedly.
- Coroutines to utilize the main thread efficiently by spreading work across multiple frames.
- Multithreading with the C# Job System for performance optimization.

## Technical Highlights

v2.0.1
- **Added Generation variations**
- **Added Fog of War**
- **Performance Optimization and refactoring**

v1.2.1
- **User Interface improvements**

v1.0.2
- **Performance optimization**

v1.0.1
- **Multithreading using the Job System** - NPC system supporting a large amount of individiual agents

v1.0.0
- **Hex Grid System** - Complete top-down hex grid implementation with cell navigation
- **Perlin Noise Generation** - Procedural terrain height and feature mapping
- **UI Toolkit** - Modern Unity UI system for clean, scalable interface design
- **Coroutine & Object Pooling** - Optimized generation of tile decoration for smooth performance


## How to Run the Build

### Windows Build
1. Download `WindowsBuild-Windows.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive to a folder
3. Double-click `CodeExample.exe` to launch the game

### Linux Build
1. Download `LinuxBuild-Linux.zip` from the [Releases](https://github.com/83nyquist/CodeExample/releases) section
2. Extract the archive
3. Make the executable runnable:
   ```bash
   chmod +x CodeExample.x86_64
