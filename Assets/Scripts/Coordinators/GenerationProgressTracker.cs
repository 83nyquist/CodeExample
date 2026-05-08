using System;
using Systems.EventBus;
using UnityEngine;

namespace Coordinators
{
    [Serializable]
    public class GenerationProgressTracker : EventBusSubscriber
    {
        [SerializeField] private int totalTileWorkUnits;
        [SerializeField] private int totalNpcWorkUnits;
        [SerializeField] private int completedTileWorkUnits;
        [SerializeField] private int completedNpcWorkUnits;

        public GenerationProgressTracker()
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
    
    // public class GenerationProgressTracker : MonoBehaviour
    // {
    //     // Constant keys for core tasks
    //     public const string TaskTiles = "Tiles";
    //     public const string TaskNeighbors = "Neighbors";
    //     public const string TaskAgents = "Agents";
    //     
    //     private int _currentWorkUnits;
    //     private int _estimatedTotalWorkUnits;
    //     private int _tilesInGrid;
    //
    //     public event Action<int> OnInitialized;
    //     public event Action<int, int, string> OnProgressUpdated;
    //
    //     public void Initialize(int radius, int populationSize, IEnumerable<string> passNames)
    //     {
    //         _tilesInGrid = CalculateTotalTiles(radius);
    //         
    //         // Base Work: Data Creation (Tiles) + Linkage (Neighbors)
    //         int baseWork = _tilesInGrid * 2;
    //         int agentWork = populationSize;
    //         
    //         // Dynamic Work: Every pass iterates over the grid (or significant portions)
    //         // We treat one pass as 1 work unit per tile.
    //         int passWork = passNames.Count() * _tilesInGrid;
    //
    //         _estimatedTotalWorkUnits = baseWork + agentWork + passWork;
    //         _currentWorkUnits = 0;
    //         
    //         OnInitialized?.Invoke(_estimatedTotalWorkUnits);
    //     }
    //
    //     public void UpdateProgress(string taskName, int amount = 1)
    //     {
    //         _currentWorkUnits += amount;
    //         OnProgressUpdated?.Invoke(_currentWorkUnits, _estimatedTotalWorkUnits, taskName);
    //     }
    //
    //     /// <summary>
    //     /// Marks a full pass as complete by adding the total tile count to progress.
    //     /// </summary>
    //     public void CompletePass(string passName)
    //     {
    //         UpdateProgress(passName, _tilesInGrid);
    //     }
    //     
    //     public int CalculateTotalTiles(int radius)
    //     {
    //         return 3 * radius * radius + 3 * radius + 1;
    //     }
    // }
}