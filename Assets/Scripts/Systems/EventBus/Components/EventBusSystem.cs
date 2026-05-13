using System;
using System.Collections.Generic;
using Systems.EventBus.Interfaces;

namespace Systems.EventBus.Components
{
    public static class EventBusSystem
    {
        private static readonly Dictionary<Type, List<Delegate>> EventSubscribers = new();

        private static IEventBus _instance;
        public static IEventBus Instance => _instance ??= new EventBusEngine();

        public static void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Instance.Subscribe(listener);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Instance.Unsubscribe(listener);
        }

        public static void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            Instance.Publish(eventToPublish);
        }

        public static int GetSubscriberCount<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);
            return EventSubscribers.TryGetValue(eventType, out var listeners) ? listeners.Count : 0;
        }

        private class EventBusEngine : IEventBus
        {
            public void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : class
            {
                Type eventType = typeof(TEvent);
                if (!EventSubscribers.ContainsKey(eventType))
                {
                    EventSubscribers.Add(eventType, new List<Delegate>());
                }

                if (!EventSubscribers[eventType].Contains(listener))
                {
                    EventSubscribers[eventType].Add(listener);
                }
            }

            public void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : class
            {
                Type eventType = typeof(TEvent);
                if (EventSubscribers.ContainsKey(eventType))
                {
                    EventSubscribers[eventType].Remove(listener);
                    if (EventSubscribers[eventType].Count == 0)
                    {
                        EventSubscribers.Remove(eventType);
                    }
                }
            }

            public void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
            {
                Type eventType = typeof(TEvent);
                if (EventSubscribers.TryGetValue(eventType, out var listeners))
                {
                    List<Delegate> listenersCopy = new List<Delegate>(listeners);
                    foreach (Delegate listener in listenersCopy)
                    {
                        (listener as Action<TEvent>)?.Invoke(eventToPublish);
                    }
                }
            }
        }
    }
}
