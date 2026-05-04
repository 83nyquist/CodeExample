using System.Collections.Generic;
using Systems.Grid.Components;

namespace Systems.Decoration.Structs
{
    /// <summary>
    /// Container representing the desired visibility state of the world.
    /// </summary>
    public readonly struct VisionContext
    {
        public readonly HashSet<TileData> VisionSet;
        public readonly HashSet<TileData> ActiveSet;
        public VisionContext(HashSet<TileData> vision, HashSet<TileData> active) { VisionSet = vision; ActiveSet = active; }
    }
}