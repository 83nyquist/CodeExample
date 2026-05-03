using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Input
{
    public class InputUIBlocker : MonoBehaviour
    {
        [SerializeField] private UIDocument[] blockingUIDocuments;

        public bool IsPointerOverUI(Vector2 mousePosition)
        {
            // 1. Check UGUI (Standard EventSystem)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            // 2. Check UI Toolkit (UIDocuments)
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

                // Convert screen position to Panel-space for UI Toolkit hit testing
                Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, mousePosition);
                VisualElement pickedElement = root.panel.Pick(panelPosition);

                // If we picked an element that isn't the transparent root, the UI is blocking.
                if (pickedElement != null && pickedElement != root)
                {
                    return true;
                }
            }

            return false;
        }
    }
}