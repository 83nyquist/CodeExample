using System;
using System.Collections;
using UnityEngine;

namespace Core.Components
{
    public class SafeEventBinder : MonoBehaviour
    {
        public static SafeEventBinder Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SafeSubscribe(MonoBehaviour host, Func<bool> condition, Action subscribe, Action unsubscribe)
        {
            StartCoroutine(BindWhenReady(host, condition, subscribe, unsubscribe));
        }

        private IEnumerator BindWhenReady(MonoBehaviour host, Func<bool> condition, Action subscribe, Action unsubscribe)
        {
            yield return new WaitUntil(() => condition());

            if (host != null && host.isActiveAndEnabled)
            {
                subscribe?.Invoke();

                // Ensure unsubscription on disable
                void HandleDisable() => unsubscribe?.Invoke();
                host.StartCoroutine(UnbindOnDisable(host, HandleDisable));
            }
        }

        private IEnumerator UnbindOnDisable(MonoBehaviour host, Action unbind)
        {
            yield return new WaitUntil(() => !host.isActiveAndEnabled);
            unbind?.Invoke();
        }
    }
}