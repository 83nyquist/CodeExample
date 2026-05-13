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

    public class VisionSetUpdatedEvent : GameEvent
    {
        public System.Collections.Generic.HashSet<Systems.Grid.Components.TileData> VisionSet { get; }
        public VisionSetUpdatedEvent(System.Collections.Generic.HashSet<Systems.Grid.Components.TileData> visionSet) => VisionSet = visionSet;
    }
}
