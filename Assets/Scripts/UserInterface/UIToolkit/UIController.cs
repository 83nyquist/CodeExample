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

        /// <summary>
        /// Toggles the flex display style of the UI root.
        /// </summary>
        public void SetVisible(bool isVisible) => Root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
