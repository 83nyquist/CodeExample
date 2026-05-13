using Data;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Zenject;

namespace Coordinators
{
    public class SettingsSyncHandler : EventBusSubscriber
    {
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private GameSettings _gameSettings;

        private SettingsPersistenceService _persistence = new();

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
            _persistence.LoadAll(_playerSettings, _gameSettings);
        }

        private void OnGridRadiusChanged(GridRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.GridRadius = e.Value;
            _persistence.SavePlayerSettings(_playerSettings);
        }

        private void OnPopulationChanged(PopulationSizeChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.PopulationSize = e.Value;
            _persistence.SavePlayerSettings(_playerSettings);
        }

        private void OnVisionChanged(VisionRadiusChangedRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.VisionRadius = e.Value;
            _persistence.SavePlayerSettings(_playerSettings);
        }

        private void OnFpsToggle(FpsToggleRequest e)
        {
            if (_playerSettings == null) return;
            _playerSettings.ShowFPS = e.Value;
            _persistence.SavePlayerSettings(_playerSettings);
        }

        private void OnVolumeChanged(VolumeChangedRequest e)
        {
            if (_gameSettings == null) return;
            _gameSettings.MasterVolume = e.Value;
            _persistence.SaveGameSettings(_gameSettings);
        }
    }
}
