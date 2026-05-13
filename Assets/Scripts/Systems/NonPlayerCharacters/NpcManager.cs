using System.Collections;
using System.Collections.Generic;
using Coordinators;
using Data;
using Systems.Decoration;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using Systems.NonPlayerCharacters.Components;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Systems.NonPlayerCharacters
{
    public class NpcManager : EventBusSubscriber
    {
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        [Inject] private PlayerSettings _playerSettings;
        
        [Header("Simulation Settings")]
        [SerializeField] private float minMoveInterval = 1f;
        [SerializeField] private float maxMoveInterval = 3f;
        [SerializeField] private float visibilityUpdateInterval = 0.2f;

        [Header("Visual Settings")]
        [SerializeField] private GameObject npcVisualPrefab;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        
        private NpcSimulationSystem _simulation;
        private NpcVisualRegistry _visuals;
        private NpcVisibilityTracker _visibilityTracker;
        
        private HashSet<TileData> _currentVisionSet = new();
        private bool _isVisibilityForced;
        private int _lastShroudedCount;
        private Coroutine _spawnCoroutine;

        /// <summary>
        /// Gets the total number of NPCs currently in the simulation.
        /// </summary>
        public int NpcCount => _simulation?.NpcCount ?? 0;
        /// <summary>
        /// Gets whether the simulation system is currently active and initialized.
        /// </summary>
        public bool IsInitialized => _simulation?.IsActive ?? false;
        
        /// <summary>
        /// Initializes sub-systems and subscribes to world generation events.
        /// </summary>
        void Start()
        {
            InitializeComponents();
            Subscribe<WorldGenerationStartedEvent>(CleanupActiveSimulation);
            Subscribe<GridInitializationFinishedEvent>(InitializeNpcs);
        }

        /// <summary>
        /// Cleans up sub-systems and disposes of native memory.
        /// </summary>
        protected override void OnDestroy()
        {
            _simulation?.Dispose();
            _visuals?.Dispose();
            
            base.OnDestroy();
        }
        
        /// <summary>
        /// Updates the NPC simulation logic.
        /// </summary>
        void Update()
        {
            if (!IsInitialized) return;
            _simulation.Update();
        }
        
        /// <summary>
        /// Completes pending simulation jobs and updates visual representations and visibility.
        /// </summary>
        void LateUpdate()
        {
            if (!IsInitialized) return;
            
            _simulation.CompleteCurrentJob();
            _visuals.UpdateVisuals(_simulation.Data, HexToWorld, Time.deltaTime);
            _visibilityTracker.Process(_simulation.Data, _axialHexGrid, _currentVisionSet, Time.deltaTime);
        }
        
        /// <summary>
        /// Instantiates the core simulation, visual, and visibility sub-systems.
        /// </summary>
        private void InitializeComponents()
        {
            _simulation = new NpcSimulationSystem(minMoveInterval, maxMoveInterval);
            _visuals = new NpcVisualRegistry(npcVisualPrefab, moveSpeed, rotationSpeed, transform);
            _visibilityTracker = new NpcVisibilityTracker(visibilityUpdateInterval);
            
            _visibilityTracker.OnCountChanged += (count) => 
            {
                _lastShroudedCount = count;
                InvokeVisibleCountChanged();
            };
        }
        
        /// <summary>
        /// Starts the NPC simulation and spawning process. 
        /// This should be called once the grid data and passes are finalized.
        /// </summary>
        public void InitializeNpcs(GridInitializationFinishedEvent obj)
        {
            _simulation.Reset(_axialHexGrid.Tiles, _worldDecorator);
            _spawnCoroutine = StartCoroutine(SpawnNpcsRoutine(_axialHexGrid.Tiles));
        }

        /// <summary>
        /// Ensures all existing NPCs (Data and Visuals) are destroyed 
        /// before starting a new simulation cycle.
        /// </summary>
        private void CleanupActiveSimulation(WorldGenerationStartedEvent obj)
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            _simulation?.Dispose();
            _visuals?.Dispose();
            
            _lastShroudedCount = 0;
            _currentVisionSet.Clear();
            InvokeVisibleCountChanged();
            
            _visuals = new NpcVisualRegistry(npcVisualPrefab, moveSpeed, rotationSpeed, transform);
        }

        /// <summary>
        /// Coroutine that handles batched spawning of NPCs to maintain frame rate.
        /// </summary>
        private IEnumerator SpawnNpcsRoutine(IReadOnlyDictionary<Vector2Int, TileData> tiles)
        {
            int count = _playerSettings.PopulationSize;
            _simulation.InitializeData(count);
            _visuals.PrepareRegistry(count);

            int batchSize = 50; 
            for (int i = 0; i < count; i += batchSize)
            {
                int batch = Mathf.Min(batchSize, count - i);
                var slice = new NativeSlice<NpcData>(_simulation.Data, i, batch);
                
                _visuals.CreateVisualsInRange(slice, i, HexToWorld);
                Publish(new ReportWorkProgressRequest(0, batch));
                yield return null;
            }

            _simulation.Activate();
            Publish(new NpcSimulationCompleteEvent(count));
            _spawnCoroutine = null;
        }
        
        /// <summary>
        /// Updates the visibility of all NPC GameObjects based on the player's vision.
        /// </summary>
        /// <param name="visionSet">The set of tiles currently in the player's vision radius.</param>
        /// <param name="forceVisible">If true, ignores the vision set (debug mode).</param>
        public void UpdateNpcVisibility(HashSet<TileData> visionSet, bool forceVisible)
        {
            _isVisibilityForced = forceVisible;
            _currentVisionSet = visionSet;

            if (IsInitialized && _visuals != null)
            {
                _visuals.UpdateVisibilityStates(_simulation.Data, visionSet, forceVisible);
            }

            InvokeVisibleCountChanged();
        }

        /// <summary>
        /// Public access to clean up the simulation, used by the WorldDecorator during regeneration.
        /// </summary>
        public void CleanupNpcs()
        {
            CleanupActiveSimulation(null);
        }

        /// <summary>
        /// Calculates the final visible count based on shroud logic or debug overrides
        /// and notifies subscribers (UI).
        /// </summary>
        private void InvokeVisibleCountChanged() => Publish(new NpcVisibleAgentsCountChangedEvent(_lastShroudedCount));
        
        /// <summary>
        /// Converts axial hex coordinates to world space coordinates.
        /// </summary>
        private Vector3 HexToWorld(int2 coord)
        {
            return _axialHexGrid.AxialToWorld(coord.x, coord.y);
        }
    }
}