using System.Collections.Generic;
using Core.Components;
using Systems.Decoration;
using Systems.Grid.Pathfinding;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Grid
{
    public class AStarPathfinding : MonoBehaviour
    {
        [Inject] private VanguardController _vanguardController;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private AxialHexGrid _axialHexGrid;

        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private Transform pathParent;
        
        private DestroyChildren _destroyPathChildren;

        public List<TileData> currentPath;
        
        private void Awake()
        {
            _destroyPathChildren = pathParent.GetComponent<DestroyChildren>();
            
            _vanguardMover.OnDestinationReached += ErasePath;
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
        }
        
        private void OnDestroy()
        {
            _vanguardMover.OnDestinationReached -= ErasePath;
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
        }
        
        public void DrawPath(TileDecorator targetDecorator)
        {
            // Ensure the Vanguard has a valid tile and that tile's visual decorator has been spawned
            if (_vanguardController.CurrentTile == null || _vanguardController.CurrentTile.Decorator == null)
            {
                return;
            }

            if (!CanTraverse(targetDecorator.TileData))
            {
                return;
            }
            
            ErasePath();
            CreatePath(_vanguardController.CurrentTile.Decorator, targetDecorator);

            foreach (TileData tile in currentPath)
            {
                Vector3 worldPosition = _axialHexGrid.AxialToWorld(tile.X, tile.Z);
                Instantiate(pathPrefab, worldPosition, Quaternion.identity, pathParent);
            }
        }
        
        public void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            ErasePath();
        }
        
        public void ErasePath(TileData data = null)
        {
            _vanguardMover.StopMoving();
            currentPath = null;
            _destroyPathChildren.Activate();
        }
        
        private void CreatePath(TileDecorator origin, TileDecorator target)
        {
            if (origin == null || target == null)
            {
                Debug.LogError("Missing origin or target tile decorator.");
                return;
            }

            currentPath = TilePathfinder.FindPath(origin.TileData, target.TileData, CanTraverse);

            if (currentPath == null || currentPath.Count == 0)
            {
                Debug.Log("No path found.");
                return;
            }
        }

        private bool CanTraverse(TileData tile)
        {
            if (tile == null)
            {
                return false;
            }

            if (tile.type == TileType.Water || 
                tile.type == TileType.Mountain || 
                tile.type == TileType.Forest)
            {
                return false;
            }

            return true;
        }
    }
}
