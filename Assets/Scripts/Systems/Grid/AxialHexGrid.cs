using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Grid
{
    /// <summary>
    /// Pure Data Repository for the Hex Grid.
    /// Responsible only for storage and spatial queries.
    /// </summary>
    public class AxialHexGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float hexSize = 1.05f;

        // Core grid storage
        private readonly Dictionary<Vector2Int, TileData> _tiles = new();
        public IReadOnlyDictionary<Vector2Int, TileData> Tiles => _tiles;

        public void CreateTileData(int q, int r)
        {
            TileData tileData = new TileData(q, r);
            _tiles[new Vector2Int(q, r)] = tileData;
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            _tiles.Clear();
        }
        
        public TileData GetTile(int q, int r)
        {
            _tiles.TryGetValue(new Vector2Int(q, r), out TileData tile);
            return tile;
        }
        
        public TileData GetTile(Vector2Int axialCoord)
        {
            _tiles.TryGetValue(axialCoord, out TileData tile);
            return tile;
        }

        public List<TileData> GetTilesInRadius(Vector2Int center, int radius)
        {
            List<TileData> results = new List<TileData>();

            foreach (Vector2Int relCoord in HexGeometry.GetCoordinatesInRingRange(0, radius))
            {
                Vector2Int absoluteCoord = center + relCoord;
                TileData tile = GetTile(absoluteCoord);
                if (tile != null) results.Add(tile);
            }
            return results;
        }
    }
}