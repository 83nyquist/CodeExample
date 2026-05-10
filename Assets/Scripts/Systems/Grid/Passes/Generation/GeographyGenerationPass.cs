using System;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;

namespace Systems.Grid.Passes.Generation
{
    [Serializable]
    public class GeographyGenerationPass : BaseGenerationPass
    {
        public float elevationScale = 0.1f;
        public float moistureScale = 0.15f;
        public float elevationOffset = 0f;

        public override string PassName => "Geography (Noise) Pass";

        /// <summary>
        /// Generates raw elevation and moisture noise data for every tile in the grid.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            foreach (var tile in grid.Tiles.Values)
            {
                tile.Elevation = GetNoise(tile.X, tile.Z, elevationScale, seed, elevationOffset);
                tile.Moisture = GetNoise(tile.X, tile.Z, moistureScale, seed + 1000, 0);
            }
        }

        /// <summary>
        /// Samples Perlin noise at a specific coordinate using scale, seed, and offset.
        /// </summary>
        private float GetNoise(int x, int y, float scale, int seed, float offset)
        {
            float xf = x * scale;
            float yf = y * scale;
            float val = Mathf.PerlinNoise(xf + seed, yf + seed);
            return Mathf.Clamp01(val + offset);
        }
    }
}