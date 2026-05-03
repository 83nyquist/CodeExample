using System;
using System.Collections.Generic;
using Systems.Decoration.Components;
using Systems.Grid.Components;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Grid.Pathfinding
{
    public class AStarPathfinding : MonoBehaviour
    {
        [Inject] private VanguardController _vanguardController;
        [Inject] private VanguardMover _vanguardMover;

        public event Action<List<TileData>> OnPathCreated;
        public event Action OnPathCleared;

        public List<TileData> currentPath;
        
        private void Awake()
        {
            _vanguardMover.OnDestinationReached += ErasePath;
        }
        
        private void OnDestroy()
        {
            _vanguardMover.OnDestinationReached -= ErasePath;
        }
        
        public void DrawPath(TileDecorator targetDecorator)
        {
            // Ensure the Vanguard has a valid tile and that tile's visual decorator has been spawned
            if (targetDecorator == null || _vanguardController.CurrentTile == null || _vanguardController.CurrentTile.Decorator == null)
            {
                return;
            }

            if (!CanTraverse(targetDecorator.TileData))
            {
                return;
            }
            
            ErasePath();
            CreatePath(_vanguardController.CurrentTile.Decorator, targetDecorator);

            // If CreatePath failed to find a valid path, currentPath will be null
            if (currentPath == null)
            {
                return;
            }

            OnPathCreated?.Invoke(currentPath);
        }

        public void ErasePath(TileData data = null)
        {
            currentPath = null;
            OnPathCleared?.Invoke();
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
                //TODO User feedback
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
