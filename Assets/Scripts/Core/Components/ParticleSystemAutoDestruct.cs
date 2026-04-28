using UnityEngine;

namespace Core.Components
{
    public class ParticleSystemAutoDestruct : MonoBehaviour
    {
        private ParticleSystem ps;


        public void Start()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public void Update()
        {
            if (ps)
            {
                if (!ps.IsAlive())
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
