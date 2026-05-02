using System.Collections.Generic;
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
        [Inject] private GenerationProgressTracker _progressTracker;
        
        private bool _isLeaderSelected = false;
        private bool _isWorldLoaded = false;
        
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI loadingSliderLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabelPercentage;
        
        private void Awake()
        {
            _progressTracker.OnInitialized += OnInitialized;
            _progressTracker.OnProgressUpdated += OnProgressUpdated;
            _axialHexGrid.OnGridGenerated += OnComplete;
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
                SetVisible(false);
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            
            if (isVisible)
            {
                _isLeaderSelected = true;
            }
            else
            {
                _isLeaderSelected = false;
                _isWorldLoaded = false;
            }
        }

        public void OnLeaderSelected()
        {
            _isLeaderSelected = true;
        }

        public void OnInitialized(int total)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = total;
            _isWorldLoaded = false;
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
