using System.Collections.Generic;
using Audio;
using Character;
using Game.Data;
using Systems.Grid;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UIController : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private AudioManager _audioManager;
        [Inject] private CharacterAnimationEvents _characterAnimationEvents;
        [Inject] private VanguardController _vanguardController;
        [Inject] private DebugDrawer _debugDrawer;
        [Inject] private GameSettings _gameSettings;
        [Inject] private PlayerSettings _playerSettings;
        
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Slider _sliderGrid;
        private Slider _sliderVolume;
        private Slider _sliderPopulation;
        private Slider _sliderVision;
        
        private Button _btnGenerate;
        private Button _btnRespawn;
        private Button _btnExit;
        
        private Toggle _tglFps;
        
        void Start()
        {
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            
            _root = uiDocument.rootVisualElement;
            
            _sliderGrid = uiDocument.rootVisualElement.Q<Slider>("slider_grid");
            _sliderVolume = uiDocument.rootVisualElement.Q<Slider>("slider_volume");
            _sliderPopulation = uiDocument.rootVisualElement.Q<Slider>("slider_population");
            _sliderVision = uiDocument.rootVisualElement.Q<Slider>("slider_vision");

            SetSliderRanges();
            
            _sliderGrid.RegisterValueChangedCallback(OnGridValueChanged);
            _sliderVolume.RegisterValueChangedCallback(OnVolumeValueChanged);
            _sliderPopulation.RegisterValueChangedCallback(OnPopulationValueChanged);
            _sliderVision.RegisterValueChangedCallback(OnVisionValueChanged);
            
            _btnGenerate = uiDocument.rootVisualElement.Q<Button>("btn_generate");
            _btnRespawn = uiDocument.rootVisualElement.Q<Button>("btn_respawn");
            _btnExit = uiDocument.rootVisualElement.Q<Button>("btn_exit");
            
            _btnGenerate.clicked += OnGenerateClicked;
            _btnRespawn.clicked += OnRespawnClicked;
            _btnExit.clicked += OnExitClicked;
            
            _tglFps = uiDocument.rootVisualElement.Q<Toggle>("tgl_fps");
            _tglFps.RegisterValueChangedCallback(OnTglFpsChanged);
            
            LoadSettings();
            
            SetEnabled(false);
        }

        private void OnDestroy()
        {
            _sliderGrid.UnregisterValueChangedCallback(OnGridValueChanged);
            _sliderVolume.UnregisterValueChangedCallback(OnVolumeValueChanged);
            _sliderPopulation.UnregisterValueChangedCallback(OnPopulationValueChanged);
            _sliderVision.UnregisterValueChangedCallback(OnVisionValueChanged);
            
            _btnGenerate.clicked -= OnGenerateClicked;
            _btnRespawn.clicked -= OnRespawnClicked;
            _btnExit.clicked -= OnExitClicked;
            
            _tglFps.UnregisterValueChangedCallback(OnTglFpsChanged);
            
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> grid)
        {
            SetEnabled(true);
        }

        private void OnGridValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);

            _sliderGrid.label = $"Grid Radius: {value}";
            _playerSettings.gridRadius = value;
        }

        private void OnVolumeValueChanged(ChangeEvent<float> evt)
        {
            // Apply to audio sources
            _audioManager.MusicSource.volume = evt.newValue;
            _characterAnimationEvents.AudioSource.volume = evt.newValue;
    
            // Display and save
            int value = (int)evt.newValue;
            _sliderVolume.label = $"Volume: {value}";
            _gameSettings.MasterVolume = value;
        }

        private void OnPopulationValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);

            _sliderPopulation.label = $"Population: {value}";
            _playerSettings.populationSize = value;
        }

        private void OnVisionValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);
            
            _sliderVision.label = $"Vision Radius: {value}";
            _playerSettings.visionRadius = value;
        }

        private void OnRespawnClicked()
        {
            _vanguardController.Respawn();
        }

        private void OnGenerateClicked()
        {
            SetEnabled(false);
            _vanguardController.Stop();
            _axialHexGrid.GenerateGrid();
        }

        private void OnExitClicked()
        {
            Application.Quit();
        }
        
        private void OnTglFpsChanged(ChangeEvent<bool> evt)
        {
            _debugDrawer.showDebug = evt.newValue;
            _playerSettings.showFPS = evt.newValue;
        }

        public void SetSliderRanges()
        {
            _sliderGrid.lowValue = 10;
            _sliderGrid.highValue = 1000;

            _sliderVolume.lowValue = 0;
            _sliderVolume.highValue = 100;

            _sliderPopulation.lowValue = 0;
            _sliderPopulation.highValue = 10000;

            _sliderVision.lowValue = 2;
            _sliderVision.highValue = 20;
        }

        public void LoadSettings()
        {
            _sliderGrid.label = $"Grid Radius: {_playerSettings.gridRadius}";
            _sliderVolume.label = $"Volume: {_gameSettings.MasterVolume}";
            _sliderPopulation.label = $"Population: {_playerSettings.populationSize}";
            _sliderVision.label = $"Vision Radius: {_playerSettings.visionRadius}";
            
            _sliderGrid.value = _playerSettings.gridRadius;
            _sliderVolume.value = _gameSettings.MasterVolume;
            _sliderPopulation.value = _playerSettings.populationSize;
            _sliderVision.value = _playerSettings.visionRadius;
            
            _tglFps.label = $"Show FPS:";
            _tglFps.value = _playerSettings.showFPS;
        }

        public void SetVisible(bool isVisible)
        {
            uiDocument.rootVisualElement.visible = isVisible;
        }

        public void SetEnabled(bool isEnabled)
        {
            _root.SetEnabled(isEnabled);
        }
    }
}
