using System;
using System.Collections.Generic;
using Character;
using Coordinators;
using Core.Components;
using Systems.Decoration;
using Systems.EventBus;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using Systems.Grid.Pathfinding;
using UnityEngine;
using Zenject;

namespace Vanguard
{
    public class VanguardController : EventBusSubscriber
    {
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private AStarPathfinding _aStarPathfinding;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        
        [Inject] private DiContainer _container;
        
        public event Action<CharacterAnimationEvents> OnAnimationEventsChanged;

        [SerializeField] private CharacterItem selectedLeader;
        private DestroyChildren _destroyChildren;
        
        private TileData _currentTile;
        public TileData CurrentTile => _currentTile;
        private bool _isResetting;
        
        private void Awake()
        {
            Subscribe<WorldGenerationFinishedEvent>(OnGenerationComplete);
            Subscribe<PlayerDestinationReachedEvent>(SetCurrentTile);
            // _worldGeneratorCoordinator.OnGenerationComplete += OnGenerationComplete;
            // _vanguardMover.OnDestinationReached += SetCurrentTile;

            _destroyChildren = GetComponent<DestroyChildren>();
            
            DeSpawn();
        }

        private void OnGenerationComplete(WorldGenerationFinishedEvent obj)
        {
            Stop();
            TileData origin = _axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero);
            ReturnToOrigin(origin);
        }

        public void SetLeader(CharacterItem item)
        {
            selectedLeader = item;
        }

        public void Spawn()
        {
            GameObject go = Instantiate(selectedLeader.gamePrefab, transform);
            _vanguardMover.Animator = go.GetComponent<Animator>();
            OnAnimationEventsChanged?.Invoke(go.GetComponent<CharacterAnimationEvents>());
        }

        public void DeSpawn()
        {
            _vanguardMover.Animator = null;
            OnAnimationEventsChanged?.Invoke(null);
            _destroyChildren.Activate();
        }

        public void Respawn()
        {
            _isResetting = true;
            Stop();
            TileData origin = _axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero);
            ReturnToOrigin(origin);
            _worldDecorator.UpdateDecorations(_currentTile);
            _isResetting = false;
        }

        public void Stop()
        {
            _vanguardMover.StopMoving();
            _aStarPathfinding.ErasePath();
        }

        private void ReturnToOrigin(TileData origin)
        {
            _currentTile = origin;
            transform.position = _axialHexGrid.AxialToWorld(_currentTile.X, _currentTile.Z);
        }

        private void SetCurrentTile(PlayerDestinationReachedEvent obj)
        {
            if (_isResetting) return;
            
            _currentTile = obj.Tile;
        }
    }
}
