using System;
using Data;
using Systems.Grid.Passes.Abstraction;
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
                if (tile.type != Enumerations.TileType.Water) continue;

                bool surroundedByMountains = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != Enumerations.TileType.Water)
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
