using System;
using Character;
using Core.Components;
using Systems.Coordinators;
using Systems.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UserInterface.UGUI
{
    public class LoadingPanelController : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private GenerationProgressTracker _progressTracker;
        
        public event Action<CharacterItem> OnCharacterSelected;
        public event Action OnLoadingStarted;
        public event Action OnLoadingFinished;
        
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI waitLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabelPercentage;
        [SerializeField] private CharacterSet characterSet;
        [SerializeField] private Transform profileParent;
        [SerializeField] private GameObject profilePrefab;
        
        private void Awake()
        {
            _progressTracker.OnInitialized += OnInitialized;
            _progressTracker.OnProgressUpdated += OnProgressUpdated;
            _worldGeneratorCoordinator.OnGenerationComplete += OnComplete;
            
            profileParent.GetComponent<DestroyChildren>().Activate();
            
            CreateLeaderProfiles();
        }

        private void OnDestroy()
        {
            _progressTracker.OnInitialized -= OnInitialized;
            _progressTracker.OnProgressUpdated -= OnProgressUpdated;
            _worldGeneratorCoordinator.OnGenerationComplete -= OnComplete;
        }

        public void CreateLeaderProfiles()
        {
            foreach (CharacterItem item in characterSet.characters)
            {
                GameObject go = Instantiate(profilePrefab, profileParent);
                
                CharacterProfile characterProfile = go.GetComponent<CharacterProfile>();
                characterProfile.SetCharacter(item);
                
                Button btn = go.GetComponent<Button>();
                btn.onClick.AddListener(() => OnSelectLeader(item));
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            
            if (isVisible)
            {
                titleLabel.gameObject.SetActive(true);
                waitLabel.gameObject.SetActive(false);
                profileParent.gameObject.SetActive(true);
            }
        }

        public void OnSelectLeader(CharacterItem item)
        {
            titleLabel.gameObject.SetActive(false);
            waitLabel.gameObject.SetActive(true);
            profileParent.gameObject.SetActive(false);
            
            OnCharacterSelected?.Invoke(item);
        }

        public void OnInitialized(int total)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = total;
            
            OnLoadingStarted?.Invoke();
        }

        public void OnProgressUpdated(int amount, int total, string workUnit)
        {
            loadingSlider.value = amount;
            loadingSliderLabel.text = $"{workUnit}: {amount} / {total}";
            loadingSliderLabelPercentage.text = $"{((float)amount / total * 100):F0}%";
        }

        public void OnComplete()
        {
            loadingSlider.value = loadingSlider.maxValue;
            OnLoadingFinished?.Invoke();
        }
    }
}
