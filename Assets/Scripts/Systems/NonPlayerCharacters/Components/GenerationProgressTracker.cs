using System;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Interfaces;
using Systems.EventBus.Events;
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

        public GenerationProgressTracker(IEventBus eventBus) : base(eventBus)
        {
            Subscribe<GenerationProgressInitializedEvent>(OnInitialize);
            Subscribe<ReportWorkProgressRequest>(OnReportProgress);
        }

        private void OnInitialize(GenerationProgressInitializedEvent e)
        {
            totalTileWorkUnits = e.TotalTileWorkUnits;
            totalNpcWorkUnits = e.TotalNpcWorkUnits;
            completedTileWorkUnits = 0;
            completedNpcWorkUnits = 0;
            PublishProgress();
        }

        private void OnReportProgress(ReportWorkProgressRequest e)
        {
            completedTileWorkUnits += e.AmountTiles;
            completedNpcWorkUnits += e.AmountNpc;
            PublishProgress();
        }

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
