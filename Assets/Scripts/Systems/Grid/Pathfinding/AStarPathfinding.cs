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
        
        private void Awake()
        {
            // _vanguardMover.OnDestinationReached += ErasePath;
            Subscribe<PlayerDestinationReachedEvent>(OnDestinationReached);
            Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            Subscribe<DrawPathRequest>(OnDrawPathRequest);
            Subscribe<ClearPathRequest>(OnClearPathRequest);
            Subscribe<WorldCleanupEvent>(OnWorldCleanup);
            Subscribe<RespawnRequest>(OnRespawnRequest);
        }
        
        private void OnRespawnRequest(RespawnRequest e) => ErasePath();
        private void OnDestinationReached(PlayerDestinationReachedEvent e) => ErasePath();
        private void OnPlayerMoved(PlayerMovedEvent e) => _playerTile = e.NewTile;
        private void OnDrawPathRequest(DrawPathRequest e) => DrawPath(e.Target);
        private void OnClearPathRequest(ClearPathRequest e) => ErasePath();
        private void OnWorldCleanup(WorldCleanupEvent e) => ErasePath();
        
        public void DrawPath(TileDecorator targetDecorator)
        {
            if (targetDecorator == null || _playerTile == null || _playerTile.Decorator == null) return;
            if (!CanTraverse(targetDecorator.TileData)) return;
            
            ErasePath();
            CreatePath(_playerTile.Decorator, targetDecorator);
            if (CurrentPath == null) return;

            Publish(new PathCreatedEvent(CurrentPath));
        }

        public void ErasePath()
        {
            CurrentPath = null;
            Publish(new PathClearedEvent());
        }
        
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

        private bool CanTraverse(TileData tile)
        {
            if (tile == null) return false;
            return tile.type != Enumerations.TileType.Water && 
                   tile.type != Enumerations.TileType.Mountain && 
                   tile.type != Enumerations.TileType.Forest;
        }
    }
}
