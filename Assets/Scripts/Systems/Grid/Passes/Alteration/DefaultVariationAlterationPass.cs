using System;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class DefaultVariationAlterationPass : BaseAlterationPass
    {
        [Header("DefaultVariationAlterationPass")]
        [SerializeField] private TileSet tileSet;
        public override string PassName => "Default Variation Pass";

        public override void Execute(AxialHexGrid grid, int seed)
        {
            if (tileSet == null) return;

            foreach (var tile in grid.Tiles.Values)
            {
                // If a previous pass (like Mountain Smoothing) already set a variation, skip it.
                if (tile.VariationIndex != -1) continue;

                int variationCount = tileSet.GetVariationCount(tile.type);
                if (variationCount <= 1)
                {
                    tile.VariationIndex = 0;
                    continue;
                }

                // Deterministic random based on coordinates and world seed
                int tileSeed = GetSeed(tile.AxialCoordinates, seed);
                tile.VariationIndex = Mathf.Abs(tileSeed % variationCount);
            }
        }

        private int GetSeed(Vector2Int coords, int globalSeed)
        {
            unchecked {
                return (17 * 31 + coords.x) * 31 + coords.y + globalSeed;
            }
        }
    }
}