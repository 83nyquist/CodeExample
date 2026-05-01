using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Systems.Grid;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Decoration
{
    public class WorldDecorator : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private DecoratorFactory _decoratorFactory;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private PlayerSettings _playerSettings;

        private int _visionRadius = 10;
        
        [Header("Performance")]
        [SerializeField] private int decorationUpdatesPerFrame = 25;

        private readonly Queue<TileData> _tilesQueuedToShow = new Queue<TileData>();
        private readonly Queue<TileData> _tilesQueuedToHide = new Queue<TileData>();

        private bool _isProcessing = false;
        private readonly Dictionary<TileData, TileDecorator> _activeDecoratorsDict = new Dictionary<TileData, TileDecorator>();
        private HashSet<TileData> _decoratedTilesSet = new HashSet<TileData>();

        private void Awake()
        {
            _axialHexGrid.OnInitialNeighborsPopulated += DecorateInitialSubset; // Listen to the new event
            _vanguardMover.OnPathNodeReached += OnPathNodeReached; // New handler
        }

        private void OnPathNodeReached(TileData tile)
        {
            UpdateDecorations(tile);
        }

        public void UpdateDecorations(TileData origin)
        {
            if (origin == null) return;

            _visionRadius = _playerSettings.visionRadius;
            List<TileData> newTilesInRadius = _axialHexGrid.GetTilesInRadius(origin.AxialCoordinates, _visionRadius);

            // Queue tiles to show (in new set but not in old set)
            foreach (TileData data in newTilesInRadius)
            {
                if (data != null && !_decoratedTilesSet.Contains(data))
                {
                    _tilesQueuedToShow.Enqueue(data);
                }
            }

            // Queue tiles to hide (in old set but not in new set)
            foreach (TileData data in _decoratedTilesSet)
            {
                if (data != null && !newTilesInRadius.Contains(data))
                {
                    _tilesQueuedToHide.Enqueue(data);
                }
            }

            // Update decorated tiles set
            _decoratedTilesSet = new HashSet<TileData>(newTilesInRadius);

            // Start processing if not already running and there's work
            if (!_isProcessing && (_tilesQueuedToHide.Count > 0 || _tilesQueuedToShow.Count > 0))
            {
                StartCoroutine(ProcessDecorationQueue());
            }
        }

        private IEnumerator ProcessDecorationQueue()
        {
            _isProcessing = true;

            while (_tilesQueuedToHide.Count > 0 || _tilesQueuedToShow.Count > 0)
            {
                int processedThisFrame = 0;
                int maxPerFrame = decorationUpdatesPerFrame;

                // Process hides (priority to cleanup first)
                while (_tilesQueuedToHide.Count > 0 && processedThisFrame < maxPerFrame)
                {
                    TileData data = _tilesQueuedToHide.Dequeue();

                    if (_activeDecoratorsDict.TryGetValue(data, out TileDecorator decorator))
                    {
                        _activeDecoratorsDict.Remove(data);
                        _decoratorFactory.ReturnTileDecorator(decorator);
                    }

                    processedThisFrame++;
                }

                // Process shows
                while (_tilesQueuedToShow.Count > 0 && processedThisFrame < maxPerFrame)
                {
                    TileData data = _tilesQueuedToShow.Dequeue();

                    // Check if tile should still be decorated
                    if (data != null && _decoratedTilesSet.Contains(data) && !_activeDecoratorsDict.ContainsKey(data))
                    {
                        TileDecorator decorator = _decoratorFactory.GetTileDecorator(data);
                        if (decorator != null)
                        {
                            data.SetDecorator(decorator);
                            _activeDecoratorsDict[data] = decorator;
                        }
                    }

                    processedThisFrame++;
                }

                yield return null;
            }

            _isProcessing = false;
        }

        public void DecorateInitialSubset(TileData centerTile) // Renamed and modified signature
        {
            // Clean up
            ReturnAllDecoratorsToPool();
            _activeDecoratorsDict.Clear();
            _decoratedTilesSet.Clear();

            _tilesQueuedToShow.Clear();
            _tilesQueuedToHide.Clear();

            _isProcessing = false;

            // Start fresh
            UpdateDecorations(centerTile); // Use the provided centerTile
        }

        private void ReturnAllDecoratorsToPool()
        {
            foreach (var decorator in _activeDecoratorsDict.Values)
            {
                if (decorator != null)
                {
                    _decoratorFactory.ReturnTileDecorator(decorator);
                }
            }
        }
        
        public HashSet<TileData> GetVisibleTiles()
        {
            return _decoratedTilesSet;
        }

        private void OnDestroy()
        {
            _axialHexGrid.OnInitialNeighborsPopulated -= DecorateInitialSubset; // Unsubscribe from the new event
            _vanguardMover.OnPathNodeReached -= OnPathNodeReached;
        }
    }
}