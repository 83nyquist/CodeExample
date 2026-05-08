using Coordinators;
using NPC;
using Systems.Decoration;
using Systems.EventBus;
using Systems.Grid;
using Systems.Grid.Components;
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
        
        private void Start()
        {
            Subscribe<GridInitializationFinishedEvent>(OnGenerationComplete);
            Subscribe<PlayerDestinationReachedEvent>(OnDestinationReached);
            Subscribe<NpcVisibleAgentsCountChangedEvent>(OnVisibleAgentsCountChanged);
            // _worldGeneratorCoordinator.OnGenerationComplete += OnGenerationComplete;
            // _vanguardMover.OnDestinationReached += OnDestinationReached;
            // _npcManager.OnVisibleAgentsCountChanged += OnVisibleAgentsCountChanged;
            
            _lblVisibleAgents = uiDocument.rootVisualElement.Q<Label>("VisibleAgents");
            _lblActiveAgents = uiDocument.rootVisualElement.Q<Label>("ActiveAgents");
            _lblVisibleTiles = uiDocument.rootVisualElement.Q<Label>("VisibleTiles");
            _lblTotalTiles = uiDocument.rootVisualElement.Q<Label>("TotalTiles");
        }

        private void OnGenerationComplete(GridInitializationFinishedEvent obj)
        {
            UpdateStaticLabels();
        }

        private void OnDestinationReached(PlayerDestinationReachedEvent obj)
        {
            UpdateStaticLabels();
        }

        private void OnVisibleAgentsCountChanged(NpcVisibleAgentsCountChangedEvent obj)
        {
            _lblVisibleAgents.text = $"Visible Agents: {obj.VisibleCount}";
        }

        public void UpdateStaticLabels()
        {
            _lblActiveAgents.text = $"Active Agents: {_npcManager.NpcCount}";
            _lblVisibleTiles.text = $"Visible Tiles: {_worldDecorator.GetVisibleTiles().Count}";
            _lblTotalTiles.text = $"Total Tiles: {_axialHexGrid.Tiles.Count}";
        }
    }
}
