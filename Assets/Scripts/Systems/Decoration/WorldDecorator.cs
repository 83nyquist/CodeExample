using System.Collections;
using System.Collections.Generic;
using Data;
using Input;
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
        [Inject] private InputHandler _inputHandler;

        private int _visionRadius = 10;
        
        [Header("Performance")]
        [SerializeField] private float maxMsPerFrame = 3f; // Time budget for spawning visuals

        private readonly Queue<TileData> _tilesQueuedToShow = new Queue<TileData>();
        private readonly Queue<TileData> _tilesQueuedToHide = new Queue<TileData>();

        private bool _isProcessing = false;
        private readonly Dictionary<TileData, TileDecorator> _activeDecoratorsDict = new Dictionary<TileData, TileDecorator>();
        private HashSet<TileData> _decoratedTilesSet = new HashSet<TileData>();

        private InputLock _inputLock;
        
        private void Awake()
        {
            _inputLock = _inputHandler.RegisterInputLock(this);
            _axialHexGrid.OnGridGenerated += OnGridGenerated; // Listen to the new event
            _vanguardMover.OnPathNodeReached += OnPathNodeReached; // New handler
        }

        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated; // Unsubscribe from the new event
            _vanguardMover.OnPathNodeReached -= OnPathNodeReached;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            TileData origin = grid.GetValueOrDefault(Vector2Int.zero);
            UpdateDecorations(origin);
        }

        private void OnPathNodeReached(TileData tile)
        {
            UpdateDecorations(tile);
        }

        public void UpdateDecorations(TileData origin)
        {
            if (origin == null) return;

            _inputLock.IsLocked = true;
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
            float budgetSeconds = maxMsPerFrame / 1000f;

            while (_tilesQueuedToHide.Count > 0 || _tilesQueuedToShow.Count > 0)
            {
                float startTime = Time.realtimeSinceStartup;

                // Process hides (priority to cleanup first)
                while (_tilesQueuedToHide.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

                    TileData data = _tilesQueuedToHide.Dequeue();

                    if (_activeDecoratorsDict.TryGetValue(data, out TileDecorator decorator))
                    {
                        _activeDecoratorsDict.Remove(data);
                        _decoratorFactory.ReturnTileDecorator(decorator);
                    }
                }

                // Process shows
                while (_tilesQueuedToShow.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

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
                }

                yield return null;
            }

            _isProcessing = false;
            _inputLock.IsLocked = false;
        }
        
        public HashSet<TileData> GetVisibleTiles()
        {
            return _decoratedTilesSet;
        }
    }
}