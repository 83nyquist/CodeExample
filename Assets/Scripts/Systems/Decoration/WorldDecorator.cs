using System.Collections.Generic;
using Data;
using Input;
using Systems.Coordinators;
using Systems.Decoration.Components;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Decoration
{
    public class WorldDecorator : MonoBehaviour
    {
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private DecoratorFactory _decoratorFactory;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private InputHandler _inputHandler;

        [Header("Performance")]
        [SerializeField] private float maxMsPerFrame = 3f; // Time budget for spawning visuals

        private IDecorationScheduler _scheduler;
        private HashSet<TileData> _currentlyVisibleTiles = new HashSet<TileData>();
        private InputLock _inputLock;
        
        private void Awake()
        {
            // Initialize scheduler
            _scheduler = new DecorationScheduler(_decoratorFactory, maxMsPerFrame);
            _scheduler.OnProcessingFinished += ReleaseInputLock;

            _inputLock = _inputHandler.RegisterInputLock(this);
            _worldGeneratorCoordinator.OnGenerationComplete += OnGenerationComplete;
            _vanguardMover.OnPathNodeReached += OnPathNodeReached;
        }

        private void OnDestroy()
        {
            _worldGeneratorCoordinator.OnGenerationComplete -= OnGenerationComplete;
            _vanguardMover.OnPathNodeReached -= OnPathNodeReached;
            _scheduler.OnProcessingFinished -= ReleaseInputLock;
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

            List<TileData> newTilesInRadius = _axialHexGrid.GetTilesInRadius(
                origin.AxialCoordinates, 
                _playerSettings.visionRadius
            );
            
            HashSet<TileData> newTilesSet = new HashSet<TileData>(newTilesInRadius);
            
            List<TileData> toShow = new List<TileData>();
            List<TileData> toHide = new List<TileData>();

            // Identify Deltas
            foreach (var tile in newTilesSet)
                if (!_currentlyVisibleTiles.Contains(tile)) toShow.Add(tile);

            foreach (var tile in _currentlyVisibleTiles)
                if (!newTilesSet.Contains(tile)) toHide.Add(tile);

            if (toShow.Count == 0 && toHide.Count == 0) return;

            // Execute updates
            _inputLock.IsLocked = true;
            _currentlyVisibleTiles = newTilesSet;
            
            StartCoroutine(_scheduler.ProcessQueues(toShow, toHide));
        }

        private void ReleaseInputLock()
        {
            _inputLock.IsLocked = false;
        }
        
        public HashSet<TileData> GetVisibleTiles() => _currentlyVisibleTiles;
    }
}