using Unity.Cinemachine;
using UnityEngine;

namespace Core.Components
{
    public class CameraZoomController : MonoBehaviour
    {
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

        private void Awake()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = GetComponent<CinemachineCamera>();
                if (cinemachineCamera == null)
                    cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
            }

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
        }

        public void OnScroll(float scrollDelta)
        {
            float zoomDelta = scrollDelta * zoomSpeed;
            _targetDistance -= zoomDelta;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }

        public void Tick()
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
