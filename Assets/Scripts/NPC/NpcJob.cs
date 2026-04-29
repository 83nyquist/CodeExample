using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace NPC
{
    [BurstCompile]
    public struct NpcJob : IJobParallelFor
    {
        public NativeArray<NpcData> NPCs;
        public float DeltaTime;
        public float MinInterval;
        public float MaxInterval;
        public uint RandomSeed;
        
        [ReadOnly] public NativeHexGrid Grid;
        [ReadOnly] public NativeHashSet<int2> VisibleTiles;  // Tiles within vision radius
        
        public void Execute(int index)
        {
            var npc = NPCs[index];
            
            // Check visibility based on whether NPC position is in visible tiles set
            npc.IsVisible = VisibleTiles.Contains(npc.Position);
            
            // Timer and movement logic (always runs, even when not visible)
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
                    npc.IsMoving = true;  // Start moving
                }
                else
                {
                    npc.IsMoving = false; // Couldn't move
                }
            }
            else
            {
                // Check if arrived at destination
                if (npc.IsMoving && npc.Position.Equals(npc.PreviousPosition) == false)
                {
                    // Still moving, keep IsMoving true
                }
                else
                {
                    npc.IsMoving = false; // Not moving this frame
                }
            }
            
            NPCs[index] = npc;
        }
        
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
        
        private float GetRotationFromDirection(int2 from, int2 to)
        {
            int2 delta = new int2(to.x - from.x, to.y - from.y);
    
            // Axial to world coordinates
            float worldX = delta.x + delta.y * 0.5f;
            float worldZ = delta.y * 0.8660254f;
    
            // Calculate angle
            float angle = Mathf.Atan2(worldZ, worldX) * Mathf.Rad2Deg;
    
            // Return normalized angle (0-360)
            return (angle + 360f) % 360f;
        }
        
        private bool IsWalkable(int2 coord)
        {
            if (!Grid.PositionToIndex.ContainsKey(coord))
                return false;
                
            int idx = Grid.PositionToIndex[coord];
            return Grid.Tiles[idx].IsWalkable;
        }
        
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