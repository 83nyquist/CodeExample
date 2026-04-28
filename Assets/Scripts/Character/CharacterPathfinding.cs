using System.Collections.Generic;
using Core.Components;
using Systems.Decoration;
using Systems.Grid;
using Systems.Grid.Pathfinding;
using UnityEngine;
using Zenject;

namespace Character
{
    public class CharacterPathfinding : MonoBehaviour
    {
        [Inject] private CharacterController _characterController;
        [Inject] private CharacterMover _characterMover;
        [Inject] private AxialHexGrid _axialHexGrid;

        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private Transform pathParent;
        
        private DestroyChildren _destroyPathChildren;

        public List<TileData> currentPath;
        
        private void Awake()
        {
            _destroyPathChildren = pathParent.GetComponent<DestroyChildren>();
            
            _characterMover.OnDestinationReached += ErasePath;
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
        }
        
        private void OnDestroy()
        {
            _characterMover.OnDestinationReached -= ErasePath;
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
        }
        
        public void DrawPath(TileDecorator targetDecorator)
        {
            if (!CanTraverse(targetDecorator.TileData))
            {
                return;
            }
            
            ErasePath();
            CreatePath(_characterController.CurrentTile.Decorator, targetDecorator);

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
            _characterMover.StopMoving();
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
