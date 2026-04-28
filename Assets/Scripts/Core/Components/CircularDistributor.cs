using System.Collections.Generic;
using UnityEngine;

namespace Core.Components
{
    public class CircularDistributor : MonoBehaviour
    {
        [SerializeField] private List<Transform> nodes;
        [SerializeField] private float radius;
        [SerializeField] private float size;
        [SerializeField, Range(0,1)] private float offset;

        public void Distribute(float circleRadius, float circleRotationOffset, float nodeSize)
        {
            int i = 0;
            foreach (Transform node in nodes)
            {
                node.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
                
                float rotationOffset = (float)i / (float)nodes.Count;
                rotationOffset += circleRotationOffset;
                float x = Mathf.Sin( rotationOffset * Mathf.PI * 2.0f ) * circleRadius;
                float y = Mathf.Cos( rotationOffset * Mathf.PI * 2.0f ) * circleRadius;
            
                node.localPosition = new Vector3(x, y, 0);
                i++;
            }
        }
    
        public void Distribute()
        {
            Distribute(radius, offset, size);
        }
    }
}
