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

        public void ShowDebugGui()
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
