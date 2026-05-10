using System;
using System.Collections.Generic;
using Data;
using Systems.Grid.Components;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class MassiveMountainAlterationPass : BaseAlterationPass
    {
        [Header("MassiveMountainAlterationPass")]
        public override string PassName => "Massive Mountain Pass";

        /// <summary>
        /// Promotes "Large" mountains to "Massive" status if they are entirely surrounded by other large mountains.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            List<TileData> targets = new List<TileData>();

            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type != Enumerations.TileType.Mountain || tile.VariationIndex != 1) continue;

                bool surroundedByLarge = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    // Must be surrounded by mountains that are at least Index 1
                    if (neighbour == null || neighbour.type != Enumerations.TileType.Mountain || neighbour.VariationIndex < 1)
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

            foreach (var tile in targets)
                tile.VariationIndex = 2;
        }
    }
}