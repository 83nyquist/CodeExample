using System.Collections.Generic;
using Systems.Decoration;
using Systems.Grid;
using UnityEngine;
using Zenject;

namespace Character
{
    public class CharacterController : MonoBehaviour
    {
        [Inject] private CharacterMover _characterMover;
        [Inject] private CharacterPathfinding _characterPathfinding;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        
        private TileData _currentTile;
        public TileData CurrentTile => _currentTile;
        
        private void Awake()
        {
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            _characterMover.OnDestinationReached += SetCurrentTile;
        }
        
        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
            _characterMover.OnDestinationReached -= SetCurrentTile;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            _currentTile = grid.GetValueOrDefault(Vector2Int.zero);
            transform.position = _axialHexGrid.AxialToWorld(_currentTile.X, _currentTile.Z);
        }

        public void Respawn()
        {
            _characterPathfinding.ErasePath();
            OnGridGenerated(_axialHexGrid.Tiles);
            _worldDecorator.UpdateDecorations(_currentTile);
        }

        private void SetCurrentTile(TileData tileData)
        {
            _currentTile = tileData;
        }
    }
}
