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

        private bool _isProcessing;

        private void Start()
        {
            var root = _uiController.Root;
            root.Q<Button>("btn_generate").clicked += OnGenerateClicked;
            root.Q<Button>("btn_respawn").clicked += OnRespawnClicked;
            root.Q<Button>("btn_exit").clicked += OnExitClicked;
        }

        private void OnGenerateClicked()
        {
            _uiController.SetEnabled(false);
            _uiManager.ShowLoadingScreen();
            _vanguardController.Stop();
            _worldGenerator.GenerateWorld();
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private async void OnRespawnClicked()
        {
            // 1. Guard against re-entry
            if (_isProcessing) return;
            
            try 
            {
                _isProcessing = true;
                _vanguardController.Respawn();
                
                // 2. Use a delay to debounce rapid clicks
                await System.Threading.Tasks.Task.Delay(500);
                
                // 3. Destruction Guard: 
                // If the object was destroyed (e.g., scene changed) while awaiting,
                // Exit immediately to avoid MissingReferenceExceptions.
                if (this == null) return;
            }
            catch (System.Exception e)
            {
                // 4. Async void errors are silent; we must log them explicitly.
                Debug.LogException(e);
            }
            finally 
            {
                // 5. Always reset the state in finally to prevent permanent "locking"
                if (this != null)
                    _isProcessing = false;
            }
        }
    }
}