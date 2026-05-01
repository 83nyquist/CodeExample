using System.Collections.Generic;
using Character;
using Systems.Decoration;
using Systems.Grid;
using UnityEngine;
using Zenject;

namespace Vanguard
{
    public class VanguardController : MonoBehaviour
    {
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private AStarPathfinding _aStarPathfinding;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        
        private TileData _currentTile;
        public TileData CurrentTile => _currentTile;
        
        private void Awake()
        {
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            _vanguardMover.OnDestinationReached += SetCurrentTile;
        }
        
        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
            _vanguardMover.OnDestinationReached -= SetCurrentTile;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            _currentTile = grid.GetValueOrDefault(Vector2Int.zero);
            transform.position = _axialHexGrid.AxialToWorld(_currentTile.X, _currentTile.Z);
        }

        public void Respawn()
        {
            _aStarPathfinding.ErasePath();
            OnGridGenerated(_axialHexGrid.Tiles);
            _worldDecorator.UpdateDecorations(_currentTile);
        }

        private void SetCurrentTile(TileData tileData)
        {
            _currentTile = tileData;
        }
    }
}
