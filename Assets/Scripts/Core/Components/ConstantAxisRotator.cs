using UnityEngine;

namespace Core.Components
{
    public class ConstantAxisRotator : MonoBehaviour
    {
        public Vector3 rotation;
        
        private void Update()
        {
            transform.Rotate(rotation * (Time.deltaTime * 360f), Space.Self);
        }
    }
}
