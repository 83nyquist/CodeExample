using Audio;
using Data;
using UnityEngine;
using UnityEngine.UIElements;
using Character;
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

        /// <summary>
        /// Binds UI Toolkit sliders and toggles to game settings and player preferences.
        /// </summary>
        private void Start()
        {
            var root = _uiController.Root;

            BindSlider(root.Q<Slider>("slider_grid"), "Grid Radius", 10, 1000, 
                _playerSettings.GridRadius, val => _playerSettings.GridRadius = val);
            BindSlider(root.Q<Slider>("slider_volume"), "Volume", 0, 100, 
                _gameSettings.MasterVolume, OnVolumeChanged);
            BindSlider(root.Q<Slider>("slider_population"), "Population", 0, 10000, 
                _playerSettings.PopulationSize, val => _playerSettings.PopulationSize = val);
            BindSlider(root.Q<Slider>("slider_vision"), "Vision Radius", 2, 20, 
                _playerSettings.VisionRadius, val => _playerSettings.VisionRadius = val);

            var tglFps = root.Q<Toggle>("tgl_fps");
            tglFps.label = "Show FPS:";
            tglFps.value = _playerSettings.ShowFPS;
            _debugDrawer.showDebug = tglFps.value;
            tglFps.RegisterValueChangedCallback(evt => {
                _debugDrawer.showDebug = evt.newValue;
                _playerSettings.ShowFPS = evt.newValue;
            });
        }

        /// <summary>
        /// Configures a slider with specific bounds and hooks up value change callbacks.
        /// </summary>
        private void BindSlider(Slider slider, string label, float min, float max, float current, System.Action<int> onUpdate)
        {
            slider.lowValue = min;
            slider.highValue = max;
            slider.value = current;
            slider.label = $"{label}: {(int)current}";

            onUpdate?.Invoke((int)current);
            slider.RegisterValueChangedCallback(evt => {
                int val = (int)evt.newValue;
                slider.label = $"{label}: {val}";
                onUpdate?.Invoke(val);
            });
        }

        /// <summary>
        /// Updates audio manager volume and syncs settings.
        /// </summary>
        private void OnVolumeChanged(int value)
        {
            float normalized = value / 100f;
            _audioManager.MusicSource.volume = normalized;
            _gameSettings.MasterVolume = value;

            ApplyVolumeToCharacter(value);
        }

        /// <summary>
        /// Syncs character-specific audio sources with the master volume.
        /// </summary>
        private void ApplyVolumeToCharacter(int value)
        {
            if (_currentCharacterEvents != null && _currentCharacterEvents.AudioSource != null)
                _currentCharacterEvents.AudioSource.volume = value / 100f;
        }
    }
}