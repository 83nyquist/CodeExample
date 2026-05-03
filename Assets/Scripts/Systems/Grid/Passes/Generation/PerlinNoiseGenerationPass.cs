using System;
using Systems.Decoration.Components;
using Systems.Grid.Components;
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
        
        public override void Execute(AxialHexGrid grid, int seed)
        {
            _seed = seed;
            
            foreach (var kvp in grid.Tiles)
            {
                TileData tile = kvp.Value;
                
                // Using x,y as 2D coordinates (q = x, r = Z)
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
        
        private float GetElevationAt(int x, int y)
        {
            float xf = x * elevationScale;
            float yf = y * elevationScale;
            
            float elevation = Mathf.PerlinNoise(xf + _seed, yf + _seed);
            elevation += Mathf.PerlinNoise(xf * 2f + _seed, yf * 2f + _seed) * 0.3f;
            elevation = Mathf.Clamp01(elevation + elevationOffset);
            
            return elevation;
        }
        
        private float GetMoistureAt(int x, int y)
        {
            float xf = x * moistureScale;
            float yf = y * moistureScale;
            
            float moisture = Mathf.PerlinNoise(xf + _seed + 1000, yf + _seed + 1000);
            moisture = Mathf.Clamp01(moisture);
            
            return moisture;
        }
        
        private TileType DetermineTileType(float elevation, float moisture)
        {
            if (elevation < waterThreshold)
                return TileType.Water;
            
            if (elevation > mountainThreshold)
                return TileType.Mountain;
            
            if (moisture > forestMoisture && elevation > waterThreshold + 0.2f)
                return TileType.Forest;
            
            if (moisture > fieldMoisture)
                return TileType.PrimaryGround;
            
            return TileType.SecondaryGround;
        }
    }
}