using UnityEngine; // KEEP IN FOR THE UNITY CONDITION IN OnExitClicked()
using Coordinators;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Systems.NonPlayerCharacters;
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
        [Inject] private NpcManager _npcManager;

        private bool _isProcessing;

        /// <summary>
        /// Binds UI buttons to local actions and system requests.
        /// </summary>
        private void Start()
        {
            var root = _uiController.Root;
            root.Q<Button>("btn_generate").clicked += OnGenerateClicked;
            root.Q<Button>("btn_agentVisibility").clicked += OnToggleAgentsClicked;
            root.Q<Button>("btn_exit").clicked += OnExitClicked;
        }

        /// <summary> Publishes a world generation request. </summary>
        private void OnGenerateClicked()
        {
            Publish(new GenerateWorldRequest());
        }

        /// <summary>
        /// Exits the application or stops play mode in the editor.
        /// </summary>
        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary> Toggles NPC debug visibility. </summary>
        private void OnToggleAgentsClicked()
        {
            _npcManager.IsNpcVisibilityDebugEnabled = !_npcManager.IsNpcVisibilityDebugEnabled;
        }
    }
}