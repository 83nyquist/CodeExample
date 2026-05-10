using Coordinators;
using Systems.EventBus;
using UnityEngine;
using UserInterface.UGUI;
using UserInterface.UIToolkit;
using Zenject;

namespace UserInterface
{
    public class UiManager : EventBusSubscriber
    {
        [Inject] private LoadingPanelController _loadingPanelController;
        [Inject] private UIController _uiController;
        [Inject] private GameFlowCoordinator _gameFlow;

        private void Start()
        {
            Subscribe<GameStateChangedEvent>(HandleStateChange);
            
            //Handle eventual race conditions
            ShowLoadingScreen();
        }

        private void HandleStateChange(GameStateChangedEvent obj)
        {
            switch (obj.State)
            {
                case GameState.Loading:
                    ShowLoadingScreen();
                    break;
                case GameState.Playing:
                    ShowGameplayUI();
                    break;
            }
        }

        private void ShowGameplayUI()
        {
            _loadingPanelController.SetVisible(false);
            _uiController.SetVisible(true);
        }

        private void ShowLoadingScreen()
        {
            _loadingPanelController.SetVisible(true);
            _uiController.SetVisible(false);
        }
    }
}
