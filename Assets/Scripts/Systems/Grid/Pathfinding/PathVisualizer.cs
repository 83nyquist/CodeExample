using System.Collections.Generic;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Grid.Pathfinding
{
    public class PathVisualizer : EventBusSubscriber
    {
        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private Transform pathParent;
        private readonly Dictionary<TileData, GameObject> _instantiatedNodes = new();
        private float _hexSize;
        
        /// <summary>
        /// Subscribes to events for path creation, clearing, and movement.
        /// </summary>
        public void Start()
        {
            Subscribe<PathCreatedEvent>(OnPathCreated);
            Subscribe<PathClearedEvent>(OnPathCleared);
            Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            Subscribe<GridInitializationFinishedEvent>(OnGridInitialized);
        }

        /// <summary> Handles the path created event. </summary>
        private void OnPathCreated(PathCreatedEvent e) => DrawPath(e.Path);
        /// <summary> Handles the path cleared event. </summary>
        private void OnPathCleared(PathClearedEvent e) => ClearPath();
        /// <summary> Removes a specific visual node when the player moves onto it. </summary>
        private void OnPlayerMoved(PlayerMovedEvent e) => ClearNode(e.NewTile);
        /// <summary> Caches the hex size for coordinate conversions. </summary>
        private void OnGridInitialized(GridInitializationFinishedEvent e) => _hexSize = e.HexSize;

        /// <summary>
        /// Instantiates visual nodes for every tile in the provided path.
        /// </summary>
        private void DrawPath(List<TileData> path)
        {
            ClearPath();
            if (path == null) return;

            foreach (TileData tile in path)
            {
                Vector3 worldPosition = AxialToWorld(tile.X, tile.Z);
                GameObject node = GameObject.Instantiate(pathPrefab, worldPosition, Quaternion.identity, pathParent);
                _instantiatedNodes[tile] = node;
            }
        }

        /// <summary>
        /// Converts axial coordinates to world space for visualization.
        /// </summary>
        private Vector3 AxialToWorld(int q, int r)
        {
            float x = _hexSize * 1.73205081f * (q + r * 0.5f);
            float z = _hexSize * 1.5f * r;
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// Destroys a visual node associated with a specific tile.
        /// </summary>
        private void ClearNode(TileData tile)
        {
            if (tile != null && _instantiatedNodes.TryGetValue(tile, out GameObject node))
            {
                GameObject.Destroy(node);
                _instantiatedNodes.Remove(tile);
            }
        }

        /// <summary>
        /// Destroys all currently instantiated path visual nodes.
        /// </summary>
        private void ClearPath()
        {
            foreach (var node in _instantiatedNodes.Values) { if (node != null) GameObject.Destroy(node); }
            _instantiatedNodes.Clear();
        }
    }
}
