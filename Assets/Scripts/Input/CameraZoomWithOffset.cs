using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class CameraZoomWithOffset : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float minOffsetZ = 3f;    // Minimum distance (close)
        [SerializeField] private float maxOffsetZ = 15f;   // Maximum distance (far)
    
        private CinemachineFollow _follow;
        private Vector3 _originalOffset;
    
        private void Awake()
        {
            if (cinemachineCamera == null)
                cinemachineCamera = GetComponent<CinemachineCamera>();
        
            _follow = cinemachineCamera.GetComponent<CinemachineFollow>();
            _originalOffset = _follow.FollowOffset;
        }
    
        private void Update()
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
        
            if (!Mathf.Approximately(scrollDelta, 0f))
            {
                // Zoom by modifying the Z component of the offset
                float zoomDelta = scrollDelta * zoomSpeed;
                Vector3 newOffset = _follow.FollowOffset;
                newOffset.z -= zoomDelta;  // Negative delta for zoom in
                newOffset.z = Mathf.Clamp(newOffset.z, minOffsetZ, maxOffsetZ);
                _follow.FollowOffset = newOffset;
            }
        }
    }
}