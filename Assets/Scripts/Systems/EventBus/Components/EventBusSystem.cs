using Systems.EventBus.Interfaces;

namespace Systems.EventBus.Components
{
    public static class EventBusSystem
    {
        private static readonly EventBusEngine _instance = new(); 

        public static IEventBus Instance => _instance;

        public static int GetSubscriberCount<TEvent>() where TEvent : class
        {
            return _instance.GetSubscriberCount<TEvent>();
        }
    }
}
