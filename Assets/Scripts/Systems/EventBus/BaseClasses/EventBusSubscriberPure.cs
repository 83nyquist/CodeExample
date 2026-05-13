using System;
using System.Collections.Generic;
using Systems.EventBus.Components;
using UnityEngine;

namespace Systems.EventBus.BaseClasses
{
    public abstract class EventBusSubscriberPure : IDisposable
    {
        private List<(Type type, Delegate handler)> _subscriptions = new();

        protected void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            Type eventType = typeof(TEvent);
            if (IsSubscribed(handler))
            {
                Debug.LogWarning($"[EventBusSubscriberPure] Already subscribed to {eventType.Name}. Skipping duplicate.");
                return;
            }

            EventBusSystem.Subscribe(handler);
            _subscriptions.Add((eventType, handler));
        }

        protected void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            Type eventType = typeof(TEvent);
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                if (_subscriptions[i].type == eventType && _subscriptions[i].handler == handler as Delegate)
                {
                    _subscriptions.RemoveAt(i);
                    break;
                }
            }
            EventBusSystem.Unsubscribe(handler);
        }

        protected void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            EventBusSystem.Publish(eventToPublish);
        }

        private bool IsSubscribed<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            foreach (var (type, existingHandler) in _subscriptions)
            {
                if (type == eventType && existingHandler == handler as Delegate) return true;
            }
            return false;
        }

        protected virtual void UnsubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Unsubscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
            _subscriptions.Clear();
        }

        public virtual void Dispose()
        {
            UnsubscribeAll();
        }
    }
}
