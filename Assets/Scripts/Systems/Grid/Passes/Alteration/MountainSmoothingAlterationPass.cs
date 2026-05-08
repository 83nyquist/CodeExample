using System;
using Systems.Decoration.Components;
using Systems.Grid.Passes.Abstraction;
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

                bool surroundedByMountains = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != TileType.Mountain)
                    {
                        surroundedByMountains = false;
                        break;
                    }
                }

                tile.VariationIndex = surroundedByMountains ? 1 : 0;
            }
        }
    }
}
