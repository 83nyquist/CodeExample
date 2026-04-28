using UnityEngine;
using System.Collections.Generic;

namespace Systems.Grid
{
    public static class AxialHexGridExtensions
    {
        /// <summary>
        /// Given an origin and radius will return a list of coordinates in that radius 
        /// </summary>
        public static List<TileData> GetTilesInRadius(this AxialHexGrid grid, Vector2Int center, int radius)
        {
            List<TileData> tilesInRadius = new List<TileData>();

            foreach (Vector2Int coord in GetCoordinatesInRadius(grid, center, radius))
            {
                tilesInRadius.Add(grid.GetTile(coord));
            }
    
            return tilesInRadius;
        }
        
        /// <summary>
        /// Given an origin and radius will return a list of coordinates in that radius 
        /// </summary>
        public static List<Vector2Int> GetCoordinatesInRadius(this AxialHexGrid grid, Vector2Int center, int radius)
        {
            var tilesInRadius = new List<Vector2Int>();
    
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
        
                for (int r = r1; r <= r2; r++)
                {
                    Vector3Int centerCube = grid.AxialToCube(center.x, center.y);
                    Vector3Int candidateCube = grid.AxialToCube(center.x + q, center.y + r);
                    int distance = (Mathf.Abs(centerCube.x - candidateCube.x) +
                                    Mathf.Abs(centerCube.y - candidateCube.y) +
                                    Mathf.Abs(centerCube.z - candidateCube.z)) / 2;
            
                    if (distance <= radius)
                    {
                        tilesInRadius.Add(new Vector2Int(center.x + q, center.y + r));
                    }
                }
            }
    
            return tilesInRadius;
        }
        
        /// <summary>
        /// Converts axial coordinates to world position (pointy-top hex grid)
        /// </summary>
        public static Vector3 AxialToWorld(this AxialHexGrid grid, int q, int r)
        {
            float x = grid.hexSize * 1.732f * (q + r * 0.5f);
            float z = grid.hexSize * 1.5f * r;
            return new Vector3(x, 0, z);
        }
        
        /// <summary>
        /// Calculates world position bounds for camera framing
        /// </summary>
        public static Bounds GetGridBounds(this AxialHexGrid grid)
        {
            if (grid.Tiles.Count == 0) return new Bounds(Vector3.zero, Vector3.one);
            
            Vector3 firstTilePos = grid.AxialToWorld(0, 0);
            Bounds bounds = new Bounds(firstTilePos, Vector3.zero);
            
            foreach (var tile in grid.Tiles.Values)
            {
                Vector3 tilePos = grid.AxialToWorld(tile.X, tile.Z);
                bounds.Encapsulate(tilePos);
            }
            return bounds;
        }
        
        /// <summary>
        /// Converts world position to axial coordinates
        /// </summary>
        public static Vector2Int WorldToAxial(this AxialHexGrid grid, Vector3 worldPosition)
        {
            float q = (worldPosition.x * 0.57735f - worldPosition.z / 3f) / grid.hexSize;
            float r = (worldPosition.z * 2f / 3f) / grid.hexSize;
            return grid.HexRound(new Vector2(q, r));
        }
        
        /// <summary>
        /// Rounds fractional hex coordinates to the nearest axial coordinate
        /// </summary>
        private static Vector2Int HexRound(this AxialHexGrid grid, Vector2 fractional)
        {
            int q = Mathf.RoundToInt(fractional.x);
            int r = Mathf.RoundToInt(fractional.y);
            int s = Mathf.RoundToInt(-fractional.x - fractional.y);
            
            float qDiff = Mathf.Abs(q - fractional.x);
            float rDiff = Mathf.Abs(r - fractional.y);
            float sDiff = Mathf.Abs(s - (-fractional.x - fractional.y));
            
            if (qDiff > rDiff && qDiff > sDiff)
                q = -r - s;
            else if (rDiff > sDiff)
                r = -q - s;
            
            return new Vector2Int(q, r);
        }
        
        /// <summary>
        /// Converts axial to cube coordinates
        /// </summary>
        public static Vector3Int AxialToCube(this AxialHexGrid grid, int q, int r)
        {
            return new Vector3Int(q, -q - r, r);
        }
        
        /// <summary>
        /// Gets neighbor coordinate in specified direction (0-5)
        /// </summary>
        public static Vector2Int GetNeighborCoordinate(this AxialHexGrid grid, Vector2Int axial, int directionIndex)
        {
            Vector2Int[] axialDirections = new Vector2Int[]
            {
                new Vector2Int(1, 0),   // East
                new Vector2Int(0, 1),   // Southeast
                new Vector2Int(-1, 1),  // Southwest
                new Vector2Int(-1, 0),  // West
                new Vector2Int(0, -1),  // Northwest
                new Vector2Int(1, -1)   // Northeast
            };
            
            Vector2Int dir = axialDirections[directionIndex % 6];
            return new Vector2Int(axial.x + dir.x, axial.y + dir.y);
        }
        
        /// <summary>
        /// Gets all neighboring coordinates for a given axial coordinate
        /// </summary>
        public static List<Vector2Int> GetNeighborCoordinates(this AxialHexGrid grid, int q, int r)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            for (int i = 0; i < 6; i++)
            {
                neighbors.Add(grid.GetNeighborCoordinate(new Vector2Int(q, r), i));
            }
            return neighbors;
        }
        
        /// <summary>
        /// Gets all tiles within a certain radius from center
        /// </summary>
        public static List<TileData> GetTilesInRadius(this AxialHexGrid grid, int centerQ, int centerR, int radius)
        {
            List<TileData> result = new List<TileData>();
            
            for (int q = -radius; q <= radius; q++)
            {
                for (int r = -radius; r <= radius; r++)
                {
                    if (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(-q - r) <= radius * 2)
                    {
                        TileData tile = grid.GetTile(centerQ + q, centerR + r);
                        if (tile != null) result.Add(tile);
                    }
                }
            }
            return result;
        }
        
        /// <summary>
        /// Gets distance between two hex tiles in axial coordinates
        /// </summary>
        public static int GetDistance(this AxialHexGrid grid, int q1, int r1, int q2, int r2)
        {
            Vector3Int cube1 = grid.AxialToCube(q1, r1);
            Vector3Int cube2 = grid.AxialToCube(q2, r2);
            
            return (Mathf.Abs(cube1.x - cube2.x) + 
                    Mathf.Abs(cube1.y - cube2.y) + 
                    Mathf.Abs(cube1.z - cube2.z)) / 2;
        }
        
        /// <summary>
        /// Gets tile count (for debugging)
        /// </summary>
        public static int GetTileCount(this AxialHexGrid grid)
        {
            return grid.Tiles.Count;
        }
        
        /// <summary>
        /// Checks if the grid is empty
        /// </summary>
        public static bool IsEmpty(this AxialHexGrid grid)
        {
            return grid.Tiles.Count == 0;
        }
    }
}