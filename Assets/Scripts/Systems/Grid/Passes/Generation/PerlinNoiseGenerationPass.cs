using System;
using Data;
using Systems.Grid.Components;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;

namespace Systems.Grid.Passes.Generation
{
    [Serializable]
    public class PerlinNoiseGenerationPass : BaseGenerationPass
    {
        [Header("PerlinNoiseGeneratorPass")]
        [Header("Frequency Settings")]
        public float elevationScale = 0.1f;
        public float moistureScale = 0.15f;
        
        [Header("Elevation Offsets")]
        public float elevationOffset = 0f;
        
        [Header("Biome Thresholds")]
        [Range(0, 1)] public float waterThreshold = 0.3f;
        [Range(0, 1)] public float mountainThreshold = 0.8f;
        
        [Range(0, 1)] public float forestMoisture = 0.6f;
        [Range(0, 1)] public float fieldMoisture = 0.3f;
        
        private int _seed;
        
        public override string PassName => "Perlin Noise Pass";
        
        /// <summary>
        /// Populates grid tiles with Perlin noise-based elevation and moisture, and determines initial tile types.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            _seed = seed;
            
            foreach (var kvp in grid.Tiles)
            {
                TileData tile = kvp.Value;
                
                float elevation = GetElevationAt(tile.X, tile.Z);
                float moisture = GetMoistureAt(tile.X, tile.Z);
                
                tile.Elevation = elevation;
                tile.Moisture = moisture;
                tile.type = DetermineTileType(elevation, moisture);
            }
        
            if (debugLog)
            {
                Debug.Log($"[{PassName}] Processed {grid.Tiles.Count} tiles on seed {_seed}");
            }
        }
        
        /// <summary>
        /// Calculates a multi-octave Perlin noise value for elevation at the given coordinates.
        /// </summary>
        private float GetElevationAt(int x, int y)
        {
            float xf = x * elevationScale;
            float yf = y * elevationScale;
            
            float elevation = Mathf.PerlinNoise(xf + _seed, yf + _seed);
            elevation += Mathf.PerlinNoise(xf * 2f + _seed, yf * 2f + _seed) * 0.3f;
            elevation = Mathf.Clamp01(elevation + elevationOffset);
            
            return elevation;
        }
        
        /// <summary>
        /// Calculates a Perlin noise value for moisture at the given coordinates using a seed offset.
        /// </summary>
        private float GetMoistureAt(int x, int y)
        {
            float xf = x * moistureScale;
            float yf = y * moistureScale;
            
            float moisture = Mathf.PerlinNoise(xf + _seed + 1000, yf + _seed + 1000);
            moisture = Mathf.Clamp01(moisture);
            
            return moisture;
        }
        
        /// <summary>
        /// Maps elevation and moisture values to specific tile types based on defined thresholds.
        /// </summary>
        private Enumerations.TileType DetermineTileType(float elevation, float moisture)
        {
            if (elevation < waterThreshold)
                return Enumerations.TileType.Water;
            
            if (elevation > mountainThreshold)
                return Enumerations.TileType.Mountain;
            
            if (moisture > forestMoisture && elevation > waterThreshold + 0.2f)
                return Enumerations.TileType.Forest;
            
            if (moisture > fieldMoisture)
                return Enumerations.TileType.PrimaryGround;
            
            return Enumerations.TileType.SecondaryGround;
        }
    }
}