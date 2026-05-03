using Systems.Decoration.Components;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [System.Serializable]
    public class ForestAlterationPass : BaseAlterationPass
    {
        [Header("Forest Settings")]
        [Tooltip("The number of different visual variations available for Forest tiles in the TileSet.")]
        [SerializeField] private int variationCount = 3;

        public override string PassName => "Forest Variation Pass";

        public override void Execute(AxialHexGrid grid, int seed)
        {
            // Seeded random ensures deterministic map generation
            System.Random random = new System.Random(seed);
            int forestTilesProcessed = 0;

            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.type == TileType.Forest)
                {
                    tile.VariationIndex = random.Next(0, variationCount);
                    forestTilesProcessed++;
                    
                    // To retain "pointy side up" orientation, we rotate in 60-degree increments.
                    // Since the grid uses X and Z coordinates, we rotate around the Y-axis.
                    float yRotation = random.Next(0, 6) * 60f;
                    tile.Rotation = new Vector3(0, yRotation, 0);
                }
            }

            if (debugLog)
            {
                Debug.Log($"[{PassName}] Randomized {forestTilesProcessed} forest tiles using {variationCount} variations.");
            }
        }
    }
}
