using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class CameraZoom : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera cinemachineCamera;
    
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minDistance = 15f;   // Minimum combined distance
        [SerializeField] private float maxDistance = 40f;   // Maximum combined distance
        [SerializeField] private float zoomSmoothing = 0.1f;
    
        [Header("Camera Angle")]
        [SerializeField] private float defaultY = 30f;
        [SerializeField] private float defaultZ = -20f;
        [SerializeField] private bool maintainAngle = true;  // Keep the same viewing angle
    
        private CinemachineFollow _follow;
        private Vector3 _originalOffset;
        private float _targetDistance;
        private float _currentDistanceVelocity;
        private float _originalMagnitude;
        private Vector3 _normalizedDirection;
    
        private void Awake()
        {
            if (cinemachineCamera == null)
                cinemachineCamera = GetComponent<CinemachineCamera>();
        
            _follow = cinemachineCamera.GetComponent<CinemachineFollow>();
        
            if (_follow != null)
            {
                _originalOffset = _follow.FollowOffset;
            
                // Calculate the direction vector (normalized)
                _normalizedDirection = _originalOffset.normalized;
            
                // Calculate the magnitude (distance from target)
                _originalMagnitude = _originalOffset.magnitude;
            
                // Set initial target distance
                _targetDistance = _originalMagnitude;
            
                // Clamp to min/max
                _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
            }
            else
            {
                Debug.LogError("CinemachineFollow component not found on camera!");
            }
        }
    
        private void Update()
        {
            HandleZoomInput();
            ApplySmoothZoom();
        }
    
        private void HandleZoomInput()
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
        
            if (Mathf.Approximately(scrollDelta, 0f))
                return;
        
            // Scroll up (positive) = zoom in (decrease distance)
            // Scroll down (negative) = zoom out (increase distance)
            float zoomDelta = scrollDelta * zoomSpeed;
            _targetDistance -= zoomDelta;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }
    
        private void ApplySmoothZoom()
        {
            if (_follow == null) return;
        
            // Get current magnitude
            float currentMagnitude = _follow.FollowOffset.magnitude;
        
            // Smoothly interpolate to target distance
            float smoothedMagnitude = Mathf.SmoothDamp(currentMagnitude, _targetDistance, ref _currentDistanceVelocity, zoomSmoothing);
        
            if (maintainAngle)
            {
                // Scale the entire offset vector proportionally to maintain the exact same angle
                Vector3 newOffset = _normalizedDirection * smoothedMagnitude;
                _follow.FollowOffset = newOffset;
            }
            else
            {
                // Alternative: Only scale Y and Z, keep X the same (if you have side offset)
                Vector3 newOffset = _follow.FollowOffset;
                newOffset.y = defaultY * (smoothedMagnitude / _originalMagnitude);
                newOffset.z = defaultZ * (smoothedMagnitude / _originalMagnitude);
                _follow.FollowOffset = newOffset;
            }
        }
    
        // Optional: Public method to set zoom programmatically (0 = min, 1 = max)
        public void SetZoomNormalized(float normalizedValue)
        {
            _targetDistance = Mathf.Lerp(minDistance, maxDistance, normalizedValue);
        }
    
        // Optional: Get current normalized zoom value
        public float GetZoomNormalized()
        {
            float currentDistance = _follow?.FollowOffset.magnitude ?? minDistance;
            return Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
        }
    }
}