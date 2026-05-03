using System;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class MountainSmoothingAlterationPass : BaseAlterationPass
    {
        [Header("MountainSmoothingAlterationPass")]
        public override string PassName => "Mountain Smoothing Pass";
    
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type != TileType.Mountain) continue;

                bool onlyMountainNeighbours = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != TileType.Mountain)
                    {
                        onlyMountainNeighbours = false;
                        break;
                    }
                }

                // Set the variation index based on your requirements:
                // Default (Small) = 0, Surrounded (Large) = 1
                tile.VariationIndex = onlyMountainNeighbours ? 1 : 0;
            }
        }
    }
}
