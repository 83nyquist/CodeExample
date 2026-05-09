using System.Collections.Generic;
using Systems.EventBus;
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
        
        public void Start()
        {
            Subscribe<PathCreatedEvent>(OnPathCreated);
            Subscribe<PathClearedEvent>(OnPathCleared);
            Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            Subscribe<GridInitializationFinishedEvent>(OnGridInitialized);
        }

        private void OnPathCreated(PathCreatedEvent e) => DrawPath(e.Path);
        private void OnPathCleared(PathClearedEvent e) => ClearPath();
        private void OnPlayerMoved(PlayerMovedEvent e) => ClearNode(e.NewTile);
        private void OnGridInitialized(GridInitializationFinishedEvent e) => _hexSize = e.HexSize;

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

        private Vector3 AxialToWorld(int q, int r)
        {
            float x = _hexSize * 1.73205081f * (q + r * 0.5f);
            float z = _hexSize * 1.5f * r;
            return new Vector3(x, 0, z);
        }

        private void ClearNode(TileData tile)
        {
            if (tile != null && _instantiatedNodes.TryGetValue(tile, out GameObject node))
            {
                GameObject.Destroy(node);
                _instantiatedNodes.Remove(tile);
            }
        }

        private void ClearPath()
        {
            foreach (var node in _instantiatedNodes.Values) { if (node != null) GameObject.Destroy(node); }
            _instantiatedNodes.Clear();
        }
    }
}
