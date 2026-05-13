using System.Collections.Generic;
using Coordinators;
using Data;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using Systems.Decoration.Interfaces;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Interfaces;
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
        [Inject] private IEventBus _eventBus;

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

        private void Start()
        {
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame, _eventBus);
            _scheduler.OnProcessingFinished += ReleaseInputLock;
            
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
            if (Application.isPlaying && _lastOrigin != null)
            {
                UpdateNpcVisibility();
            }
        }

        private void OnGenerationStarted(WorldGenerationStartedEvent obj)
        {
            _activeDecorators.Clear();
            _currentVisionSet.Clear();
            _isInitialDecoration = true;
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

            var context = _visionStrategy.CalculateVision(origin);
            _currentVisionSet = context.VisionSet;

            var (toShow, toHide) = TileVisibilityProcessor.IdentifyChanges(context, _activeDecorators);

            UpdateNpcVisibility();
            if (toShow.Count > 0 || toHide.Count > 0)
            {
                ExecuteStateTransition(context.ActiveSet, toShow, toHide);
            }
        }

        private void ExecuteStateTransition(HashSet<TileData> nextActiveSet, List<TileData> toShow, List<TileData> toHide)
        {
            Publish(new InputLockRequest(ToString()));
            _activeDecorators = nextActiveSet;
            StartCoroutine(_scheduler.ProcessQueues(toShow, toHide, _isInitialDecoration));
            _isInitialDecoration = false;
        }

        private void UpdateNpcVisibility()
        {
            if (_npcManager == null) return;
            _npcManager.UpdateNpcVisibility(_currentVisionSet, debugShowNpcsOutsideVision);
        }

        private void ReleaseInputLock()
        {
            Publish(new WorldVisualsReadyEvent());
            Publish(new InputUnlockRequest(ToString()));
        }
        
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
        
        public HashSet<TileData> GetVisibleTiles() => _activeDecorators;
        
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
