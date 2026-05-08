using System;
using System.Collections.Generic;
using Character;
using Coordinators;
using Core.Components;
using Systems.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UserInterface.UGUI
{
    /// <summary>
    /// Loading panel UI controller. Attached to a UI GameObject in the scene.
    /// Uses EventBusSubscriber for automatic event cleanup.
    /// </summary>
    public class LoadingPanelController : EventBusSubscriber
    {
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI waitLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderTilesLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderNpcLabel;
        [SerializeField] private TextMeshProUGUI loadingSliderLabelPercentage;
        [SerializeField] private CharacterSet characterSet;
        [SerializeField] private GameObject containerParent;
        [SerializeField] private Transform profileParent;
        [SerializeField] private GameObject profilePrefab;
        
        private void Awake()
        {
            profileParent.GetComponent<DestroyChildren>().Activate();
            CreateLeaderProfiles();
        }

        private void Start()
        {
            Subscribe<CommanderSelectedRequest>(OnCharacterSelectedRequest);
            Subscribe<GenerationProgressInitializedEvent>(OnProgressInit);
            Subscribe<GenerationProgressUpdatedEvent>(OnProgressUpdate);
            Subscribe<NpcSimulationCompleteEvent>(OnAllComplete);
            Subscribe<GameStateChangedEvent>(OnGameStateChangedEvent);
            Subscribe<CommanderSelectedRequest>(OnCharacterSelectedRequest);
        }

        public void CreateLeaderProfiles()
        {
            foreach (CharacterItem item in characterSet.characters)
            {
                GameObject go = Instantiate(profilePrefab, profileParent);
                
                CharacterProfile characterProfile = go.GetComponent<CharacterProfile>();
                characterProfile.SetCharacter(item);
                
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Publish(new CommanderSelectedRequest(item));
                });
            }
        }

        public void SetVisible(bool isVisible)
        {
            containerParent.SetActive(isVisible);
            
            if (isVisible)
            {
                titleLabel.gameObject.SetActive(true);
                waitLabel.gameObject.SetActive(false);
                profileParent.gameObject.SetActive(true);
            }
        }

        public void OnCharacterSelectedRequest(CommanderSelectedRequest obj)
        {
            titleLabel.gameObject.SetActive(false);
            waitLabel.gameObject.SetActive(true);
            profileParent.gameObject.SetActive(false);

            //TODO Refactor to a list of localized lines, this is just for fun
            List<string> quotes = new List<string>()
            {
                "Wait in daylight; avoid waiting in the dark without a headlamp.",
                "Wait for those behind to make contact, never go on without seeing them.",
                "Wait patiently in dense fog – moving on without visibility increases risk.",
                "Wait to cross a river until the water level has dropped.",
                "Better to wait one hour too many than one hour too little, the mountain always waits."
            };
            
            waitLabel.text = $"{obj.Character.name} - {quotes[Random.Range(0, quotes.Count)]}";
            
            Publish(new GameFlowInitUnlockRequest(ToString()));
        }
        
        private void OnGameStateChangedEvent(GameStateChangedEvent obj)
        {
            if (obj.State == GameState.Initializing)
            {
                Publish(new GameFlowInitLockRequest(ToString()));
            }
        }

        private void OnProgressInit(GenerationProgressInitializedEvent e)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = e.TotalTileWorkUnits + e.TotalNpcWorkUnits; // Max value is total work units
            loadingSlider.value = 0; // Ensure slider starts at 0
        }

        private void OnProgressUpdate(GenerationProgressUpdatedEvent e)
        {
            // Slider value should be the sum of all completed work
            loadingSlider.value = e.CompletedTileWorkUnits + e.CompletedNpcWorkUnits;
            bool isFinished = loadingSlider.value >= loadingSlider.maxValue;
            
            loadingSliderTilesLabel.text = $"Tiles: {e.CompletedTileWorkUnits} / {e.TotalTileWorkUnits}";
            loadingSliderNpcLabel.text = $"NPCs: {e.CompletedNpcWorkUnits} / {e.TotalNpcWorkUnits}";

            // Visual hack: Cap display at 99% until the slider actually hits the max value
            float displayPercentage = isFinished ? 100f : Mathf.Min(e.Progress, 99f);
            loadingSliderLabelPercentage.text = $"{displayPercentage:F0}%";

            if (isFinished) loadingSlider.value = loadingSlider.maxValue;
        }

        private void OnAllComplete(NpcSimulationCompleteEvent e)
        {
            loadingSlider.value = loadingSlider.maxValue;
            loadingSliderLabelPercentage.text = "100%";
        }
    }
}