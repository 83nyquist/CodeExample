using System;
using Core.Enumerations;
using Data;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Components
{
    [Serializable]
    public class TileData
    {
        [SerializeField] private int x;
        [SerializeField] private int z;
        
        public int X => x;
        public int Z => z;
        
        [NonSerialized] private TileData[] _neighbours = new TileData[6];
        public TileData[] Neighbours => _neighbours;
        
        public float Elevation { get; set; }
        public float Moisture { get; set; }
        public Enumerations.TileType type;
        public bool IsDiscovered { get; set; }
        public bool IsInVision { get; set; }
        public int VariationIndex { get; set; } = -1;
        public Vector3 Rotation { get; set; }

        public TileDecorator Decorator { get; private set;}

        public Vector2Int AxialCoordinates => new Vector2Int(x, z);
        public Vector3Int CubeCoordinates => AxialToCube(x, z);
        
        public bool IsWalkable => type != Enumerations.TileType.Water && 
                                  type != Enumerations.TileType.Forest && 
                                  type != Enumerations.TileType.Mountain;
        
        /// <summary>
        /// Initializes a new tile data instance at the specified axial coordinates.
        /// </summary>
        public TileData(int q, int r)
        {
            x = q;
            z = r;
        }
        
        /// <summary>
        /// Retrieves a neighbor tile by its clockwise index (0-5).
        /// </summary>
        public TileData GetNeighbour(int directionIndex)
        {
            if (directionIndex < 0 || directionIndex >= 6) return null;
            return _neighbours[directionIndex];
        }
        
        /// <summary>
        /// Sets the array of neighboring tiles.
        /// </summary>
        public void SetNeighbours(TileData[] neighbours)
        {
            if (neighbours.Length != 6) return;
            _neighbours = neighbours;
        }

        /// <summary>
        /// Calculates the coordinate of a neighbor using the HexGeometry utility.
        /// </summary>
        public Vector2Int GetNeighborCoordinate(int directionIndex)
        {
            return HexGeometry.GetNeighborCoordinate(x, z, (Directions.Axial)directionIndex);
        }

        /// <summary>
        /// Associates a visual decorator with this tile data.
        /// </summary>
        public void SetDecorator(TileDecorator decorator)
        {
            Decorator = decorator;
        }
        
        /// <summary>
        /// Converts axial coordinates (Q, R) to cube coordinates (Q, S, R).
        /// </summary>
        private Vector3Int AxialToCube(int q, int r)
        {
            return new Vector3Int(q, -q - r, r);
        }
        
        /// <summary>
        /// Calculates the hex distance to another TileData instance.
        /// </summary>
        public float DistanceTo(TileData other)
        {
            if (other == null) return -1;
            return DistanceTo(other.x, other.z);
        }

        /// <summary>
        /// Calculates the hex distance to a specific axial coordinate.
        /// </summary>
        public float DistanceTo(Vector2Int coord)
        {
            return DistanceTo(coord.x, coord.y);
        }
        
        /// <summary>
        /// Calculates the hex distance to specified Q and R coordinates.
        /// </summary>
        public float DistanceTo(int q, int r)
        {
            Vector3Int cube1 = AxialToCube(x, z);
            Vector3Int cube2 = AxialToCube(q, r);
            return (Math.Abs(cube1.x - cube2.x) + Math.Abs(cube1.y - cube2.y) + Math.Abs(cube1.z - cube2.z)) / 2f;
        }
        
        /// <summary>
        /// Returns a string representation of the tile's coordinates.
        /// </summary>
        public override string ToString()
        {
            return $"GridData ({x}, {z})";
        }
    }
}
