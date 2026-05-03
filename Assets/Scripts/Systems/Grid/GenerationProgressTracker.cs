using System;
using UnityEngine;

namespace Systems.Grid
{
    public enum WorkUnitTypes
    {
        Tiles,
        Neighbors,
        Agents
    }

    public class GenerationProgressTracker : MonoBehaviour
    {
        private int _totalTiles;
        private int _totalNeighbourHookups;
        private int _totalAgents;

        private int _currentTileData;
        private int _currentNeighbors;
        private int _currentAgents;
        
        private int _estimatedTotalWorkUnits;

        public event Action<int> OnInitialized;
        public event Action<int, int, WorkUnitTypes> OnProgressUpdated;

        public void Initialize(int radius, int populationSize)
        {
            _totalTiles = CalculateTotalTiles(radius);
            _totalNeighbourHookups = _totalTiles;
            _totalAgents = populationSize;

            _estimatedTotalWorkUnits = _totalTiles + _totalNeighbourHookups + _totalAgents;
            
            _currentTileData = 0;
            _currentNeighbors = 0;
            _currentAgents = 0;
            
            OnInitialized?.Invoke( _estimatedTotalWorkUnits);
        }

        public void UpdateProgress(WorkUnitTypes workUnitType, int amount = 1)
        {
            switch (workUnitType)
            {
                case WorkUnitTypes.Tiles:
                {
                    _currentTileData += amount;
                    break;
                }
                case WorkUnitTypes.Neighbors:
                {
                    _currentNeighbors += amount;
                    break;
                }
                case WorkUnitTypes.Agents:
                {
                    _currentAgents += amount;
                    break;
                }
            }

            int total = _currentTileData + _currentNeighbors + _currentAgents;
            OnProgressUpdated?.Invoke(total, _estimatedTotalWorkUnits, workUnitType);
        }
        
        public int CalculateTotalTiles(int radius)
        {
            return 3 * radius * radius + 3 * radius + 1;
        }
    }
}