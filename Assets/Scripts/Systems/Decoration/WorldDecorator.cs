using System.Collections.Generic;
using Data;
using Input;
using Systems.Coordinators;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using NPC;
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
        private HashSet<TileData> _currentlyVisibleTiles = new HashSet<TileData>();
        private HashSet<TileData> _currentVisionSet = new HashSet<TileData>();
        private TileData _lastOrigin;
        private InputLock _inputLock;
        
        private void Awake()
        {
            // Initialize scheduler
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame);
            _scheduler.OnProcessingFinished += ReleaseInputLock;

            _inputLock = _inputHandler.RegisterInputLock(this);
            _worldGeneratorCoordinator.OnGenerationStarted += OnGenerationStarted;
            _worldGeneratorCoordinator.OnGenerationComplete += OnGenerationComplete;
            _vanguardMover.OnPathNodeReached += OnPathNodeReached;
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
            _currentlyVisibleTiles.Clear();
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

            // 1. Determine the "In Vision" set (Full Detail)
            List<TileData> visionTiles = _axialHexGrid.GetTilesInRadius(origin.AxialCoordinates, _playerSettings.visionRadius);
            _currentVisionSet = new HashSet<TileData>(visionTiles);

            // 2. Determine the "Active" set (Tiles that should have ANY prefab: Full or Shrouded)
            HashSet<TileData> nextActiveSet = new HashSet<TileData>();

            if (shroudMode == ShroudMode.RadiusBased)
            {
                // Only tiles within the secondary radius are active
                var radiusTiles = _axialHexGrid.GetTilesInRadius(origin.AxialCoordinates, secondaryShroudRadius);
                nextActiveSet.UnionWith(radiusTiles);
            }
            else
            {
                // Discovery mode: All tiles currently in vision OR already discovered remain active
                nextActiveSet.UnionWith(_currentVisionSet);
                foreach (var tile in _axialHexGrid.Tiles.Values)
                {
                    if (tile.IsDiscovered) nextActiveSet.Add(tile);
                }
            }
            
            List<TileData> toShow = new List<TileData>();
            List<TileData> toHide = new List<TileData>();

            // 3. Process Tiles that should be Active
            foreach (var tile in nextActiveSet)
            {
                bool isCurrentlyInVision = _currentVisionSet.Contains(tile);
                
                if (!_currentlyVisibleTiles.Contains(tile))
                {
                    // Freshly entering the active set
                    tile.IsDiscovered = true; 
                    tile.IsInVision = isCurrentlyInVision;
                    toShow.Add(tile);
                }
                else if (tile.IsInVision != isCurrentlyInVision)
                {
                    // Already active, but toggling between Shrouded and Visible
                    // We need to swap prefabs, so we hide then show
                    toHide.Add(tile);
                    tile.IsInVision = isCurrentlyInVision;
                    // If we are in radius mode and moving out of vision, we ensure it's still marked discovered
                    // so the factory knows to pick the shrouded prefab.
                    tile.IsDiscovered = true; 
                    toShow.Add(tile);
                }
            }

            // 4. Process Tiles that should no longer be Active (Moving out of shroud radius)
            foreach (var tile in _currentlyVisibleTiles)
            {
                if (!nextActiveSet.Contains(tile))
                {
                    toHide.Add(tile);
                    // If we aren't using Discovery mode, we reset IsInVision to prevent stale state
                    tile.IsInVision = false;
                }
            }

            // Update NPC visibility based on the new vision set
            UpdateNpcVisibility();

            if (toShow.Count == 0 && toHide.Count == 0) return;

            // Execute updates
            _inputLock.IsLocked = true;
            _currentlyVisibleTiles = nextActiveSet;
            
            StartCoroutine(_scheduler.ProcessQueues(toShow, toHide));
        }

        private void UpdateNpcVisibility()
        {
            if (_npcManager == null) return;
            
            // We pass the strict vision set and our debug override to the NPC manager
            _npcManager.UpdateNpcVisibility(_currentVisionSet, debugShowNpcsOutsideVision);
        }

        private void ReleaseInputLock()
        {
            _inputLock.IsLocked = false;
        }
        
        public HashSet<TileData> GetVisibleTiles() => _currentlyVisibleTiles;
        
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