using System;
using System.Collections.Generic;
using System.IO;
using Systems.EventBus.Components;
using Systems.EventBus.Enums;
using Systems.EventBus.Events;
using UnityEngine;

namespace Systems.EventBus.BaseClasses
{
    public abstract class EventBusSubscriber : MonoBehaviour
    {
        [Header("Event Bus Settings")]
        [SerializeField] protected EventBusLogLevel logLevel = EventBusLogLevel.Warning;

        private List<(Type type, Delegate handler)> _subscriptions = new();

        protected virtual EventBusLogLevel GetLogLevel() => logLevel;

        protected void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            Type eventType = typeof(TEvent);
            if (IsSubscribed(handler)) return;

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

        protected void Publish<TEvent>(TEvent eventToPublish,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "",
            [System.Runtime.CompilerServices.CallerFilePath] string file = "") where TEvent : class
        {
            if (eventToPublish is GameEvent gameEvent)
            {
                gameEvent.Source = Path.GetFileNameWithoutExtension(file);
                gameEvent.SourceMember = caller;
                gameEvent.Timestamp = DateTime.UtcNow;
            }

            if (GetLogLevel() >= EventBusLogLevel.Verbose)
            {
                Debug.Log($"[EventBus] {Path.GetFileNameWithoutExtension(file)}.{caller} published {typeof(TEvent).Name}");
            }

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

        private void UnsubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Unsubscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
            _subscriptions.Clear();
        }

        private void ResubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Subscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
        }

        protected virtual void OnEnable() => ResubscribeAll();
        protected virtual void OnDisable() => UnsubscribeAll();
        protected virtual void OnDestroy() => UnsubscribeAll();
    }
}
