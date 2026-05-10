using UnityEngine;

namespace Input
{
    /// <summary>
    /// Generic raycaster that supports both 3D and 2D physics.
    /// Returns any hit collider (or component of type T) - completely decoupled from any specific game system.
    /// </summary>
    public class LayerMaskRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask layerMaskToHit = ~0;

        /// <summary>
        /// Initializes the camera reference to the main camera if not set.
        /// </summary>
        private void Awake()
        {
            if (inputCamera == null)
                inputCamera = Camera.main;
        }

        /// <summary>
        /// Returns true if raycast hits anything.
        /// </summary>
        public bool Raycast(Vector2 mousePosition, out RaycastHitData hitData)
        {
            hitData = default;
            
            if (inputCamera == null) return false;
            
            Ray ray = inputCamera.ScreenPointToRay(mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity, layerMaskToHit))
            {
                hitData = new RaycastHitData(hit3D.collider, hit3D.point, hit3D.collider.gameObject);
                return true;
            }
            
            Vector2 origin = new Vector2(ray.origin.x, ray.origin.y);
            Vector2 direction = new Vector2(ray.direction.x, ray.direction.y);
            RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, Mathf.Infinity, layerMaskToHit);
            
            if (hit2D.collider != null)
            {
                hitData = new RaycastHitData(hit2D.collider, hit2D.point, hit2D.collider.gameObject);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Returns component of type T from the hit object (if any).
        /// </summary>
        public T Raycast<T>(Vector2 mousePosition) where T : Component
        {
            if (Raycast(mousePosition, out RaycastHitData hitData))
            {
                return hitData.GameObject.GetComponentInParent<T>();
            }
            return null;
        }
        
        /// <summary>
        /// Returns the raw GameObject hit by the raycast.
        /// </summary>
        public GameObject RaycastGameObject(Vector2 mousePosition)
        {
            if (Raycast(mousePosition, out RaycastHitData hitData))
                return hitData.GameObject;
            return null;
        }
    }
    
    /// <summary>
    /// Unified hit data for both 3D and 2D raycasts.
    /// </summary>
    public readonly struct RaycastHitData
    {
        public readonly Collider Collider3D;
        public readonly Collider2D Collider2D;
        public readonly Vector3 Point;
        public readonly GameObject GameObject;
        
        /// <summary>
        /// Initializes hit data from a 3D raycast.
        /// </summary>
        public RaycastHitData(Collider collider, Vector3 point, GameObject gameObject)
        {
            Collider3D = collider;
            Collider2D = null;
            Point = point;
            GameObject = gameObject;
        }
        
        /// <summary>
        /// Initializes hit data from a 2D raycast.
        /// </summary>
        public RaycastHitData(Collider2D collider, Vector2 point, GameObject gameObject)
        {
            Collider3D = null;
            Collider2D = collider;
            Point = point;
            GameObject = gameObject;
        }
        
        public bool IsValid => GameObject != null;
    }
}