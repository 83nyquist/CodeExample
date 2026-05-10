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
    
        /// <summary>
        /// Evaluates water tiles to determine if they should use a "deep water" variation based on neighbors.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type != Enumerations.TileType.Water) continue;

                bool isDeep = true;
                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour == null || neighbour.type != Enumerations.TileType.Water)
                    {
                        isDeep = false;
                        break;
                    }
                }

                tile.VariationIndex = isDeep ? 1 : 0;
            }
        }
    }
}
