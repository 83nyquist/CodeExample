using System.Collections.Generic;
using Systems.NPC.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.NPC.Components
{
    public class NpcSpawner
    {
        private readonly float _maxMoveInterval;
        
        public NpcSpawner(float maxMoveInterval)
        {
            _maxMoveInterval = maxMoveInterval;
        }
        
        public NativeArray<NpcData> Spawn(int count, NativeHexGrid grid)
        {
            var npcs = new NativeArray<NpcData>(count, Allocator.Persistent);
            var walkableTiles = GetWalkableTiles(grid);
            
            if (walkableTiles.Count == 0)
            {
                Debug.LogError("No walkable tiles found for NPC spawning!");
                return npcs;
            }
            
            for (int i = 0; i < count; i++)
            {
                int2 startPos = walkableTiles[Random.Range(0, walkableTiles.Count)];
                npcs[i] = new NpcData
                {
                    Position = startPos,
                    PreviousPosition = startPos,
                    Timer = Random.Range(0f, _maxMoveInterval),
                    Id = i,
                    IsVisible = false,
                    IsMoving = false
                };
            }
            
            Debug.Log($"Spawned {count} NPCs on {walkableTiles.Count} walkable tiles");
            return npcs;
        }
        
        private List<int2> GetWalkableTiles(NativeHexGrid grid)
        {
            var walkableTiles = new List<int2>();
            
            for (int i = 0; i < grid.Tiles.Length; i++)
            {
                if (grid.Tiles[i].IsWalkable)
                    walkableTiles.Add(grid.Tiles[i].Coordinates);
            }
            
            return walkableTiles;
        }
    }
}