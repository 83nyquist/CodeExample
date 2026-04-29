using Unity.Mathematics;

// This struct can be used in jobs (no references, only value types)
namespace NPC
{
    public struct BlittableTileData
    {
        public int2 Coordinates;
        public byte MovementCost;     // 255 = blocking, 1-254 = varying costs
        public byte TerrainType;      // For visual variety
        public int NeighborIndices;   // Packed into single int (bitmask or index)
    
        // For pathfinding
        public bool IsWalkable => MovementCost < 255;
    }
}
