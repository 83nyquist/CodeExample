using System.Collections.Generic;
using Systems.Decoration.Structs;
using Systems.Grid.Components;

namespace Systems.Decoration.Components
{
    public static class TileVisibilityProcessor
    {
        public static (List<TileData> toShow, List<TileData> toHide) IdentifyChanges(
            VisionContext context, 
            HashSet<TileData> activeDecorators)
        {
            var toShow = new List<TileData>();
            var toHide = new List<TileData>();

            // 1. Process tiles that should be active
            foreach (var tile in context.ActiveSet)
            {
                bool isInVision = context.VisionSet.Contains(tile);
                
                if (!activeDecorators.Contains(tile))
                {
                    // Entering the scene
                    tile.IsDiscovered = true; 
                    tile.IsInVision = isInVision;
                    toShow.Add(tile);
                }
                else if (tile.IsInVision != isInVision)
                {
                    // State swap (Full <-> Shrouded)
                    toHide.Add(tile);
                    tile.IsDiscovered = true; 
                    tile.IsInVision = isInVision;
                    toShow.Add(tile);
                }
            }

            // 2. Process tiles leaving the active range
            foreach (var tile in activeDecorators)
            {
                if (!context.ActiveSet.Contains(tile))
                {
                    toHide.Add(tile);
                    // Reset flags for pooled objects
                    tile.IsInVision = false;
                }
            }

            return (toShow, toHide);
        }
    }
}