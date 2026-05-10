using Coordinators;
using Systems.EventBus;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace UserInterface.UIToolkit
{
    /// <summary>
    /// Handles the high-level state and visibility of the UI Toolkit Document.
    /// </summary>
    public class UIController : EventBusSubscriber
    {
        [Inject] private WorldGeneratorCoordinator _worldGeneratorCoordinator;

        [SerializeField] private UIDocument uiDocument;

        public VisualElement Root => uiDocument.rootVisualElement;

        // private void Start()
        // {
        //     Subscribe<WorldGenerationFinishedEvent>(OnGenerationComplete);
        //     SetEnabled(false);
        // }

        // private void OnGenerationComplete(WorldGenerationFinishedEvent obj) => SetEnabled(true);

        public void SetVisible(bool isVisible) => Root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        // public void SetEnabled(bool isEnabled) => Root.SetEnabled(isEnabled);
    }
}
