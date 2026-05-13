using Audio;
using Data;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using UnityEngine;
using UnityEngine.UIElements;
using Character;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UISettingsController : EventBusSubscriber
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

            BindSlider(root.Q<Slider>("slider_grid"), "Grid Radius", 10, 1000, _playerSettings.GridRadius);
            BindSlider(root.Q<Slider>("slider_volume"), "Volume", 0, 100, _gameSettings.MasterVolume);
            BindSlider(root.Q<Slider>("slider_population"), "Population", 0, 10000, _playerSettings.PopulationSize);
            BindSlider(root.Q<Slider>("slider_vision"), "Vision Radius", 2, 20, _playerSettings.VisionRadius);

            root.Q<Slider>("slider_grid").RegisterValueChangedCallback(evt =>
                Publish(new GridRadiusChangedRequest((int)evt.newValue)));
            root.Q<Slider>("slider_population").RegisterValueChangedCallback(evt =>
                Publish(new PopulationSizeChangedRequest((int)evt.newValue)));
            root.Q<Slider>("slider_vision").RegisterValueChangedCallback(evt =>
                Publish(new VisionRadiusChangedRequest((int)evt.newValue)));
            root.Q<Slider>("slider_volume").RegisterValueChangedCallback(evt =>
            {
                int val = (int)evt.newValue;
                _audioManager.MusicSource.volume = val / 100f;
                Publish(new VolumeChangedRequest(val));
                ApplyVolumeToCharacter(val);
            });

            var tglFps = root.Q<Toggle>("tgl_fps");
            tglFps.label = "Show FPS:";
            tglFps.value = _playerSettings.ShowFPS;
            _debugDrawer.showDebug = tglFps.value;
            tglFps.RegisterValueChangedCallback(evt =>
            {
                _debugDrawer.showDebug = evt.newValue;
                Publish(new FpsToggleRequest(evt.newValue));
            });

            _audioManager.MusicSource.volume = _gameSettings.MasterVolume / 100f;
            ApplyVolumeToCharacter(_gameSettings.MasterVolume);
        }

        private static void BindSlider(Slider slider, string label, float min, float max, float current)
        {
            slider.lowValue = min;
            slider.highValue = max;
            slider.value = current;
            slider.label = $"{label}: {(int)current}";
            slider.RegisterValueChangedCallback(evt =>
            {
                slider.label = $"{label}: {(int)evt.newValue}";
            });
        }

        private void ApplyVolumeToCharacter(int value)
        {
            if (_currentCharacterEvents != null && _currentCharacterEvents.AudioSource != null)
                _currentCharacterEvents.AudioSource.volume = value / 100f;
        }
    }
}