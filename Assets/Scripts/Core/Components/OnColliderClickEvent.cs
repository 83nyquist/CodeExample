using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Core.Components
{
    public class OnColliderClickEvent : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private UnityEvent onClick;
    
        public void OnPointerDown(PointerEventData eventData)
        {
            onClick.SafeInvoke();
        }
    }
}
