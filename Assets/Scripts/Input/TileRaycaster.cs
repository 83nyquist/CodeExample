using Systems.Decoration.Components;
using UnityEngine;

namespace Input
{
    public class TileRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask tileLayerMask = ~0;

        private void Awake()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }
        }

        public TileDecorator RaycastTileDecorator(Vector2 mousePosition)
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

            // Returns the TileDecorator component from the hit object or its parents
            return hit.collider.GetComponentInParent<TileDecorator>();
        }
    }
}