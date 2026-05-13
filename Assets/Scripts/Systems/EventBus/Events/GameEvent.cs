using System;

namespace Systems.EventBus.Events
{
    public abstract class GameEvent
    {
        public string Source { get; set; }
        public string SourceMember { get; set; }
        public DateTime Timestamp { get; set; }

        protected GameEvent()
        {
            Source = "Unknown";
            SourceMember = "Unknown";
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"[{GetType().Name}] from {Source}.{SourceMember} at {Timestamp:T}";
        }
    }
}
