using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Systems.NPC.Structs
{
    public struct NativeHexGrid : IDisposable
    {
        public NativeArray<BlittableTileData> Tiles;
        public NativeHashMap<int2, int> PositionToIndex; // Maps axial coords to array index
        public int GridWidth;
        public int GridHeight;
    
        public NativeHexGrid(int capacity, Allocator allocator)
        {
            Tiles = new NativeArray<BlittableTileData>(capacity, allocator);
            PositionToIndex = new NativeHashMap<int2, int>(capacity, allocator);
            GridWidth = 0;
            GridHeight = 0;
        }
    
        public void Dispose()
        {
            if (Tiles.IsCreated) Tiles.Dispose();
            if (PositionToIndex.IsCreated) PositionToIndex.Dispose();
        }
    }
}