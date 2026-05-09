using Audio;
using Data;
using UnityEngine;
using UnityEngine.UIElements;
using Character;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UISettingsController : MonoBehaviour
    {
        [Inject] private UIController _uiController;
        [Inject] private AudioManager _audioManager;
        [Inject] private GameSettings _gameSettings;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private DebugDrawer _debugDrawer;

        private CharacterAnimationEvents _currentCharacterEvents;

        private void Start()
        {
            var root = _uiController.Root;

            // Bind Sliders
            BindSlider(root.Q<Slider>("slider_grid"), "Grid Radius", 10, 1000, 
                _playerSettings.GridRadius, val => _playerSettings.GridRadius = val);

            BindSlider(root.Q<Slider>("slider_volume"), "Volume", 0, 100, 
                _gameSettings.MasterVolume, OnVolumeChanged);

            BindSlider(root.Q<Slider>("slider_population"), "Population", 0, 10000, 
                _playerSettings.PopulationSize, val => _playerSettings.PopulationSize = val);

            BindSlider(root.Q<Slider>("slider_vision"), "Vision Radius", 2, 20, 
                _playerSettings.VisionRadius, val => _playerSettings.VisionRadius = val);

            // Bind Toggles
            var tglFps = root.Q<Toggle>("tgl_fps");
            tglFps.label = "Show FPS:";
            tglFps.value = _playerSettings.ShowFPS;

            // Ensure the initial state is applied to the drawer on start
            _debugDrawer.showDebug = tglFps.value;

            tglFps.RegisterValueChangedCallback(evt => {
                _debugDrawer.showDebug = evt.newValue;
                _playerSettings.ShowFPS = evt.newValue;
            });
        }

        private void BindSlider(Slider slider, string label, float min, float max, float current, System.Action<int> onUpdate)
        {
            slider.lowValue = min;
            slider.highValue = max;
            slider.value = current;
            slider.label = $"{label}: {(int)current}";

            // Trigger the update logic immediately to sync systems with stored prefs
            onUpdate?.Invoke((int)current);

            slider.RegisterValueChangedCallback(evt => {
                int val = (int)evt.newValue;
                slider.label = $"{label}: {val}";
                onUpdate?.Invoke(val);
            });
        }

        private void OnVolumeChanged(int value)
        {
            float normalized = value / 100f;
            _audioManager.MusicSource.volume = normalized;
            _gameSettings.MasterVolume = value;

            ApplyVolumeToCharacter(value);
        }

        private void ApplyVolumeToCharacter(int value)
        {
            if (_currentCharacterEvents != null && _currentCharacterEvents.AudioSource != null)
                _currentCharacterEvents.AudioSource.volume = value / 100f;
        }
    }
}