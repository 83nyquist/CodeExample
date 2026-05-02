using System.Collections.Generic;
using NPC;
using Systems.Decoration;
using Systems.Grid;
using UnityEngine;
using UnityEngine.UIElements;
using Vanguard;
using Zenject;

namespace UserInterface.UIToolkit
{
    public class UiLabels : MonoBehaviour
    {
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
            _axialHexGrid.OnGridGenerated += OnGridGenerated;
            _vanguardMover.OnDestinationReached += OnDestinationReached;
            
            _lblVisibleAgents = uiDocument.rootVisualElement.Q<Label>("VisibleAgents");
            _lblActiveAgents = uiDocument.rootVisualElement.Q<Label>("ActiveAgents");
            _lblVisibleTiles = uiDocument.rootVisualElement.Q<Label>("VisibleTiles");
            _lblTotalTiles = uiDocument.rootVisualElement.Q<Label>("TotalTiles");

            _npcManager.OnVisibleAgentsCountChanged += OnVisibleAgentsCountChanged;
        }

        private void OnDestroy()
        {
            _axialHexGrid.OnGridGenerated -= OnGridGenerated;
            _vanguardMover.OnDestinationReached -= OnDestinationReached;
            _npcManager.OnVisibleAgentsCountChanged -= OnVisibleAgentsCountChanged;
        }

        private void OnGridGenerated(Dictionary<Vector2Int, TileData> obj)
        {
            UpdateStaticLabels();
        }

        private void OnDestinationReached(TileData obj)
        {
            UpdateStaticLabels();
        }

        private void OnVisibleAgentsCountChanged(int count)
        {
            _lblVisibleAgents.text = $"Visible Agents: {count}";
        }

        public void UpdateStaticLabels()
        {
            _lblActiveAgents.text = $"Active Agents: {_npcManager.NpcCount}";
            _lblVisibleTiles.text = $"Visible Tiles: {_worldDecorator.GetVisibleTiles().Count}";
            _lblTotalTiles.text = $"Total Tiles: {_axialHexGrid.Tiles.Count}";
        }
    }
}
