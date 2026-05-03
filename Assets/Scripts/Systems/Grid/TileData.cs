using System;
using System.Collections.Generic;
using Core.Enumerations;
using Systems.Decoration;
using UnityEngine;

namespace Systems.Grid
{
    [Serializable]
    public class TileData
    {
        // Coordinates
        [SerializeField] private int x;  // Q coordinate
        [SerializeField] private int z;  // R coordinate
        
        public int X => x;  // Q coordinate
        public int Z => z;  // R coordinate
        
        private Dictionary<Directions.Axial, TileData> _neighbours = new Dictionary<Directions.Axial, TileData>();
        public Dictionary<Directions.Axial, TileData> Neighbours => _neighbours;
        
        //Properties
        public float Elevation { get; set; }
        public float Moisture { get; set; }
        public TileType type;
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
        
        public TileData GetNeighbour(Directions.Axial direction)
        {
            _neighbours.TryGetValue(direction, out TileData neighbour);
            return neighbour;
        }
        
        public void SetNeighbours(Dictionary<Directions.Axial, TileData> neighbours)
        {
            _neighbours = neighbours;
        }
        
        public Dictionary<Directions.Axial, TileData> GetAllNeighbours()
        {
            return new Dictionary<Directions.Axial, TileData>(_neighbours);
        }
        
        public Vector2Int GetNeighborCoordinate(Directions.Axial direction)
        {
            return HexGeometry.GetNeighborCoordinate(x, z, direction);
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
