using UnityEngine;

namespace Core.Components
{
    public class Billboard : MonoBehaviour
    {
        private UnityEngine.Camera _mainCamera;

        void Start()
        {
            // Cache reference to the main camera
            _mainCamera = UnityEngine.Camera.main;
        }
        
        void LateUpdate()
        {
            if (_mainCamera == null)
            {
                return; // Stop if no camera is found
            }

            // Make the health bar always face the camera orthogonally
            Vector3 cameraForward = _mainCamera.transform.rotation * Vector3.forward; // Forward direction of the camera
            Vector3 cameraUp = _mainCamera.transform.rotation * Vector3.up;          // Up direction of the camera

            // Apply the rotation to make the Canvas face the camera directly
            transform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
        }
    }
}