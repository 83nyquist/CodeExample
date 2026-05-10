using System.Collections.Generic;
using Systems.Grid.Components;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.NonPlayerCharacters.Components
{
    public class NativeGridBuilder
    {
        /// <summary>
        /// Converts a managed dictionary of TileData into a NativeHexGrid for use in Burst-compiled jobs.
        /// </summary>
        /// <param name="tiles">The managed tile data dictionary.</param>
        /// <param name="allocator">The allocation strategy for the native memory.</param>
        /// <returns>A populated NativeHexGrid instance.</returns>
        public NativeHexGrid BuildFromTileData(IReadOnlyDictionary<Vector2Int, TileData> tiles, Allocator allocator)
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