using System;
using System.Collections.Generic;
using Systems.EventBus.Interfaces;

namespace Systems.EventBus.Components
{
    public class EventBusEngine : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventSubscribers = new();

        public void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (!_eventSubscribers.ContainsKey(eventType))
                _eventSubscribers.Add(eventType, new List<Delegate>());

            if (!_eventSubscribers[eventType].Contains(listener))
                _eventSubscribers[eventType].Add(listener);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (_eventSubscribers.ContainsKey(eventType))
            {
                _eventSubscribers[eventType].Remove(listener);
                if (_eventSubscribers[eventType].Count == 0)
                    _eventSubscribers.Remove(eventType);
            }
        }

        public void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (_eventSubscribers.TryGetValue(eventType, out var listeners))
            {
                List<Delegate> listenersCopy = new List<Delegate>(listeners);
                foreach (Delegate listener in listenersCopy)
                {
                    (listener as Action<TEvent>)?.Invoke(eventToPublish);
                }
            }
        }

        public void Subscribe(Type eventType, Delegate listener)
        {
            if (!_eventSubscribers.ContainsKey(eventType))
                _eventSubscribers.Add(eventType, new List<Delegate>());

            if (!_eventSubscribers[eventType].Contains(listener))
                _eventSubscribers[eventType].Add(listener);
        }

        public void Unsubscribe(Type eventType, Delegate listener)
        {
            if (_eventSubscribers.ContainsKey(eventType))
            {
                _eventSubscribers[eventType].Remove(listener);
                if (_eventSubscribers[eventType].Count == 0)
                    _eventSubscribers.Remove(eventType);
            }
        }

        public int GetSubscriberCount<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);
            return _eventSubscribers.TryGetValue(eventType, out var listeners) ? listeners.Count : 0;
        }
    }
}
