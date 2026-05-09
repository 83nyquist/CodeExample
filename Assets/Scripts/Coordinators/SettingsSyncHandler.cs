using Data;
using Systems.EventBus;
using Zenject;

namespace Coordinators
{
    /// <summary>
    /// Listens to UI requests and synchronizes them with ScriptableObject data.
    /// This keeps the UI decoupled from the data persistence logic.
    /// </summary>
    public class SettingsSyncHandler : EventBusSubscriber
    {
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private GameSettings _gameSettings;

        private void Start()
        {
            Subscribe<GridRadiusChangedRequest>(OnGridRadiusChanged);
            Subscribe<PopulationSizeChangedRequest>(OnPopulationChanged);
            Subscribe<VisionRadiusChangedRequest>(OnVisionChanged);
            Subscribe<FpsToggleRequest>(OnFpsToggle);
            Subscribe<VolumeChangedRequest>(OnVolumeChanged);
        }

        public void Initialize()
        {
            // Load from PlayerPrefs when the asset is initialized
            _playerSettings.Load();
            _gameSettings.Load();
        }

        private void OnGridRadiusChanged(GridRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.GridRadius = e.Value;
        }

        private void OnPopulationChanged(PopulationSizeChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.PopulationSize = e.Value;
        }

        private void OnVisionChanged(VisionRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.VisionRadius = e.Value;
        }

        private void OnFpsToggle(FpsToggleRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.ShowFPS = e.Value;
        }

        private void OnVolumeChanged(VolumeChangedRequest e)
        {
            if (_gameSettings == null) return;
            _gameSettings.MasterVolume = e.Value;
        }
    }
}