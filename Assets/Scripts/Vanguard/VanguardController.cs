using System.Collections.Generic;
using Character;
using Core.Components;
using Systems.Decoration;
using Systems.Grid;
using UnityEngine;
using UserInterface.UIToolkit;
using Zenject;

namespace Vanguard
{
    public class VanguardController : MonoBehaviour
    {
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private AStarPathfinding _aStarPathfinding;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        [Inject] private UIController _uiController;
        
        [Inject] private DiContainer _container;
        
        [SerializeField] private CharacterItem selectedLeader;
        private DestroyChildren _destroyChildren;
        
        private TileData _currentTile;
        public TileData CurrentTile => _currentTile;
        
        private void Awake()
        {
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            _vanguardMover.OnDestinationReached += SetCurrentTile;

            _destroyChildren = GetComponent<DestroyChildren>();
            
            DeSpawn();
        }
        
        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
            _vanguardMover.OnDestinationReached -= SetCurrentTile;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            Stop();
            TileData origin = grid.GetValueOrDefault(Vector2Int.zero);
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
            _uiController.AnimationEvents = go.GetComponent<CharacterAnimationEvents>();
        }

        public void DeSpawn()
        {
            _vanguardMover.Animator = null;
            _uiController.AnimationEvents = null;
            _destroyChildren.Activate();
        }

        public void Respawn()
        {
            Stop();
            ReturnToOrigin(_axialHexGrid.Tiles.GetValueOrDefault(Vector2Int.zero));
            _worldDecorator.UpdateDecorations(_currentTile);
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

        private void SetCurrentTile(TileData tileData)
        {
            _currentTile = tileData;
        }
    }
}
