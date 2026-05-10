using System.Collections.Generic;
using Systems.Grid.Components;
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

        private readonly Dictionary<Vector2Int, TileData> _tiles = new();
        /// <summary> Provides read-only access to the internal tile storage. </summary>
        public IReadOnlyDictionary<Vector2Int, TileData> Tiles => _tiles;

        /// <summary>
        /// Creates and stores a new TileData object at the specified axial coordinates.
        /// </summary>
        public void CreateTileData(int q, int r)
        {
            TileData tileData = new TileData(q, r);
            _tiles[new Vector2Int(q, r)] = tileData;
        }
        
        /// <summary>
        /// Removes all tile data from the internal storage.
        /// </summary>
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            _tiles.Clear();
        }
        
        /// <summary> Retrieves the TileData at the specified axial coordinates (q, r). </summary>
        public TileData GetTile(int q, int r)
        {
            _tiles.TryGetValue(new Vector2Int(q, r), out TileData tile);
            return tile;
        }
        
        /// <summary> Retrieves the TileData at the specified axial coordinate vector. </summary>
        public TileData GetTile(Vector2Int axialCoord)
        {
            _tiles.TryGetValue(axialCoord, out TileData tile);
            return tile;
        }

        /// <summary>
        /// Retrieves a list of TileData objects within a radius of a center coordinate.
        /// </summary>
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