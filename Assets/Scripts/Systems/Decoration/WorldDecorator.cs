using System.Collections.Generic;
using Coordinators;
using Data;
using Input;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using NPC;
using Systems.Decoration.Interfaces;
using Systems.EventBus;
using Vanguard;
using Zenject;

namespace Systems.Decoration
{
    public class WorldDecorator : EventBusSubscriber
    {
        public enum ShroudMode
        {
            DiscoveryBased, // Shroud everything previously visited
            RadiusBased     // Shroud only within a specific secondary radius
        }

        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private DecoratorFactory _decoratorFactory;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private NpcManager _npcManager;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private InputHandler _inputHandler;

        [Header("Performance")]
        [SerializeField] private float maxMsPerFrame = 3f; // Time budget for spawning visuals

        [Header("Shrouding Configuration")]
        [SerializeField] private ShroudMode shroudMode = ShroudMode.DiscoveryBased;
        [SerializeField] private int secondaryShroudRadius = 8;

        [Header("NPC Visibility")]
        [SerializeField] private bool debugShowNpcsOutsideVision = false;

        private IDecorationScheduler _scheduler;
        private IVisionStrategy _visionStrategy;
        private HashSet<TileData> _activeDecorators = new();
        private HashSet<TileData> _currentVisionSet = new();
        private TileData _lastOrigin;
        private InputLock _inputLock;

        private void Start()
        {
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame);
            _scheduler.OnProcessingFinished += ReleaseInputLock;

            _inputLock = _inputHandler.RegisterInputLock(this);
            
            InitializeStrategy();
            
            Subscribe<WorldGenerationStartedEvent>(OnGenerationStarted);
            Subscribe<GridInitializationFinishedEvent>(OnGenerationComplete);
            Subscribe<PlayerMovedEvent>(OnPathNodeReached);
        }

        private void InitializeStrategy()
        {
            _visionStrategy = shroudMode == ShroudMode.DiscoveryBased 
                ? new DiscoveryVisionStrategy(_axialHexGrid, _playerSettings) 
                : new RadiusVisionStrategy(_axialHexGrid, _playerSettings, secondaryShroudRadius);
        }

        protected override void OnDestroy()
        {
            _scheduler.OnProcessingFinished -= ReleaseInputLock;
            base.OnDestroy();
        }

        private void OnValidate()
        {
            // Allow live-toggling the debug view in the editor
            if (Application.isPlaying && _lastOrigin != null)
            {
                UpdateNpcVisibility();
            }
        }

        private void OnGenerationStarted(WorldGenerationStartedEvent obj)
        {
            _activeDecorators.Clear();
            _currentVisionSet.Clear();
            _decoratorFactory.CleanupActiveDecorators();
            
            if (_npcManager != null)
                _npcManager.CleanupNpcs();
        }

        private void OnGenerationComplete(GridInitializationFinishedEvent obj)
        {
            TileData origin = _axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero);
            UpdateDecorations(origin);
        }

        private void OnPathNodeReached(PlayerMovedEvent obj)
        {
            UpdateDecorations(obj.NewTile);
        }

        public void UpdateDecorations(TileData origin)
        {
            if (origin == null || _scheduler.IsProcessing) return;
            _lastOrigin = origin;

            // 1. Logic: Determine what should be seen
            var context = _visionStrategy.CalculateVision(origin);
            _currentVisionSet = context.VisionSet;

            // 2. Logic: Compare against current state
            var (toShow, toHide) = TileVisibilityProcessor.IdentifyChanges(context, _activeDecorators);

            UpdateNpcVisibility();

            if (toShow.Count > 0 || toHide.Count > 0)
            {
                ExecuteStateTransition(context.ActiveSet, toShow, toHide);
            }
        }

        private void ExecuteStateTransition(HashSet<TileData> nextActiveSet, List<TileData> toShow, List<TileData> toHide)
        {
            _inputLock.IsLocked = true;
            _activeDecorators = nextActiveSet;
            
            StartCoroutine(_scheduler.ProcessQueues(toShow, toHide));
        }

        private void UpdateNpcVisibility()
        {
            if (_npcManager == null) return;
            _npcManager.UpdateNpcVisibility(_currentVisionSet, debugShowNpcsOutsideVision);
        }

        private void ReleaseInputLock()
        {
            Publish(new WorldVisualsReadyEvent());
            _inputLock.IsLocked = false;
        }
        
        public int GetInitialWorkEstimate()
        {
            // Calculate based on the radius used by the current strategy
            if (_decoratorFactory.TileSet == null)
            {
                // If no TileSet is assigned, the DecoratorSystem won't actually create any visuals.
                // Therefore, it contributes 0 work units to the total generation progress.
                Debug.LogWarning("[DecoratorSystem] TileSet not assigned. Initial work estimate is 0. Please assign a TileSet ScriptableObject in the Inspector.", this);
                return 0;
            }
            int radius = shroudMode == ShroudMode.DiscoveryBased ? _playerSettings.VisionRadius : secondaryShroudRadius;
            return 3 * radius * radius + 3 * radius + 1;
        }
        
        public HashSet<TileData> GetVisibleTiles() => _activeDecorators;
        
        /// <summary>
        /// Returns tiles strictly within the player's vision radius.
        /// Useful for other systems (like Combat or AI) to check line-of-sight.
        /// </summary>
        public HashSet<TileData> GetTilesInVision() => _currentVisionSet;
        
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