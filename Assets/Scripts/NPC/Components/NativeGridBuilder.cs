using System.Collections.Generic;
using NPC.Structs;
using Systems.Grid;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NPC.Components
{
    public class NativeGridBuilder
    {
        public NativeHexGrid BuildFromTileData(Dictionary<Vector2Int, TileData> tiles, Allocator allocator)
        {
            var nativeGrid = new NativeHexGrid(tiles.Count, allocator);
            
            int index = 0;
            foreach (var kvp in tiles)
            {
                TileData tile = kvp.Value;
                
                nativeGrid.Tiles[index] = new BlittableTileData
                {
                    Coordinates = new int2(tile.X, tile.Z),
                    MovementCost = tile.IsWalkable ? (byte)1 : byte.MaxValue,
                    TerrainType = (byte)tile.type,
                    NeighborIndices = 0
                };
                
                nativeGrid.PositionToIndex.Add(new int2(tile.X, tile.Z), index);
                index++;
            }
            
            return nativeGrid;
        }
    }
}