using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using Systems.Grid;
using Systems.Decoration;
using Systems.NPC.Components;
using Systems.NPC.Structs;

namespace NPC
{
    public class NpcManager : MonoBehaviour
    {
        // Dependencies (DIP)
        [Inject] private AxialHexGrid _hexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private GenerationProgressTracker _progressTracker;
        
        [Header("Simulation Settings")]
        [SerializeField] private float minMoveInterval = 1f;
        [SerializeField] private float maxMoveInterval = 3f;
        [SerializeField] private float visibilityUpdateInterval = 0.2f;

        [Header("Visual Settings")]
        [SerializeField] private GameObject npcVisualPrefab;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        
        // Sub-Systems (SRP)
        private NpcSimulationSystem _simulation;
        private NpcVisualRegistry _visuals;
        private NpcVisibilityTracker _visibilityTracker;
        
        private Coroutine _spawnCoroutine;

        // Properties
        public int NpcCount => _simulation?.NpcCount ?? 0;
        public bool IsInitialized => _simulation?.IsActive ?? false;

        // Events
        public event Action<int> OnVisibleAgentsCountChanged;
        public event Action OnComplete;
        
        void Awake()
        {
            InitializeComponents();
            _hexGrid.OnGridStarted += CleanupActiveSimulation; // Signal to stop everything
            _hexGrid.OnGridGenerated += OnGridGenerated;
        }
        
        void OnDestroy()
        {
            _hexGrid.OnGridStarted -= CleanupActiveSimulation;
            _hexGrid.OnGridGenerated -= OnGridGenerated;
            _simulation?.Dispose();
            _visuals?.Dispose();
        }
        
        void Update()
        {
            if (!IsInitialized) return;
            _simulation.Update();
        }
        
        void LateUpdate()
        {
            if (!IsInitialized) return;
            
            _simulation.CompleteCurrentJob();
            _visuals.UpdateVisuals(_simulation.Data, HexToWorld, Time.deltaTime);
            _visibilityTracker.Process(_simulation.Data, Time.deltaTime);
        }
        
        private void InitializeComponents()
        {
            _simulation = new NpcSimulationSystem(minMoveInterval, maxMoveInterval);
            _visuals = new NpcVisualRegistry(npcVisualPrefab, moveSpeed, rotationSpeed, transform);
            _visibilityTracker = new NpcVisibilityTracker(visibilityUpdateInterval);
            
            _visibilityTracker.OnCountChanged += (count) => OnVisibleAgentsCountChanged?.Invoke(count);
        }
        
        private void OnGridGenerated(Dictionary<Vector2Int, TileData> tiles)
        {
            _simulation.Reset(tiles, _worldDecorator);
            _spawnCoroutine = StartCoroutine(SpawnNpcsRoutine(tiles));
        }

        /// <summary>
        /// Ensures all existing NPCs (Data and Visuals) are destroyed 
        /// before starting a new simulation cycle.
        /// </summary>
        private void CleanupActiveSimulation()
        {
            // 1. Stop any spawning currently in progress
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            // Stop simulation logic and free native memory immediately
            _simulation?.Dispose();

            // 2. Clear visual GameObjects
            // Note: We call Dispose to destroy objects, then we must re-prepare the registry
            _visuals?.Dispose();
            _visuals = new NpcVisualRegistry(npcVisualPrefab, moveSpeed, rotationSpeed, transform);
        }

        private IEnumerator SpawnNpcsRoutine(Dictionary<Vector2Int, TileData> tiles)
        {
            int count = _playerSettings.populationSize;
            _simulation.InitializeData(count);
            _visuals.PrepareRegistry(count);

            int batchSize = 50; 
            for (int i = 0; i < count; i += batchSize)
            {
                int batch = Mathf.Min(batchSize, count - i);
                var slice = new NativeSlice<NpcData>(_simulation.Data, i, batch);
                
                _visuals.CreateVisualsInRange(slice, i, HexToWorld);
                _progressTracker.UpdateProgress(WorkUnitTypes.Agents, batch);
                yield return null;
            }

            _simulation.Activate();
            OnComplete?.Invoke();
            _spawnCoroutine = null;
        }
        
        private Vector3 HexToWorld(int2 coord)
        {
            return _hexGrid.AxialToWorld(coord.x, coord.y);
        }
    }
}