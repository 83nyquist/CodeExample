using System;
using System.Collections.Generic;
using Game;
using Game.Data;
using NPC.Components;
using NPC.Structs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using Systems.Grid;
using Systems.Decoration;
using Random = UnityEngine.Random;

namespace NPC
{
    public class NpcManager : MonoBehaviour
    {
        [Inject] private AxialHexGrid _hexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        [Inject] private PlayerSettings _playerSettings;
        
        [Header("NPC Settings")]
        [SerializeField] private float minMoveInterval = 1f;
        [SerializeField] private float maxMoveInterval = 3f;
        [SerializeField] private GameObject npcVisualPrefab;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        
        // Core data
        private NativeHexGrid _nativeGrid;
        private NativeArray<NpcData> _npcs;
        
        // Components
        private NativeGridBuilder _gridBuilder;
        private NpcSpawner _spawner;
        private VisionManager _visionManager;
        private NpcVisualRegistry _visualRegistry;
        
        // Job system
        private JobHandle _jobHandle;
        private bool _isJobScheduled;
        
        // Event handling
        private float _eventUpdateTimer;
        private readonly float _eventUpdateInterval = 0.2f;
        private int _cachedVisibleCount;
        
        public int NpcCount => _npcs.IsCreated ? _npcs.Length : 0;
        public event Action<int> OnVisibleAgentsCountChanged;
        public bool IsInitialized => _npcs.IsCreated;
        
        void Awake()
        {
            InitializeComponents();
            _hexGrid.OnGridGenerated += OnGridGenerated;
        }
        
        void OnDestroy()
        {
            if (_hexGrid != null)
                _hexGrid.OnGridGenerated -= OnGridGenerated;
                
            CompleteJob();
            CleanupResources();
        }
        
        void Update()
        {
            if (!_npcs.IsCreated) return;
            
            CompleteJobIfFinished();
            ScheduleMovementJob();
        }
        
        void LateUpdate()
        {
            if (!_npcs.IsCreated) return;
            
            CompleteJob();
            UpdateVisuals();
            UpdateVisibleCountEvent();
        }
        
        private void InitializeComponents()
        {
            _gridBuilder = new NativeGridBuilder();
            _spawner = new NpcSpawner(maxMoveInterval);
            _visionManager = new VisionManager(_worldDecorator, _hexGrid.Tiles?.Count ?? 100);
            _visualRegistry = new NpcVisualRegistry(npcVisualPrefab, moveSpeed, rotationSpeed, transform);
        }
        
        private void OnGridGenerated(Dictionary<Vector2Int, TileData> tiles)
        {
            // Complete any running job first
            CompleteJob();

            // Clean up ALL existing resources
            CleanupResources();

            // RECREATE vision manager (don't rebuild disposed one)
            _visionManager = new VisionManager(_worldDecorator, tiles.Count);

            // Build new grid and spawn
            _nativeGrid = _gridBuilder.BuildFromTileData(tiles, Allocator.Persistent);
            _npcs = _spawner.Spawn(_playerSettings.populationSize, _nativeGrid);
            _visualRegistry.CreateVisuals(_npcs, HexToWorld);

            // Reset event cache
            _cachedVisibleCount = -1;
            _eventUpdateTimer = 0;
        }
        
        private void ScheduleMovementJob()
        {
            var visibleTiles = _visionManager.GetVisibleTiles(Time.time);
            
            var job = new NpcJob
            {
                NPCs = _npcs,
                DeltaTime = Time.deltaTime,
                MinInterval = minMoveInterval,
                MaxInterval = maxMoveInterval,
                RandomSeed = (uint)Random.Range(1, 999999),
                Grid = _nativeGrid,
                VisibleTiles = visibleTiles
            };
            
            _jobHandle = job.Schedule(_npcs.Length, 64);
            _isJobScheduled = true;
            JobHandle.ScheduleBatchedJobs();
        }
        
        private void UpdateVisuals()
        {
            _visualRegistry.UpdateVisuals(_npcs, HexToWorld, Time.deltaTime);
        }
        
        private void UpdateVisibleCountEvent()
        {
            // Don't try to read if a job is still writing to _npcs
            if (_isJobScheduled && !_jobHandle.IsCompleted) return;
    
            _eventUpdateTimer += Time.deltaTime;
            if (_eventUpdateTimer < _eventUpdateInterval) return;
    
            _eventUpdateTimer = 0;
    
            int newCount = CalculateVisibleCount();
            if (newCount != _cachedVisibleCount)
            {
                _cachedVisibleCount = newCount;
                OnVisibleAgentsCountChanged?.Invoke(newCount);
            }
        }
        
        private int CalculateVisibleCount()
        {
            int count = 0;
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i].IsVisible) count++;
            }
            return count;
        }
        
        private void CompleteJobIfFinished()
        {
            if (_isJobScheduled && _jobHandle.IsCompleted)
            {
                _jobHandle.Complete();
                _isJobScheduled = false;
            }
        }
        
        private void CompleteJob()
        {
            if (_isJobScheduled)
            {
                _jobHandle.Complete();
                _isJobScheduled = false;
            }
        }
        
        private void CleanupResources()
        {
            // Dispose native containers
            if (_npcs.IsCreated) 
            {
                _npcs.Dispose();
            }
    
            if (_nativeGrid.Tiles.IsCreated) 
            {
                _nativeGrid.Dispose();
            }
    
            // Clean up vision
            _visionManager?.Dispose();
    
            // This should destroy all visual GameObjects
            _visualRegistry?.Dispose();
    
            // Reset job state
            _isJobScheduled = false;
        }
        
        private Vector3 HexToWorld(int2 coord)
        {
            return _hexGrid.AxialToWorld(coord.x, coord.y);
        }
    }
}