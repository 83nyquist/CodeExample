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
        
        /// <summary>
        /// Initializes the UI container and generates leader profiles.
        /// </summary>
        private void Awake()
        {
            profileParent.GetComponent<DestroyChildren>().Activate();
            CreateLeaderProfiles();
        }

        /// <summary>
        /// Subscribes to world generation and game state events.
        /// </summary>
        private void Start()
        {
            Subscribe<CommanderSelectedRequest>(OnCharacterSelectedRequest);
            Subscribe<GenerationProgressInitializedEvent>(OnProgressInit);
            Subscribe<GenerationProgressUpdatedEvent>(OnProgressUpdate);
            Subscribe<NpcSimulationCompleteEvent>(OnAllComplete);
            Subscribe<GameStateChangedEvent>(OnGameStateChangedEvent);
            Subscribe<CommanderSelectedRequest>(OnCharacterSelectedRequest);
        }

        /// <summary>
        /// Instantiates character profile buttons based on the provided character set.
        /// </summary>
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

        /// <summary>
        /// Toggles the visibility of the loading panel container and its sub-elements.
        /// </summary>
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

        /// <summary>
        /// Handles the UI transition when a character is selected, displaying a random quote.
        /// </summary>
        public void OnCharacterSelectedRequest(CommanderSelectedRequest obj)
        {
            titleLabel.gameObject.SetActive(false);
            waitLabel.gameObject.SetActive(true);
            profileParent.gameObject.SetActive(false);

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
        
        /// <summary>
        /// Responds to game state changes to lock or unlock initialization.
        /// </summary>
        private void OnGameStateChangedEvent(GameStateChangedEvent obj)
        {
            if (obj.State == GameState.Loading)
            {
                Publish(new GameFlowInitLockRequest(ToString()));
            }
        }

        /// <summary>
        /// Configures the loading slider bounds based on expected work units.
        /// </summary>
        private void OnProgressInit(GenerationProgressInitializedEvent e)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = e.TotalTileWorkUnits + e.TotalNpcWorkUnits;
            loadingSlider.value = 0;
        }

        /// <summary>
        /// Updates the progress slider and text labels as generation work is completed.
        /// </summary>
        private void OnProgressUpdate(GenerationProgressUpdatedEvent e)
        {
            loadingSlider.value = e.CompletedTileWorkUnits + e.CompletedNpcWorkUnits;
            bool isFinished = loadingSlider.value >= loadingSlider.maxValue;
            
            loadingSliderTilesLabel.text = $"Work Units, Tiles: {e.CompletedTileWorkUnits:N0} / {e.TotalTileWorkUnits:N0}";
            loadingSliderNpcLabel.text = $"Work Units, NPCs: {e.CompletedNpcWorkUnits:N0} / {e.TotalNpcWorkUnits:N0}";
            
            float displayPercentage = isFinished ? 100f : Mathf.Min(e.Progress, 99f);
            loadingSliderLabelPercentage.text = $"{displayPercentage:F0}%";

            if (isFinished) loadingSlider.value = loadingSlider.maxValue;
        }

        /// <summary>
        /// Forces the loading UI to a 100% state upon simulation completion.
        /// </summary>
        private void OnAllComplete(NpcSimulationCompleteEvent e)
        {
            loadingSlider.value = loadingSlider.maxValue;
            loadingSliderLabelPercentage.text = "100%";
        }
    }
}