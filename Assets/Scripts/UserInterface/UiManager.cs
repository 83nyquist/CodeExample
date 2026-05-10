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

        /// <summary>
        /// Subscribes to state changes and sets the initial UI state.
        /// </summary>
        private void Start()
        {
            Subscribe<GameStateChangedEvent>(HandleStateChange);
            ShowLoadingScreen();
        }

        /// <summary>
        /// Routes UI visibility changes based on the global game state.
        /// </summary>
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

        /// <summary>
        /// Transitions visibility to active gameplay HUD.
        /// </summary>
        private void ShowGameplayUI()
        {
            _loadingPanelController.SetVisible(false);
            _uiController.SetVisible(true);
        }

        /// <summary>
        /// Transitions visibility to the loading screen.
        /// </summary>
        private void ShowLoadingScreen()
        {
            _loadingPanelController.SetVisible(true);
            _uiController.SetVisible(false);
        }
    }
}
