using System.Collections.Generic;
using Character;
using Systems.Grid;
using UnityEngine;
using Zenject;
using CharacterController = Character.CharacterController;

namespace UserInterface
{
    public class OutputAggregator : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private CharacterMover _characterMover;
        [Inject] private CharacterController _characterController;
        [Inject] private CharacterPathfinding _characterPathfinding;
        [Inject] private UIController _uIController;
        
        private void Start()
        {
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            _characterMover.OnDestinationReached += OnDestinationReached;
        }

        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
            _characterMover.OnDestinationReached -= OnDestinationReached;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> obj)
        {
            UpdateOutput();
        }

        private void OnDestinationReached(TileData obj)
        {
            UpdateOutput();
        }

        private void UpdateOutput()
        {
            string result = "";
            result += $"\n Current Tile: {_characterController.CurrentTile.AxialCoordinates}";
            result += $"\n Total Tiles: {_axialHexGrid.Tiles.Count}";
            
            _uIController.UpdateOutput(result);
        }
    }
}
