using System;
using System.Collections.Generic;
using Systems.Decoration;
using Systems.Grid.Components;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Systems.NonPlayerCharacters.Components
{
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

        /// <summary>
        /// Initializes the simulation system with timing constraints.
        /// </summary>
        public NpcSimulationSystem(float min, float max)
        {
            _minInterval = min;
            _maxInterval = max;
        }

        /// <summary>
        /// Resets the simulation by disposing existing native data and rebuilding the grid from tile data.
        /// </summary>
        public void Reset(IReadOnlyDictionary<Vector2Int, TileData> tiles, WorldDecorator decorator)
        {
            IsActive = false;
            Dispose(); 
            
            _visionManager = new VisionManager(decorator, tiles.Count);
            _nativeGrid = new NativeGridBuilder().BuildFromTileData(tiles, Allocator.Persistent);
        }

        /// <summary>
        /// Allocates and populates the native NPC data array.
        /// </summary>
        public void InitializeData(int count)
        {
            _npcs = new NpcSpawner(_maxInterval).Spawn(count, _nativeGrid);
        }

        /// <summary>
        /// Enables the simulation logic.
        /// </summary>
        public void Activate() => IsActive = true;

        /// <summary>
        /// Manages the job lifecycle, completing previous frames and scheduling new movement/visibility jobs.
        /// </summary>
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
                    NpCs = _npcs,
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

        /// <summary>
        /// Forces the completion of the currently running NPC simulation job.
        /// </summary>
        public void CompleteCurrentJob()
        {
            if (_isJobScheduled)
            {
                _jobHandle.Complete();
                _isJobScheduled = false;
            }
        }

        /// <summary>
        /// Disposes of all native arrays and unmanaged memory used by the simulation.
        /// </summary>
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
