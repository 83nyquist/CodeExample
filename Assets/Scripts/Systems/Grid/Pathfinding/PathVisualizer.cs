using System.Collections.Generic;
using Core.Components;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using Vanguard;
using Zenject;
using UnityEngine;

namespace Systems.Grid.Pathfinding
{
    public class PathVisualizer : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private AStarPathfinding _pathfinding;
        [Inject] private VanguardMover _vanguardMover;

        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private Transform pathParent;
        
        private readonly Dictionary<TileData, GameObject> _instantiatedNodes = new();
        private DestroyChildren _destroyPathChildren;

        private void Start()
        {
            _destroyPathChildren = pathParent.GetComponent<DestroyChildren>();
            
            // Subscribe to events
            _pathfinding.OnPathCreated += DrawPath;
            _pathfinding.OnPathCleared += ClearPath;
            _vanguardMover.OnPathNodeReached += ClearNode;
        }

        private void OnDestroy()
        {
            if (_pathfinding != null)
            {
                _pathfinding.OnPathCreated -= DrawPath;
                _pathfinding.OnPathCleared -= ClearPath;
            }
            if (_vanguardMover != null) _vanguardMover.OnPathNodeReached -= ClearNode;
        }

        private void DrawPath(List<TileData> path)
        {
            ClearPath();
            
            if (path == null) return;

            foreach (TileData tile in path)
            {
                Vector3 worldPosition = _axialHexGrid.AxialToWorld(tile.X, tile.Z);
                GameObject node = Instantiate(pathPrefab, worldPosition, Quaternion.identity, pathParent);
                _instantiatedNodes[tile] = node;
            }
        }

        private void ClearNode(TileData tile)
        {
            if (tile != null && _instantiatedNodes.TryGetValue(tile, out GameObject node))
            {
                Destroy(node);
                _instantiatedNodes.Remove(tile);
            }
        }

        private void ClearPath()
        {
            foreach (var node in _instantiatedNodes.Values) { if (node != null) Destroy(node); }
            _instantiatedNodes.Clear();
            _destroyPathChildren.Activate();
        }
    }
}
