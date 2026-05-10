using System;
using Data;
using Systems.Grid.Passes.Abstraction;
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

        /// <summary>
        /// Assigns tile types across the grid based on elevation and moisture thresholds.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                tile.type = DetermineType(tile.Elevation, tile.Moisture);
            }
        }

        /// <summary>
        /// Evaluates a specific tile type based on provided elevation and moisture values.
        /// </summary>
        private Enumerations.TileType DetermineType(float elevation, float moisture)
        {
            if (elevation < waterThreshold) return Enumerations.TileType.Water;
            if (elevation > mountainThreshold) return Enumerations.TileType.Mountain;
            
            if (moisture > 0.5f) return Enumerations.TileType.Forest;
            return Enumerations.TileType.PrimaryGround;
        }
    }
}