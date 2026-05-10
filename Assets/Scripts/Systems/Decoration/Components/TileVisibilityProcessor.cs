using System.Collections.Generic;
using Systems.Decoration.Structs;
using Systems.Grid.Components;

namespace Systems.Decoration.Components
{
    public static class TileVisibilityProcessor
    {
        /// <summary>
        /// Compares the current vision context against active decorators to determine which tiles to spawn or hide.
        /// </summary>
        /// <param name="context">The calculated vision and active tile sets.</param>
        /// <param name="activeDecorators">The set of tiles currently represented by active GameObjects.</param>
        /// <returns>A tuple containing lists of tiles to show and tiles to hide.</returns>
        public static (List<TileData> toShow, List<TileData> toHide) IdentifyChanges(
            VisionContext context, 
            HashSet<TileData> activeDecorators)
        {
            var toShow = new List<TileData>();
            var toHide = new List<TileData>();
            foreach (var tile in context.ActiveSet)
            {
                bool isInVision = context.VisionSet.Contains(tile);
                
                if (!activeDecorators.Contains(tile))
                {
                    tile.IsDiscovered = true; 
                    tile.IsInVision = isInVision;
                    toShow.Add(tile);
                }
                else if (tile.IsInVision != isInVision)
                {
                    toHide.Add(tile);
                    tile.IsDiscovered = true; 
                    tile.IsInVision = isInVision;
                    toShow.Add(tile);
                }
            }
            foreach (var tile in activeDecorators)
            {
                if (!context.ActiveSet.Contains(tile))
                {
                    toHide.Add(tile);
                    tile.IsInVision = false;
                }
            }

            return (toShow, toHide);
        }
    }
}