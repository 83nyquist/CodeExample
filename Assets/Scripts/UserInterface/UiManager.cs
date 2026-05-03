using System.Collections.Generic;
using Systems.Grid;
using UnityEngine;
using UserInterface.UGUI;
using UserInterface.UIToolkit;
using Zenject;

namespace UserInterface
{
    public class UiManager : MonoBehaviour
    {
        [Inject] private LoadingPanelController _loadingPanelController;
        [Inject] private UIController _uiController;
        [Inject] private AxialHexGrid _axialHexGrid;
        
        // private void Awake()
        // {
        //     _axialHexGrid.OnGridGenerated += OnGridGenerated;
        // }
        //
        // private void OnDestroy()
        // {
        //     _axialHexGrid.OnGridGenerated -= OnGridGenerated;
        // }
        //
        // private void OnGridGenerated(Dictionary<Vector2Int, TileData> obj)
        // {
        //     ShowGameMenu();
        // }

        public void ShowGameMenu()
        {
            _loadingPanelController.SetVisible(false);
            _uiController.SetVisible(true);
        }

        public void ShowLoadingScrean()
        {
            _loadingPanelController.SetVisible(true);
            _uiController.SetVisible(false);
        }
    }
}
