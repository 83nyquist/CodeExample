using System;
using Systems.Decoration.Components;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [Serializable]
    public class DefaultVariationAlterationPass : BaseAlterationPass
    {
        [Header("DefaultVariationAlterationPass")]
        [SerializeField] private TileSet tileSet;
        public override string PassName => "Default Variation Pass";

        /// <summary>
        /// Ensures every tile has a valid variation index, utilizing a coordinate-based seed if no variation is set.
        /// </summary>
        public override void Execute(AxialHexGrid grid, int seed)
        {
            if (tileSet == null) return;

            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.VariationIndex != -1) continue;

                int variationCount = tileSet.GetVariationCount(tile.type);
                if (variationCount <= 1)
                {
                    tile.VariationIndex = 0;
                    continue;
                }

                int tileSeed = GetSeed(tile.AxialCoordinates, seed);
                tile.VariationIndex = Mathf.Abs(tileSeed % variationCount);
            }
        }

        /// <summary>
        /// Generates a deterministic integer seed based on axial coordinates and a global seed.
        /// </summary>
        private int GetSeed(Vector2Int coords, int globalSeed)
        {
            unchecked {
                return (17 * 31 + coords.x) * 31 + coords.y + globalSeed;
            }
        }
    }
}