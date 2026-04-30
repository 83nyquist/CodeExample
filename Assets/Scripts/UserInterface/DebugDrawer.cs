using NPC;
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

        void Update()
        {
            if (!showDebug)
            {
                return;
            }
            
            // Calculate smoothed FPS (exponential moving average)
            float currentFPS = 1f / Time.deltaTime;
            _fpsSmoothing = Mathf.Lerp(_fpsSmoothing, currentFPS, 0.1f);
    
            // Update display on delay
            _fpsTimer += Time.deltaTime;
            if (_fpsTimer >= _fpsUpdateDelay)
            {
                _fpsTimer = 0;
                _displayedFPS = Mathf.RoundToInt(_fpsSmoothing);
            }
        }

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
