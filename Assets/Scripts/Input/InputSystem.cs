using System.Collections.Generic;
using Core.Components;
using Systems.Decoration.Components;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputSystem : EventBusSubscriber
    {
        [SerializeField] private List<string> inputLocks;
        private bool IsInputLocked => inputLocks.Count > 0;

        private LayerMaskRaycaster _layerMaskRaycaster;
        private InputUIBlocker _uiBlocker;
        [SerializeField] private float dragThresholdPixels = 5f;
        private bool _isPointerDown;
        private bool _isDragging;
        private Vector2 _pointerDownPosition;
        private TileDecorator _lastDraggedTileDecorator;

        [SerializeField] private CameraZoomController _zoomController;

        private void Awake()
        {
            _layerMaskRaycaster = FindAnyObjectByType<LayerMaskRaycaster>();
            _uiBlocker = FindAnyObjectByType<InputUIBlocker>();

            if (_zoomController == null)
            {
                _zoomController = FindAnyObjectByType<CameraZoomController>();
                if (_zoomController == null)
                    _zoomController = gameObject.AddComponent<CameraZoomController>();
            }

            Subscribe<InputLockRequest>(HandleLockRequest);
            Subscribe<InputUnlockRequest>(HandleUnlockRequest);
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
                HandlePointerDown(mousePosition);

            if (_isPointerDown && Mouse.current.leftButton.isPressed)
                HandlePointerDrag(mousePosition);

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                HandlePointerUp(mousePosition);

            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(scrollDelta, 0f) && !_uiBlocker.IsPointerOverUI(mousePosition))
            {
                _zoomController?.OnScroll(scrollDelta);
            }

            _zoomController?.Tick();
        }

        private void HandleLockRequest(InputLockRequest e) => inputLocks.Add(e.BlockerId);

        private void HandleUnlockRequest(InputUnlockRequest e) => inputLocks.Remove(e.BlockerId);

        private void HandlePointerDown(Vector2 mousePosition)
        {
            if (_uiBlocker.IsPointerOverUI(mousePosition)) return;

            TileDecorator tileDecorator = _layerMaskRaycaster?.Raycast<TileDecorator>(mousePosition);
            if (tileDecorator == null) return;

            _isPointerDown = true;
            _isDragging = false;
            _pointerDownPosition = mousePosition;
            _lastDraggedTileDecorator = null;

            DrawPath(tileDecorator);
        }

        private void HandlePointerDrag(Vector2 mousePosition)
        {
            if (_uiBlocker.IsPointerOverUI(mousePosition))
            {
                DrawPath(null);
                _lastDraggedTileDecorator = null;
                return;
            }

            float distanceFromDown = Vector2.Distance(_pointerDownPosition, mousePosition);
            if (!_isDragging && distanceFromDown < dragThresholdPixels) return;

            _isDragging = true;
            TileDecorator tileDecorator = _layerMaskRaycaster?.Raycast<TileDecorator>(mousePosition);

            if (tileDecorator == null)
            {
                DrawPath(null);
                _lastDraggedTileDecorator = null;
                return;
            }

            if (tileDecorator == _lastDraggedTileDecorator) return;

            DrawPath(tileDecorator);
            _lastDraggedTileDecorator = tileDecorator;
        }

        private void HandlePointerUp(Vector2 mousePosition)
        {
            if (!_isPointerDown) return;

            TileDecorator tileDecorator = null;
            if (!_uiBlocker.IsPointerOverUI(mousePosition))
                tileDecorator = _layerMaskRaycaster?.Raycast<TileDecorator>(mousePosition);

            if (tileDecorator != null)
                MoveTo(tileDecorator);

            _isPointerDown = false;
            _isDragging = false;
            _lastDraggedTileDecorator = null;
        }

        private void MoveTo(TileDecorator tileDecorator)
        {
            if (tileDecorator == null || IsInputLocked) return;
            Publish(new PlayerMoveRequest());
        }

        private void DrawPath(TileDecorator tileDecorator)
        {
            if (IsInputLocked) return;

            if (tileDecorator == null)
            {
                Publish(new ClearPathRequest());
                return;
            }
            Publish(new DrawPathRequest(tileDecorator));
        }
    }
}
