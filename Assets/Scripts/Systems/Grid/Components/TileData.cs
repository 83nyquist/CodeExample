using System;
using Core.Enumerations;
using Systems.Decoration.Components;
using UnityEngine;

namespace Systems.Grid.Components
{
    [Serializable]
    public class TileData
    {
        // Coordinates
        [SerializeField] private int x;  // Q coordinate
        [SerializeField] private int z;  // R coordinate
        
        public int X => x;  // Q coordinate
        public int Z => z;  // R coordinate
        
        [NonSerialized] private TileData[] _neighbours = new TileData[6];
        public TileData[] Neighbours => _neighbours;
        
        //Properties
        public float Elevation { get; set; }
        public float Moisture { get; set; }
        public TileType type;
        public int VariationIndex { get; set; } = -1;
        public Vector3 Rotation { get; set; }

        public TileDecorator Decorator { get; private set;}

        public Vector2Int AxialCoordinates => new Vector2Int(x, z);
        // public Vector3 WorldCoordinates => grid.AxialToWorld(X, Z);
        public Vector3Int CubeCoordinates => AxialToCube(x, z);
        
        public bool IsWalkable => type != TileType.Water && 
                                  type != TileType.Forest && 
                                  type != TileType.Mountain;
        
        public TileData(int q, int r)
        {
            // this.grid = grid;
            x = q;
            z = r;
        }
        
        /// <summary>
        /// Gets neighbor by clockwise index (0-5)
        /// </summary>
        public TileData GetNeighbour(int directionIndex)
        {
            if (directionIndex < 0 || directionIndex >= 6) return null;
            return _neighbours[directionIndex];
        }
        
        public void SetNeighbours(TileData[] neighbours)
        {
            if (neighbours.Length != 6) return;
            _neighbours = neighbours;
        }

        public Vector2Int GetNeighborCoordinate(int directionIndex)
        {
            return HexGeometry.GetNeighborCoordinate(x, z, (Directions.Axial)directionIndex);
        }

        public void SetDecorator(TileDecorator decorator)
        {
            Decorator = decorator;
        }
        
        private Vector3Int AxialToCube(int q, int r)
        {
            return new Vector3Int(q, -q - r, r);
        }
        
        public float DistanceTo(TileData other)
        {
            if (other == null) return -1;
            return DistanceTo(other.x, other.z);
        }

        public float DistanceTo(Vector2Int coord)
        {
            return DistanceTo(coord.x, coord.y);
        }
        
        public float DistanceTo(int q, int r)
        {
            Vector3Int cube1 = AxialToCube(x, z);
            Vector3Int cube2 = AxialToCube(q, r);
            return (Math.Abs(cube1.x - cube2.x) + Math.Abs(cube1.y - cube2.y) + Math.Abs(cube1.z - cube2.z)) / 2f;
        }
        
        public override string ToString()
        {
            return $"GridData ({x}, {z})";
        }
    }
}
