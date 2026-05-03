using Systems.Coordinators;
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
        [Inject] private GameFlowCoordinator _gameFlow;

        private void Start()
        {
            // Forward UI events to the Coordinator
            _loadingPanelController.OnCharacterSelected += _gameFlow.SelectCharacter;
            _loadingPanelController.OnLoadingStarted += _gameFlow.ResetWorldState;
            
            // React to state changes from the Coordinator
            _gameFlow.OnStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            if (_loadingPanelController != null) _loadingPanelController.OnCharacterSelected -= _gameFlow.SelectCharacter;
            if (_loadingPanelController != null) _loadingPanelController.OnLoadingStarted -= _gameFlow.ResetWorldState;
            if (_gameFlow != null) _gameFlow.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Initializing:
                case GameState.CharacterSelection:
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

        public void ShowLoadingScreen()
        {
            _loadingPanelController.SetVisible(true);
            _uiController.SetVisible(false);
        }
    }
}
