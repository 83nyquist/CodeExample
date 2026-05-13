namespace Systems.EventBus.Events
{
    public class NpcSimulationCompleteEvent : GameEvent
    {
        public int TotalAgents { get; }
        public NpcSimulationCompleteEvent(int totalAgents) => TotalAgents = totalAgents;
    }

    public class NpcVisibleAgentsCountChangedEvent : GameEvent
    {
        public int VisibleCount { get; }
        public NpcVisibleAgentsCountChangedEvent(int visibleCount) => VisibleCount = visibleCount;
    }
}
