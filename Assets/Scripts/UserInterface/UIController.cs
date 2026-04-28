using Audio;
using Character;
using Systems.Grid;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using CharacterController = Character.CharacterController;

namespace UserInterface
{
    public class UIController : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private AudioManager _audioManager;
        [Inject] private CharacterAnimationEvents _characterAnimationEvents;
        [Inject] private CharacterController _characterController;
        
        [SerializeField] private UIDocument uiDocument;
        private Slider _slider;
        private Button _btnGenerate;
        private Button _btnRespawn;
        private Button _btnExit;
        private Label _lblOutput;
        
        void Start()
        {
            _slider = uiDocument.rootVisualElement.Q<Slider>("Slider");
            _btnGenerate = uiDocument.rootVisualElement.Q<Button>("btn_generate");
            _btnRespawn = uiDocument.rootVisualElement.Q<Button>("btn_respawn");
            _btnExit = uiDocument.rootVisualElement.Q<Button>("btn_exit");
            _lblOutput = uiDocument.rootVisualElement.Q<Label>("lbl_output");

            _btnGenerate.clicked += OnGenerateClicked;
            _btnRespawn.clicked += OnRespawnClicked;
            _btnExit.clicked += OnExitClicked;
            
            _slider.RegisterValueChangedCallback(OnSliderValueChanged);
        }

        private void OnDestroy()
        {
            _btnGenerate.clicked -= OnGenerateClicked;
            _btnRespawn.clicked -= OnRespawnClicked;
            _btnExit.clicked -= OnExitClicked;
            
            _slider.UnregisterValueChangedCallback(OnSliderValueChanged);
        }

        private void OnRespawnClicked()
        {
            _characterController.Respawn();
        }

        private void OnGenerateClicked()
        {
            _axialHexGrid.GenerateGrid();
        }

        private void OnExitClicked()
        {
            Application.Quit();
        }

        private void OnSliderValueChanged(ChangeEvent<float> evt)
        {
            float value = evt.newValue/100;

            _audioManager.MusicSource.volume = value;
            _characterAnimationEvents.AudioSource.volume = value;
        }

        public void UpdateOutput(string output)
        {
            _lblOutput.text = output;
        }
    }
}
