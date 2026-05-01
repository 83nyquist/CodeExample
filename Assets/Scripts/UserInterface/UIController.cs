using Audio;
using Character;
using Game;
using Systems.Grid;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface
{
    public class UIController : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private AudioManager _audioManager;
        [Inject] private CharacterAnimationEvents _characterAnimationEvents;
        [Inject] private VanguardController _vanguardController;
        [Inject] private DebugDrawer _debugDrawer;
        
        [SerializeField] private UIDocument uiDocument;
        
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
            Debug.Log($"Pre all VisionRadius: {PlayerPrefs.GetInt("VisionRadius", -999)}");
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
        }

        private void OnGridValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);

            _sliderGrid.label = $"Grid Radius: {value}";
            GameSettings.GridRadius = value;
        }

        private void OnVolumeValueChanged(ChangeEvent<float> evt)
        {
            // Apply to audio sources
            _audioManager.MusicSource.volume = evt.newValue;
            _characterAnimationEvents.AudioSource.volume = evt.newValue;
    
            // Display and save
            int value = (int)evt.newValue;
            _sliderVolume.label = $"Volume: {value}";
            GameSettings.MasterVolume = value;
        }

        private void OnPopulationValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);

            _sliderPopulation.label = $"Population: {value}";
            GameSettings.PopulationSize = value;
        }

        private void OnVisionValueChanged(ChangeEvent<float> evt)
        {
            int value = (int)(evt.newValue);
            
            _sliderVision.label = $"Vision Radius: {value}";
            GameSettings.VisionRadius = value;
        }

        private void OnRespawnClicked()
        {
            _vanguardController.Respawn();
        }

        private void OnGenerateClicked()
        {
            _axialHexGrid.GenerateGrid();
        }

        private void OnExitClicked()
        {
            Application.Quit();
        }

        private void OnTglFpsChanged(ChangeEvent<bool> evt)
        {
            _debugDrawer.showDebug = evt.newValue;
            GameSettings.ShowFPS = evt.newValue;
        }

        public void SetSliderRanges()
        {
            _sliderGrid.lowValue = 0;
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
            _sliderGrid.label = $"Grid Radius: {GameSettings.GridRadius}";
            _sliderVolume.label = $"Volume: {GameSettings.MasterVolume}";
            _sliderPopulation.label = $"Population: {GameSettings.PopulationSize}";
            _sliderVision.label = $"Vision Radius: {GameSettings.VisionRadius}";
            
            _sliderGrid.value = GameSettings.GridRadius;
            _sliderVolume.value = GameSettings.MasterVolume;
            _sliderPopulation.value = GameSettings.PopulationSize;
            _sliderVision.value = GameSettings.VisionRadius;
            
            _tglFps.label = $"Show FPS:";
            _tglFps.value = GameSettings.ShowFPS;
        }
    }
}
