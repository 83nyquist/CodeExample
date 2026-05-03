using System;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class WaterDepthAlterationPass : BaseAlterationPass
    {
        
        [Header("WaterDepthAlterationPass")]
        public override string PassName => "Water Depth Pass";
    
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type != TileType.Water) continue;

                bool surroundedByMountains = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != TileType.Water)
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
