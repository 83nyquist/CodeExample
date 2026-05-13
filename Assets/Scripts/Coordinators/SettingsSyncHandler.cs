using Data;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
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

        /// <summary>
        /// Subscribes to settings change requests from the UI.
        /// </summary>
        private void Start()
        {
            Subscribe<GridRadiusChangedRequest>(OnGridRadiusChanged);
            Subscribe<PopulationSizeChangedRequest>(OnPopulationChanged);
            Subscribe<VisionRadiusChangedRequest>(OnVisionChanged);
            Subscribe<FpsToggleRequest>(OnFpsToggle);
            Subscribe<VolumeChangedRequest>(OnVolumeChanged);
        }

        /// <summary>
        /// Loads initial values for player and game settings.
        /// </summary>
        public void Initialize()
        {
            _playerSettings.Load();
            _gameSettings.Load();
        }

        /// <summary>
        /// Updates the grid radius setting in response to a UI request.
        /// </summary>
        private void OnGridRadiusChanged(GridRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.GridRadius = e.Value;
        }

        /// <summary>
        /// Updates the population size setting in response to a UI request.
        /// </summary>
        private void OnPopulationChanged(PopulationSizeChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.PopulationSize = e.Value;
        }

        /// <summary>
        /// Updates the vision radius setting in response to a UI request.
        /// </summary>
        private void OnVisionChanged(VisionRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.VisionRadius = e.Value;
        }

        /// <summary>
        /// Toggles the FPS display visibility.
        /// </summary>
        private void OnFpsToggle(FpsToggleRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.ShowFPS = e.Value;
        }

        /// <summary>
        /// Updates the master volume setting in response to a UI request.
        /// </summary>
        private void OnVolumeChanged(VolumeChangedRequest e)
        {
            if (_gameSettings == null) return;
            _gameSettings.MasterVolume = e.Value;
        }
    }
}