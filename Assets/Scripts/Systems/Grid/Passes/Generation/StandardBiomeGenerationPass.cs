using System;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Generation
{
    [Serializable]
    public class StandardBiomeGenerationPass : BaseGenerationPass
    {
        [Header("StandardBiomeGenerationPass")]
        public float waterThreshold = 0.3f;
        public float mountainThreshold = 0.8f;

        public override string PassName => "Standard Biome Logic Pass";

        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                tile.type = DetermineType(tile.Elevation, tile.Moisture);
            }
        }

        private TileType DetermineType(float elevation, float moisture)
        {
            if (elevation < waterThreshold) return TileType.Water;
            if (elevation > mountainThreshold) return TileType.Mountain;
            
            // Example: Simple split
            if (moisture > 0.5f) return TileType.Forest;
            return TileType.PrimaryGround;
        }
    }
}