using System;
using Systems.Decoration.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Input
{
    public class MouseInput : MonoBehaviour
    {
        [Inject] private TileRaycaster _tileRaycaster;
        [Inject] private InputUIBlocker _uiBlocker;

        [SerializeField] private float dragThresholdPixels = 5f;

        public event Action<TileDecorator> OnTileDecoratorPointerDown;
        public event Action<TileDecorator> OnTileDecoratorPointerUp;
        public event Action<TileDecorator> OnTileDecoratorDrag;
        public event Action<float> OnScroll;

        private bool _isPointerDown;
        private bool _isDragging;
        private Vector2 _pointerDownPosition;
        private TileDecorator _pointerDownTileDecorator;
        private TileDecorator _lastDraggedTileDecorator;

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

            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(scrollDelta, 0f) && !_uiBlocker.IsPointerOverUI(mousePosition))
            {
                OnScroll?.Invoke(scrollDelta);
            }
        }

        private void HandlePointerDown(Vector2 mousePosition)
        {
            if (_uiBlocker.IsPointerOverUI(mousePosition))
            {
                return;
            }

            TileDecorator tileDecorator = _tileRaycaster.RaycastTileDecorator(mousePosition);

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
            if (_uiBlocker.IsPointerOverUI(mousePosition))
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

            TileDecorator tileDecorator = _tileRaycaster.RaycastTileDecorator(mousePosition);

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

            if (!_uiBlocker.IsPointerOverUI(mousePosition))
            {
                tileDecorator = _tileRaycaster.RaycastTileDecorator(mousePosition);
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
    }
}
