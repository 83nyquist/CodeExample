using System;
using Systems.Decoration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Input
{
    public class MouseInput : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask tileLayerMask = ~0;
        [SerializeField] private UIDocument[] blockingUIDocuments;
        [SerializeField] private float dragThresholdPixels = 5f;

        public event Action<TileDecorator> OnTileDecoratorPointerDown;
        public event Action<TileDecorator> OnTileDecoratorPointerUp;
        public event Action<TileDecorator> OnTileDecoratorDrag;

        private bool _isPointerDown;
        private bool _isDragging;
        private Vector2 _pointerDownPosition;
        private TileDecorator _pointerDownTileDecorator;
        private TileDecorator _lastDraggedTileDecorator;

        private void Awake()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandlePointerDown(mousePosition);
            }

            if (_isPointerDown && Mouse.current.leftButton.isPressed)
            {
                HandlePointerDrag(mousePosition);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandlePointerUp(mousePosition);
            }
        }

        private void HandlePointerDown(Vector2 mousePosition)
        {
            if (IsPointerOverUI(mousePosition))
            {
                return;
            }

            TileDecorator tileDecorator = RaycastTileDecorator(mousePosition);

            if (tileDecorator == null)
            {
                return;
            }

            _isPointerDown = true;
            _isDragging = false;
            _pointerDownPosition = mousePosition;
            _pointerDownTileDecorator = tileDecorator;
            _lastDraggedTileDecorator = null;

            OnTileDecoratorPointerDown?.Invoke(tileDecorator);
        }

        private void HandlePointerDrag(Vector2 mousePosition)
        {
            if (IsPointerOverUI(mousePosition))
            {
                OnTileDecoratorDrag?.Invoke(null);
                _lastDraggedTileDecorator = null;
                return;
            }

            float distanceFromDown = Vector2.Distance(_pointerDownPosition, mousePosition);

            if (!_isDragging && distanceFromDown < dragThresholdPixels)
            {
                return;
            }

            _isDragging = true;

            TileDecorator tileDecorator = RaycastTileDecorator(mousePosition);

            if (tileDecorator == null)
            {
                OnTileDecoratorDrag?.Invoke(null);
                _lastDraggedTileDecorator = null;
                return;
            }

            if (tileDecorator == _lastDraggedTileDecorator)
            {
                return;
            }

            _lastDraggedTileDecorator = tileDecorator;

            OnTileDecoratorDrag?.Invoke(tileDecorator);
        }

        private void HandlePointerUp(Vector2 mousePosition)
        {
            if (!_isPointerDown)
            {
                return;
            }

            TileDecorator tileDecorator = null;

            if (!IsPointerOverUI(mousePosition))
            {
                tileDecorator = RaycastTileDecorator(mousePosition);
            }

            if (tileDecorator != null)
            {
                OnTileDecoratorPointerUp?.Invoke(tileDecorator);
            }

            _isPointerDown = false;
            _isDragging = false;
            _pointerDownTileDecorator = null;
            _lastDraggedTileDecorator = null;
        }

        private TileDecorator RaycastTileDecorator(Vector2 mousePosition)
        {
            if (inputCamera == null)
            {
                return null;
            }

            Ray ray = inputCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<TileDecorator>();
        }

        private bool IsPointerOverUI(Vector2 mousePosition)
        {
            // Check UGUI (EventSystem)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            // Check UI Toolkit
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
