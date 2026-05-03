using UnityEngine;

namespace Systems.Decoration.Components
{
    public class TileVariationService : ITileVariationService
    {
        private readonly int _globalSeed;

        public TileVariationService(int globalSeed = 0)
        {
            _globalSeed = globalSeed;
        }

        public GameObject GetPrefab(TileSet tileSet, TileType type, Vector2Int coordinates, bool enableVariations)
        {
            if (tileSet == null) return null;

            if (enableVariations)
            {
                int variationCount = tileSet.GetVariationCount(type);
                if (variationCount > 0)
                {
                    int seed = GetVariationSeed(coordinates);
                    int variationIndex = Mathf.Abs(seed % variationCount);
                    return tileSet.GetTilePrefab(type, variationIndex);
                }
            }

            return tileSet.GetTilePrefab(type, -1);
        }

        public void ApplyVariation(TileDecorator decorator, TileType type, Vector2Int coordinates)
        {
            int seed = GetVariationSeed(coordinates);
            var random = new System.Random(seed);

            if (decorator.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                if (type != TileType.Water)
                {
                    float variation = (float)random.NextDouble() * 0.1f - 0.05f;
                    spriteRenderer.color += new Color(variation, variation, variation, 0);
                }
            }
        }

        private int GetVariationSeed(Vector2Int coordinates)
        {
            unchecked
            {
                return (17 * 31 + coordinates.x) * 31 + coordinates.y + _globalSeed;
            }
        }
    }
}