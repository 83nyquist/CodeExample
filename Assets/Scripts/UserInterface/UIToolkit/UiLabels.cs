using Coordinators;
using Systems.Decoration;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using Systems.Grid;
using Systems.NonPlayerCharacters;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UiLabels : EventBusSubscriber
    {
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;
        [Inject] private UIController _uIController;
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private WorldDecorator _worldDecorator;
        [Inject] private VanguardMover _vanguardMover;
        [Inject] private NpcManager _npcManager;
        
        [SerializeField] private UIDocument uiDocument;
        
        private Label _lblVisibleAgents;
        private Label _lblActiveAgents;
        private Label _lblVisibleTiles;
        private Label _lblTotalTiles;
        
        /// <summary>
        /// Hooks into system events and queries the root visual element for label references.
        /// </summary>
        private void Start()
        {
            Subscribe<GridInitializationFinishedEvent>(OnGenerationComplete);
            Subscribe<PlayerDestinationReachedEvent>(OnDestinationReached);
            Subscribe<NpcVisibleAgentsCountChangedEvent>(OnVisibleAgentsCountChanged);
            
            _lblVisibleAgents = uiDocument.rootVisualElement.Q<Label>("VisibleAgents");
            _lblActiveAgents = uiDocument.rootVisualElement.Q<Label>("ActiveAgents");
            _lblVisibleTiles = uiDocument.rootVisualElement.Q<Label>("VisibleTiles");
            _lblTotalTiles = uiDocument.rootVisualElement.Q<Label>("TotalTiles");
        }

        /// <summary> Refreshes static labels when generation completes. </summary>
        private void OnGenerationComplete(GridInitializationFinishedEvent obj)
        {
            UpdateStaticLabels();
        }

        /// <summary> Refreshes static labels when the player moves. </summary>
        private void OnDestinationReached(PlayerDestinationReachedEvent obj)
        {
            UpdateStaticLabels();
        }

        /// <summary> Updates the visible agent count label. </summary>
        private void OnVisibleAgentsCountChanged(NpcVisibleAgentsCountChangedEvent obj)
        {
            _lblVisibleAgents.text = $"Visible Agents: {obj.VisibleCount}";
        }

        /// <summary>
        /// Pulls current counts from the NPC and Grid systems to update static UI text.
        /// </summary>
        public void UpdateStaticLabels()
        {
            _lblActiveAgents.text = $"Active Agents: {_npcManager.NpcCount}";
            _lblVisibleTiles.text = $"Visible Tiles: {_worldDecorator.GetVisibleTiles().Count}";
            _lblTotalTiles.text = $"Total Tiles: {_axialHexGrid.Tiles.Count}";
        }
    }
}
