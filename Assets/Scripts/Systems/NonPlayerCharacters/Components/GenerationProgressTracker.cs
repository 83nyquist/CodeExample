using System;
using Systems.EventBus;
using UnityEngine;

namespace Systems.NonPlayerCharacters.Components
{
    [Serializable]
    public class GenerationProgressTracker : EventBusSubscriberPure
    {
        [SerializeField] private int totalTileWorkUnits;
        [SerializeField] private int totalNpcWorkUnits;
        [SerializeField] private int completedTileWorkUnits;
        [SerializeField] private int completedNpcWorkUnits;

        /// <summary>
        /// Initializes the tracker and subscribes to progress-related events.
        /// </summary>
        public GenerationProgressTracker()
        {
            Subscribe<GenerationProgressInitializedEvent>(OnInitialize);
            Subscribe<ReportWorkProgressRequest>(OnReportProgress);
        }

        /// <summary>
        /// Sets up total work units for a new generation cycle.
        /// </summary>
        private void OnInitialize(GenerationProgressInitializedEvent e)
        {
            totalTileWorkUnits = e.TotalTileWorkUnits;
            totalNpcWorkUnits = e.TotalNpcWorkUnits;
            completedTileWorkUnits = 0;
            completedNpcWorkUnits = 0;
            PublishProgress();
        }

        /// <summary>
        /// Updates completed work units based on incoming progress reports.
        /// </summary>
        private void OnReportProgress(ReportWorkProgressRequest e)
        {
            completedTileWorkUnits += e.AmountTiles;
            completedNpcWorkUnits += e.AmountNpc;
            PublishProgress();
        }

        /// <summary>
        /// Calculates the percentage of total progress and publishes an update event.
        /// </summary>
        private void PublishProgress()
        {
            int completedWorkUnits = completedTileWorkUnits + completedNpcWorkUnits;
            int totalWorkUnits = totalTileWorkUnits + totalNpcWorkUnits;
            
            float progress = totalWorkUnits > 0 ? (float)completedWorkUnits / totalWorkUnits : 0f;
            float progressPercent = progress * 100f; 
            
            Publish(new GenerationProgressUpdatedEvent(progressPercent, completedTileWorkUnits, totalTileWorkUnits, completedNpcWorkUnits, totalNpcWorkUnits));
        }
    }
}