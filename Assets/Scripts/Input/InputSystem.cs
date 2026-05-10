using System.Collections.Generic;
using Systems.Decoration.Components;
using Systems.EventBus;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    // ============================================
    // #region INPUT HANDLER (MAIN CLASS)
    // ============================================

    /// <summary>
    /// Main input handler - processes input locking and routes tile interaction events.
    /// </summary>
    public class InputSystem : EventBusSubscriber
    {
        [SerializeField] private List<string> inputLocks;
        private bool IsInputLocked => inputLocks.Count > 0;

        // Mouse input fields (merged from MouseInput)
        private LayerMaskRaycaster _layerMaskRaycaster;
        private InputUIBlocker _uiBlocker;
        [SerializeField] private float dragThresholdPixels = 5f;
        private bool _isPointerDown;
        private bool _isDragging;
        private Vector2 _pointerDownPosition;
        private TileDecorator _lastDraggedTileDecorator;

        // Camera zoom fields (merged from CameraZoom)
        [Header("Zoom Settings")]
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minDistance = 15f;
        [SerializeField] private float maxDistance = 40f;
        [SerializeField] private float zoomSmoothing = 0.1f;
        [Header("Camera Angle")]
        [SerializeField] private float defaultY = 30f;
        [SerializeField] private float defaultZ = -20f;
        [SerializeField] private bool maintainAngle = true;
        
        private CinemachineFollow _follow;
        private Vector3 _originalOffset;
        private float _targetDistance;
        private float _currentDistanceVelocity;
        private float _originalMagnitude;
        private Vector3 _normalizedDirection;

        // ============================================
        // #region UNITY LIFECYCLE
        // ============================================

        private void Awake()
        {
            // Initialize mouse input dependencies
            _layerMaskRaycaster = FindAnyObjectByType<LayerMaskRaycaster>();
            _uiBlocker = FindAnyObjectByType<InputUIBlocker>();

            // Initialize camera zoom
            if (cinemachineCamera == null)
                cinemachineCamera = GetComponent<CinemachineCamera>();
            
            _follow = cinemachineCamera?.GetComponent<CinemachineFollow>();
            
            if (_follow != null)
            {
                _originalOffset = _follow.FollowOffset;
                _normalizedDirection = _originalOffset.normalized;
                _originalMagnitude = _originalOffset.magnitude;
                _targetDistance = Mathf.Clamp(_originalMagnitude, minDistance, maxDistance);
            }
            else if (cinemachineCamera != null)
            {
                Debug.LogError("CinemachineFollow component not found on camera!");
            }

            // Subscribe to events
            Subscribe<InputLockRequest>(HandleLockRequest);
            Subscribe<InputUnlockRequest>(HandleUnlockRequest);
        }
        
        private void Update()
        {
            // Mouse input update
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
                OnMouseScroll(scrollDelta);
                // Publish(new MouseScrollEvent(scrollDelta));
            }

            // Camera zoom update
            ApplySmoothZoom();
        }

        // protected override void OnDestroy()
        // {
        //     base.OnDestroy();
        // }

        // ============================================
        // #region INPUT LOCKING
        // ============================================
       
        private void HandleLockRequest(InputLockRequest e)
        {
            inputLocks.Add(e.BlockerId);
        }

        private void HandleUnlockRequest(InputUnlockRequest e)
        {
            inputLocks.Remove(e.BlockerId);
        }

        // ============================================
        // #region MOUSE INPUT HANDLING
        // ============================================

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
            // Publish(new TilePointerDownEvent(tileDecorator));
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
            // Publish(new TileDragEvent(tileDecorator));
        }

        private void HandlePointerUp(Vector2 mousePosition)
        {
            if (!_isPointerDown) return;

            TileDecorator tileDecorator = null;
            if (!_uiBlocker.IsPointerOverUI(mousePosition))
                tileDecorator = _layerMaskRaycaster?.Raycast<TileDecorator>(mousePosition);

            if (tileDecorator != null)
            {
                MoveTo(tileDecorator);
                // Publish(new TilePointerUpEvent(tileDecorator));
            }

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

        // ============================================
        // #region CAMERA ZOOM HANDLING
        // ============================================

        private void OnMouseScroll(float scrollDelta)
        {
            float zoomDelta = scrollDelta * zoomSpeed;
            _targetDistance -= zoomDelta;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }

        private void ApplySmoothZoom()
        {
            if (_follow == null) return;
            
            float currentMagnitude = _follow.FollowOffset.magnitude;
            float smoothedMagnitude = Mathf.SmoothDamp(currentMagnitude, _targetDistance, ref _currentDistanceVelocity, zoomSmoothing);
            
            if (maintainAngle)
            {
                Vector3 newOffset = _normalizedDirection * smoothedMagnitude;
                _follow.FollowOffset = newOffset;
            }
            else
            {
                Vector3 newOffset = _follow.FollowOffset;
                newOffset.y = defaultY * (smoothedMagnitude / _originalMagnitude);
                newOffset.z = defaultZ * (smoothedMagnitude / _originalMagnitude);
                _follow.FollowOffset = newOffset;
            }
        }

        // Public zoom methods
        public void SetZoomNormalized(float normalizedValue)
        {
            _targetDistance = Mathf.Lerp(minDistance, maxDistance, normalizedValue);
        }

        public float GetZoomNormalized()
        {
            float currentDistance = _follow?.FollowOffset.magnitude ?? minDistance;
            return Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
        }
    }
}