using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.NonPlayerCharacters.Structs
{
    [BurstCompile]
    public struct NpcJob : IJobParallelFor
    {
        public NativeArray<NpcData> NpCs;
        public float DeltaTime;
        public float MinInterval;
        public float MaxInterval;
        public uint RandomSeed;
        
        [ReadOnly] public NativeHexGrid Grid;
        [ReadOnly] public NativeHashSet<int2> VisibleTiles;  // Tiles within vision radius
        
        /// <summary>
        /// Executes the NPC simulation for a single index, handling visibility and movement.
        /// </summary>
        public void Execute(int index)
        {
            var npc = NpCs[index];
            
            npc.IsVisible = VisibleTiles.Contains(npc.Position);
            
            npc.Timer -= DeltaTime;
            
            if (npc.Timer <= 0f)
            {
                float randomValue = (GetRandom((uint)index + 1000) % 1000) / 1000f;
                npc.Timer = MinInterval + (randomValue * (MaxInterval - MinInterval));
    
                int2 newPos = GetRandomWalkableNeighbor(npc.Position, index);
                if (!newPos.Equals(npc.Position))
                {
                    npc.PreviousPosition = npc.Position;
                    npc.Position = newPos;
                    npc.IsMoving = true;
                }
                else
                {
                    npc.IsMoving = false;
                }
            }
            else
            {
                if (npc.IsMoving && npc.Position.Equals(npc.PreviousPosition) == false)
                {
                }
                else
                {
                    npc.IsMoving = false;
                }
            }
            
            NpCs[index] = npc;
        }
        
        /// <summary>
        /// Finds a random adjacent walkable tile for the NPC to move to.
        /// </summary>
        private int2 GetRandomWalkableNeighbor(int2 pos, int seed)
        {
            int startDir = (int)(GetRandom((uint)seed) % 6);
            
            for (int i = 0; i < 6; i++)
            {
                int dir = (startDir + i) % 6;
                int2 neighbor = GetNeighbor(pos, dir);
                
                if (IsWalkable(neighbor))
                {
                    return neighbor;
                }
            }
            
            return pos;
        }
        
        /// <summary>
        /// Calculates the neighbor coordinate based on hex direction.
        /// </summary>
        private int2 GetNeighbor(int2 pos, int direction)
        {
            switch (direction)
            {
                case 0: return new int2(pos.x + 1, pos.y);
                case 1: return new int2(pos.x - 1, pos.y);
                case 2: return new int2(pos.x, pos.y + 1);
                case 3: return new int2(pos.x, pos.y - 1);
                case 4: return new int2(pos.x + 1, pos.y - 1);
                case 5: return new int2(pos.x - 1, pos.y + 1);
                default: return pos;
            }
        }
        
        /// <summary>
        /// Calculates a world-space rotation angle between two hex coordinates.
        /// </summary>
        private float GetRotationFromDirection(int2 from, int2 to)
        {
            int2 delta = new int2(to.x - from.x, to.y - from.y);
    
            float worldX = delta.x + delta.y * 0.5f;
            float worldZ = delta.y * 0.8660254f;
    
            float angle = Mathf.Atan2(worldZ, worldX) * Mathf.Rad2Deg;
    
            return (angle + 360f) % 360f;
        }
        
        /// <summary>
        /// Determines if a specific hex coordinate is valid and walkable.
        /// </summary>
        private bool IsWalkable(int2 coord)
        {
            if (!Grid.PositionToIndex.ContainsKey(coord))
                return false;
                
            int idx = Grid.PositionToIndex[coord];
            return Grid.Tiles[idx].IsWalkable;
        }
        
        /// <summary>
        /// Generates a pseudo-random unsigned integer based on a seed and offset.
        /// </summary>
        private uint GetRandom(uint offset)
        {
            uint state = RandomSeed + offset;
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}