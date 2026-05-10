using System;
using Data;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class MountainSmoothingAlterationPass : BaseAlterationPass
    {
        [Header("MountainSmoothingAlterationPass")]
        public override string PassName => "Mountain Smoothing Pass";
    
        /// <summary>
        /// Updates mountain tile variations based on whether they are completely surrounded by other mountains.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type != Enumerations.TileType.Mountain) continue;

                bool surroundedByMountains = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != Enumerations.TileType.Mountain)
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
