using System.Collections.Generic;
using Data;
using Systems.Decoration.Components;
using Systems.EventBus;
using Systems.Grid.Components;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Grid.Pathfinding
{
    public class AStarPathfinding : EventBusSubscriber
    {
        [Inject] private VanguardController _vanguardController;
        [Inject] private VanguardMover _vanguardMover;

        public List<TileData> CurrentPath { get; private set; }
        private TileData _playerTile;
        
        /// <summary>
        /// Subscribes to pathfinding-related events and initializes the controller.
        /// </summary>
        private void Awake()
        {
            Subscribe<PlayerDestinationReachedEvent>(OnDestinationReached);
            Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            Subscribe<DrawPathRequest>(OnDrawPathRequest);
            Subscribe<ClearPathRequest>(OnClearPathRequest);
            Subscribe<WorldCleanupEvent>(OnWorldCleanup);
            Subscribe<RespawnRequest>(OnRespawnRequest);
        }
        
        /// <summary> Clears the path when a respawn is requested. </summary>
        private void OnRespawnRequest(RespawnRequest e) => ErasePath();
        /// <summary> Clears the path when the destination is reached. </summary>
        private void OnDestinationReached(PlayerDestinationReachedEvent e) => ErasePath();
        /// <summary> Updates the cached player tile position. </summary>
        private void OnPlayerMoved(PlayerMovedEvent e) => _playerTile = e.NewTile;
        /// <summary> Responds to a request to draw a path to a target. </summary>
        private void OnDrawPathRequest(DrawPathRequest e) => DrawPath(e.Target);
        /// <summary> Responds to a request to clear the current path. </summary>
        private void OnClearPathRequest(ClearPathRequest e) => ErasePath();
        /// <summary> Responds to a world cleanup event. </summary>
        private void OnWorldCleanup(WorldCleanupEvent e) => ErasePath();
        
        /// <summary>
        /// Validates and triggers the creation of a path to the specified decorator.
        /// </summary>
        public void DrawPath(TileDecorator targetDecorator)
        {
            if (targetDecorator == null || _playerTile == null || _playerTile.Decorator == null) return;
            if (!CanTraverse(targetDecorator.TileData)) return;
            
            ErasePath();
            CreatePath(_playerTile.Decorator, targetDecorator);
            if (CurrentPath == null) return;

            Publish(new PathCreatedEvent(CurrentPath));
        }

        /// <summary>
        /// Resets the current path and notifies the system.
        /// </summary>
        public void ErasePath()
        {
            CurrentPath = null;
            Publish(new PathClearedEvent());
        }
        
        /// <summary>
        /// Uses the TilePathfinder utility to calculate a list of tiles between two points.
        /// </summary>
        private void CreatePath(TileDecorator origin, TileDecorator target)
        {
            if (origin == null || target == null)
            {
                Debug.LogError("Missing origin or target tile decorator.");
                return;
            }

            CurrentPath = TilePathfinder.FindPath(origin.TileData, target.TileData, CanTraverse);

            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                Debug.Log("No path found.");
            }
        }

        /// <summary>
        /// Determines if a tile can be walked on based on its type.
        /// </summary>
        private bool CanTraverse(TileData tile)
        {
            if (tile == null) return false;
            return tile.type != Enumerations.TileType.Water && 
                   tile.type != Enumerations.TileType.Mountain && 
                   tile.type != Enumerations.TileType.Forest;
        }
    }
}
