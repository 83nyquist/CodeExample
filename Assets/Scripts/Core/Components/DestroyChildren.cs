using Core.Attributes;
using UnityEngine;

namespace Core.Components
{
    public class DestroyChildren : MonoBehaviour
    {
        [InspectorButton("Activate")]
        public bool activate;
        
        public void Activate()
        {
            if (transform != null)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        Destroy(transform.GetChild(i).gameObject);
                    else
                        DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
    }
}
