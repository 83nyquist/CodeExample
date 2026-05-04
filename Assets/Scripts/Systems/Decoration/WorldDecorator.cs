using System.Collections.Generic;
using Data;
using Input;
using Systems.Coordinators;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using NPC;
using Systems.Decoration.Interfaces;
using Vanguard;
using Zenject;

namespace Systems.Decoration
{
    public class WorldDecorator : MonoBehaviour
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

        private void Awake()
        {
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame);
            _scheduler.OnProcessingFinished += ReleaseInputLock;

            _inputLock = _inputHandler.RegisterInputLock(this);
            
            InitializeStrategy();

            _worldGeneratorCoordinator.OnGenerationStarted += OnGenerationStarted;
            _worldGeneratorCoordinator.OnGenerationComplete += OnGenerationComplete;
            _vanguardMover.OnPathNodeReached += OnPathNodeReached;
        }

        private void InitializeStrategy()
        {
            _visionStrategy = shroudMode == ShroudMode.DiscoveryBased 
                ? new DiscoveryVisionStrategy(_axialHexGrid, _playerSettings) 
                : new RadiusVisionStrategy(_axialHexGrid, _playerSettings, secondaryShroudRadius);
        }

        private void OnDestroy()
        {
            _worldGeneratorCoordinator.OnGenerationStarted -= OnGenerationStarted;
            _worldGeneratorCoordinator.OnGenerationComplete -= OnGenerationComplete;
            _vanguardMover.OnPathNodeReached -= OnPathNodeReached;
            _scheduler.OnProcessingFinished -= ReleaseInputLock;
        }

        private void OnValidate()
        {
            // Allow live-toggling the debug view in the editor
            if (Application.isPlaying && _lastOrigin != null)
            {
                UpdateNpcVisibility();
            }
        }

        private void OnGenerationStarted()
        {
            _activeDecorators.Clear();
            _currentVisionSet.Clear();
            _decoratorFactory.CleanupActiveDecorators();
            
            if (_npcManager != null)
                _npcManager.CleanupNpcs();
        }

        private void OnGenerationComplete()
        {
            TileData origin = _axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero);
            UpdateDecorations(origin);
        }

        private void OnPathNodeReached(TileData tile)
        {
            UpdateDecorations(tile);
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
            _inputLock.IsLocked = false;
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