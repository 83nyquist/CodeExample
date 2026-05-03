using System;
using System.Collections.Generic;
using Systems.Decoration;
using Systems.Grid;
using Systems.NPC.Structs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Systems.NPC.Components
{
    /// <summary>
    /// SRP: Manages Native memory and Job System lifecycle.
    /// </summary>
    public class NpcSimulationSystem : IDisposable
    {
        private NativeHexGrid _nativeGrid;
        private NativeArray<NpcData> _npcs;
        private VisionManager _visionManager;
        private JobHandle _jobHandle;
        private bool _isJobScheduled;
        
        private readonly float _minInterval;
        private readonly float _maxInterval;

        public NativeArray<NpcData> Data => _npcs;
        public int NpcCount => _npcs.IsCreated ? _npcs.Length : 0;
        public bool IsActive { get; private set; }

        public NpcSimulationSystem(float min, float max)
        {
            _minInterval = min;
            _maxInterval = max;
        }

        public void Reset(Dictionary<Vector2Int, TileData> tiles, WorldDecorator decorator)
        {
            IsActive = false;
            // Dispose is safe to call even if already disposed by CleanupActiveSimulation
            Dispose(); 
            
            _visionManager = new VisionManager(decorator, tiles.Count);
            _nativeGrid = new NativeGridBuilder().BuildFromTileData(tiles, Allocator.Persistent);
        }

        public void InitializeData(int count)
        {
            _npcs = new NpcSpawner(_maxInterval).Spawn(count, _nativeGrid);
        }

        public void Activate() => IsActive = true;

        public void Update()
        {
            if (!IsActive) return;

            if (_isJobScheduled && _jobHandle.IsCompleted)
            {
                _jobHandle.Complete();
                _isJobScheduled = false;
            }

            if (!_isJobScheduled)
            {
                var job = new NpcJob
                {
                    NPCs = _npcs,
                    DeltaTime = Time.deltaTime,
                    MinInterval = _minInterval,
                    MaxInterval = _maxInterval,
                    RandomSeed = (uint)UnityEngine.Random.Range(1, 999999),
                    Grid = _nativeGrid,
                    VisibleTiles = _visionManager.GetVisibleTiles(Time.time)
                };
                _jobHandle = job.Schedule(_npcs.Length, 64);
                _isJobScheduled = true;
                JobHandle.ScheduleBatchedJobs();
            }
        }

        public void CompleteCurrentJob()
        {
            if (_isJobScheduled)
            {
                _jobHandle.Complete();
                _isJobScheduled = false;
            }
        }

        public void Dispose()
        {
            CompleteCurrentJob();
            if (_npcs.IsCreated) _npcs.Dispose();
            if (_nativeGrid.Tiles.IsCreated) _nativeGrid.Dispose();
            _visionManager?.Dispose();
            IsActive = false;
        }
    }
}
