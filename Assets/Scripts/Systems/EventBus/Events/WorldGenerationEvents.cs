namespace Systems.EventBus.Events
{
    public class WorldGenerationStartedEvent : GameEvent { }

    public class WorldGenerationFinishedEvent : GameEvent { }

    public class WorldVisualsReadyEvent : GameEvent { }

    public class GenerationProgressInitializedEvent : GameEvent
    {
        public int TotalTileWorkUnits { get; }
        public int TotalNpcWorkUnits { get; }
        public GenerationProgressInitializedEvent(int totalTileWorkUnits, int totalNpcWorkUnits)
        {
            TotalTileWorkUnits = totalTileWorkUnits;
            TotalNpcWorkUnits = totalNpcWorkUnits;
        }
    }

    public class GenerationProgressUpdatedEvent : GameEvent
    {
        public float Progress { get; }
        public float CompletedTileWorkUnits { get; }
        public float CompletedNpcWorkUnits { get; }
        public float TotalTileWorkUnits { get; }
        public float TotalNpcWorkUnits { get; }
        public GenerationProgressUpdatedEvent(float progress, float completedTileWorkUnits, float totalTileWorkUnits, float completedNpcWorkUnits, float totalNpcWorkUnits)
        {
            Progress = progress;
            CompletedNpcWorkUnits = completedNpcWorkUnits;
            TotalNpcWorkUnits = totalNpcWorkUnits;
            CompletedTileWorkUnits = completedTileWorkUnits;
            TotalTileWorkUnits = totalTileWorkUnits;
        }
    }

    public class ReportWorkProgressRequest : GameEvent
    {
        public int AmountTiles { get; }
        public int AmountNpc { get; }
        public ReportWorkProgressRequest(int amountTiles, int amountNpc)
        {
            AmountTiles = amountTiles;
            AmountNpc = amountNpc;
        }
    }

    public class WorldCleanupEvent : GameEvent { }
}
