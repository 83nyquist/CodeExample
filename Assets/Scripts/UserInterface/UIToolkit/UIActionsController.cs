using Systems.Coordinators;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UIActionsController : MonoBehaviour
    {
        [Inject] private UIController _uiController;
        [Inject] private UiManager _uiManager;
        [Inject] private VanguardController _vanguardController;
        [Inject] private WorldGeneratorCoordinator _worldGenerator;

        private void Start()
        {
            var root = _uiController.Root;
            root.Q<Button>("btn_generate").clicked += OnGenerateClicked;
            root.Q<Button>("btn_respawn").clicked += OnRespawnClicked;
            root.Q<Button>("btn_exit").clicked += () => Application.Quit();
        }

        private void OnGenerateClicked()
        {
            _uiController.SetEnabled(false);
            _uiManager.ShowLoadingScreen();
            _vanguardController.Stop();
            _worldGenerator.GenerateWorld();
        }

        private void OnRespawnClicked() => _vanguardController.Respawn();
    }
}