using UnityEngine;

namespace Systems.Decoration.Components
{
    public interface ITileVariationService
    {
        GameObject GetPrefab(TileSet tileSet, TileType type, Vector2Int coordinates, bool enableVariations);
        void ApplyVariation(TileDecorator decorator, TileType type, Vector2Int coordinates);
    }
}