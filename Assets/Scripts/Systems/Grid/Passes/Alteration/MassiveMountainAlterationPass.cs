using System;
using System.Collections.Generic;
using Systems.Decoration.Components;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class MassiveMountainAlterationPass : BaseAlterationPass
    {
        [Header("MassiveMountainAlterationPass")]
        public override string PassName => "Massive Mountain Pass";

        public override void Execute(AxialHexGrid grid, int seed)
        {
            // We collect targets in a list to prevent "bleeding" logic errors 
            // where a tile's neighbor changes state during the same loop.
            List<TileData> targets = new List<TileData>();

            foreach (var tile in grid.Tiles.Values)
            {
                // Only consider mountains that are already "Large" (Index 1)
                if (tile.type != TileType.Mountain || tile.VariationIndex != 1) continue;

                bool surroundedByLarge = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    // Must be surrounded by mountains that are at least Index 1
                    if (neighbour == null || neighbour.type != TileType.Mountain || neighbour.VariationIndex < 1)
                    {
                        surroundedByLarge = false;
                        break;
                    }
                }

                if (surroundedByLarge)
                {
                    targets.Add(tile);
                }
            }

            // Finalize the promotion to Index 2
            foreach (var tile in targets)
                tile.VariationIndex = 2;
        }
    }
}