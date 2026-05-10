using Systems.NonPlayerCharacters;
using UnityEngine;
using Zenject;

namespace UserInterface
{
    public class DebugDrawer : MonoBehaviour
    {
        [Inject] private NpcManager _npcManager;
        
        public bool showDebug = true;
        
        private float _fpsSmoothing = 0f;
        private float _fpsUpdateDelay = 0.2f;  // Update every 0.2 seconds
        private float _fpsTimer = 0f;
        private int _displayedFPS = 0;

        /// <summary>
        /// Calculates smoothed FPS and updates the display timer.
        /// </summary>
        void Update()
        {
            if (!showDebug)
            {
                return;
            }
            
            float currentFPS = 1f / Time.deltaTime;
            _fpsSmoothing = Mathf.Lerp(_fpsSmoothing, currentFPS, 0.1f);
    
            _fpsTimer += Time.deltaTime;
            if (_fpsTimer >= _fpsUpdateDelay)
            {
                _fpsTimer = 0;
                _displayedFPS = Mathf.RoundToInt(_fpsSmoothing);
            }
        }

        /// <summary>
        /// Renders the FPS debug box to the screen.
        /// </summary>
        void OnGUI()
        {
            if (!showDebug)
            {
                return;
            }
            
            GUILayout.BeginArea(new Rect(Screen.width - 100, 10, 90, 25));
            GUILayout.Box($"FPS: {_displayedFPS}");
            GUILayout.EndArea();
        }
    }
}
