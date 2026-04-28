using UnityEngine;

namespace Core.Components
{
    public class DeactivateOnPlay : MonoBehaviour
    {
        public bool OnAwake;
        public bool OnStart;

        void Awake()
        {
            if (OnAwake)
            {
                gameObject.SetActive(false);
            }
        }

        void Start ()
        {
            if (OnStart)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
