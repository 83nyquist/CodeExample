using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Input
{
    public class InputUIBlocker : MonoBehaviour
    {
        [SerializeField] private UIDocument[] blockingUIDocuments;

        /// <summary>
        /// Determines if the pointer is currently over a UGUI element or a specific UI Toolkit VisualElement.
        /// </summary>
        public bool IsPointerOverUI(Vector2 mousePosition)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            if (blockingUIDocuments == null)
            {
                return false;
            }

            foreach (UIDocument uiDocument in blockingUIDocuments)
            {
                if (uiDocument == null || uiDocument.rootVisualElement == null)
                {
                    continue;
                }

                VisualElement root = uiDocument.rootVisualElement;

                if (root.panel == null)
                {
                    continue;
                }

                Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, mousePosition);
                VisualElement pickedElement = root.panel.Pick(panelPosition);

                if (pickedElement != null && pickedElement != root)
                {
                    return true;
                }
            }

            return false;
        }
    }
}