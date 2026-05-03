using System.Collections.Generic;
using Character;
using Core.Components;
using Systems.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vanguard;
using Zenject;

namespace UserInterface.UGUI
{
    public class LoadingPanelController : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private GenerationProgressTracker _progressTracker;
        [Inject] private VanguardController _vanguardController;
        [Inject] private UiManager _uiManager;
        
        private bool _isLeaderSelected = false;
        private bool _isWorldLoaded = false;
        
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabelPercentage;
        [SerializeField] private CharacterSet characterSet;
        [SerializeField] private Transform profileParent;
        [SerializeField] private GameObject profilePrefab;
        
        private void Awake()
        {
            _progressTracker.OnInitialized += OnInitialized;
            _progressTracker.OnProgressUpdated += OnProgressUpdated;
            _axialHexGrid.OnGridGenerated += OnComplete;
            
            profileParent.GetComponent<DestroyChildren>().Activate();
            
            CreateLeaderProfiles();
        }

        private void OnDestroy()
        {
            _progressTracker.OnInitialized -= OnInitialized;
            _progressTracker.OnProgressUpdated -= OnProgressUpdated;
            _axialHexGrid.OnGridGenerated -= OnComplete;
        }

        void Update()
        {
            if (_isLeaderSelected && _isWorldLoaded)
            {
                InitializeVanguard();
            }
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
                profileParent.gameObject.SetActive(true);
            }
            else
            {
                _isLeaderSelected = false;
                _isWorldLoaded = false;
            }
        }

        public void InitializeVanguard()
        {
            _vanguardController.Spawn();
            SetVisible(false);
            _uiManager.ShowGameMenu();
        }

        public void OnSelectLeader(CharacterItem item)
        {
            titleLabel.gameObject.SetActive(false);
            profileParent.gameObject.SetActive(false);
            _vanguardController.SetLeader(item);
            _isLeaderSelected = true;
        }

        public void OnInitialized(int total)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = total;
            _isWorldLoaded = false;
            _vanguardController.DeSpawn();
        }

        public void OnProgressUpdated(int amount, int total, WorkUnitTypes workUnitType)
        {
            loadingSlider.value = amount;
            loadingSliderLabel.text = $"{workUnitType}: {amount} / {total}";
            loadingSliderLabelPercentage.text = $"{((float)amount / total * 100):F0}%";
        }

        public void OnComplete(Dictionary<Vector2Int, TileData> grid)
        {
            loadingSlider.value = loadingSlider.maxValue;
            loadingSliderLabel.text = "Loading Completed";
            loadingSliderLabel.text = "100%";
            _isWorldLoaded = true;
        }
    }
}
