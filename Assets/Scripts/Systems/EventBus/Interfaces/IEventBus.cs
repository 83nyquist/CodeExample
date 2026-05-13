using System;

namespace Systems.EventBus.Interfaces
{
    public interface IEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : class;
        void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : class;
        void Publish<TEvent>(TEvent eventToPublish) where TEvent : class;

        void Subscribe(Type eventType, Delegate listener);
        void Unsubscribe(Type eventType, Delegate listener);
    }
}
