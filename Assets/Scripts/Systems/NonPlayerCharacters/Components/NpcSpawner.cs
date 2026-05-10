using System.Collections.Generic;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.NonPlayerCharacters.Components
{
    public class NpcSpawner
    {
        private readonly float _maxMoveInterval;
        
        /// <summary>
        /// Initializes the spawner with the maximum movement interval for NPCs.
        /// </summary>
        public NpcSpawner(float maxMoveInterval)
        {
            _maxMoveInterval = maxMoveInterval;
        }
        
        /// <summary>
        /// Spawns a specified number of NPCs at random walkable positions on the grid.
        /// </summary>
        /// <param name="count">Number of NPCs to spawn.</param>
        /// <param name="grid">The grid used to validate walkable positions.</param>
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
            
            return npcs;
        }
        
        /// <summary>
        /// Scans the native grid to find all coordinates that are marked as walkable.
        /// </summary>
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