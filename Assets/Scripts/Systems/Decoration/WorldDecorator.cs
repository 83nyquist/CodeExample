using System.Collections.Generic;
using Coordinators;
using Data;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using Systems.Decoration.Interfaces;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Systems.NonPlayerCharacters;
using Vanguard;
using Zenject;

namespace Systems.Decoration
{
    public class WorldDecorator : EventBusSubscriber
    {
        public enum ShroudMode
        {
            DiscoveryBased,
            RadiusBased
        }

        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private DecoratorFactory _decoratorFactory;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private NpcManager _npcManager;
        [Inject] private PlayerSettings _playerSettings;

        [SerializeField] private float maxMsPerFrame = 3f;

        [SerializeField] private ShroudMode shroudMode = ShroudMode.DiscoveryBased;
        [SerializeField] private int secondaryShroudRadius = 8;

        [SerializeField] private bool debugShowNpcsOutsideVision = false;

        private IDecorationScheduler _scheduler;
        private IVisionStrategy _visionStrategy;
        private HashSet<TileData> _activeDecorators = new();
        private HashSet<TileData> _currentVisionSet = new();
        private TileData _lastOrigin;
        private bool _isInitialDecoration = true;

        /// <summary>
        /// Initializes the decoration scheduler, strategy, and event subscriptions.
        /// </summary>
        private void Start()
        {
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame);
            _scheduler.OnProcessingFinished += ReleaseInputLock;
            
            InitializeStrategy();
            
            Subscribe<WorldGenerationStartedEvent>(OnGenerationStarted);
            Subscribe<GridInitializationFinishedEvent>(OnGenerationComplete);
            Subscribe<PlayerMovedEvent>(OnPathNodeReached);
        }

        /// <summary>
        /// Instantiates the vision strategy based on the current shrouding mode.
        /// </summary>
        private void InitializeStrategy()
        {
            _visionStrategy = shroudMode == ShroudMode.DiscoveryBased 
                ? new DiscoveryVisionStrategy(_axialHexGrid, _playerSettings) 
                : new RadiusVisionStrategy(_axialHexGrid, _playerSettings, secondaryShroudRadius);
        }

        /// <summary>
        /// Unsubscribes from scheduler events before destruction.
        /// </summary>
        protected override void OnDestroy()
        {
            _scheduler.OnProcessingFinished -= ReleaseInputLock;
            base.OnDestroy();
        }

        /// <summary>
        /// Updates NPC visibility in response to inspector changes during play mode.
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying && _lastOrigin != null)
            {
                UpdateNpcVisibility();
            }
        }

        /// <summary>
        /// Clears all existing visual state when world generation begins.
        /// </summary>
        private void OnGenerationStarted(WorldGenerationStartedEvent obj)
        {
            _activeDecorators.Clear();
            _currentVisionSet.Clear();
            _isInitialDecoration = true;
            _decoratorFactory.CleanupActiveDecorators();
            
            if (_npcManager != null)
                _npcManager.CleanupNpcs();
        }

        /// <summary>
        /// Triggers initial decoration placement once the grid is ready.
        /// </summary>
        private void OnGenerationComplete(GridInitializationFinishedEvent obj)
        {
            TileData origin = _axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero);
            UpdateDecorations(origin);
        }

        /// <summary>
        /// Triggers decoration updates when the player enters a new tile.
        /// </summary>
        private void OnPathNodeReached(PlayerMovedEvent obj)
        {
            UpdateDecorations(obj.NewTile);
        }

        /// <summary>
        /// Calculates vision changes and initiates the visual state transition.
        /// </summary>
        public void UpdateDecorations(TileData origin)
        {
            if (origin == null || _scheduler.IsProcessing) return;
            _lastOrigin = origin;

            var context = _visionStrategy.CalculateVision(origin);
            _currentVisionSet = context.VisionSet;

            var (toShow, toHide) = TileVisibilityProcessor.IdentifyChanges(context, _activeDecorators);

            UpdateNpcVisibility();
            if (toShow.Count > 0 || toHide.Count > 0)
            {
                ExecuteStateTransition(context.ActiveSet, toShow, toHide);
            }
        }

        /// <summary>
        /// Requests an input lock and starts the batched processing of spawning/hiding visuals.
        /// </summary>
        private void ExecuteStateTransition(HashSet<TileData> nextActiveSet, List<TileData> toShow, List<TileData> toHide)
        {
            Publish(new InputLockRequest(ToString()));
            _activeDecorators = nextActiveSet;
            StartCoroutine(_scheduler.ProcessQueues(toShow, toHide, _isInitialDecoration));
            _isInitialDecoration = false;
        }

        /// <summary>
        /// Updates NPC visibility state based on the current vision set.
        /// </summary>
        private void UpdateNpcVisibility()
        {
            if (_npcManager == null) return;
            _npcManager.UpdateNpcVisibility(_currentVisionSet, debugShowNpcsOutsideVision);
        }

        /// <summary>
        /// Publishes events to signal that visuals are ready and unlocks input.
        /// </summary>
        private void ReleaseInputLock()
        {
            Publish(new WorldVisualsReadyEvent());
            Publish(new InputUnlockRequest(ToString()));
        }
        
        /// <summary>
        /// Provides an estimate of the total work units for the progress tracker.
        /// </summary>
        public int GetInitialWorkEstimate()
        {
            if (_decoratorFactory.TileSet == null)
            {
                Debug.LogWarning("[DecoratorSystem] TileSet not assigned. Initial work estimate is 0. Please assign a TileSet ScriptableObject in the Inspector.", this);
                return 0;
            }
            int radius = shroudMode == ShroudMode.DiscoveryBased ? _playerSettings.VisionRadius : secondaryShroudRadius;
            return 3 * radius * radius + 3 * radius + 1;
        }
        
        /// <summary>
        /// Gets the set of tiles that currently have active decorators.
        /// </summary>
        public HashSet<TileData> GetVisibleTiles() => _activeDecorators;
        
        /// <summary>
        /// Gets tiles strictly within the player's vision radius.
        /// </summary>
        public HashSet<TileData> GetTilesInVision() => _currentVisionSet;
        
        /// <summary>
        /// Gets or sets whether NPCs should be visible regardless of vision radius for debugging.
        /// </summary>
        public bool IsNpcVisibilityDebugEnabled
        {
            get => debugShowNpcsOutsideVision;
            set
            {
                if (debugShowNpcsOutsideVision == value) return;
                debugShowNpcsOutsideVision = value;
                UpdateNpcVisibility();
            }
        }
    }
}