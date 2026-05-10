using Coordinators;
using Systems.Decoration;
using Systems.EventBus;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UIActionsController : EventBusSubscriber
    {
        [Inject] private UIController _uiController;
        [Inject] private UiManager _uiManager;
        [Inject] private VanguardController _vanguardController;
        [Inject] private WorldGeneratorCoordinator _worldGenerator;
        [Inject] private WorldDecorator _worldDecorator;

        private bool _isProcessing;

        private void Start()
        {
            var root = _uiController.Root;
            root.Q<Button>("btn_generate").clicked += OnGenerateClicked;
            root.Q<Button>("btn_agentVisibility").clicked += OnToggleAgentsClicked;
            root.Q<Button>("btn_exit").clicked += OnExitClicked;
        }

        private void OnGenerateClicked()
        {
            Publish(new GenerateWorldRequest());
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnToggleAgentsClicked()
        {
            _worldDecorator.IsNpcVisibilityDebugEnabled = !_worldDecorator.IsNpcVisibilityDebugEnabled;
        }
    }
}