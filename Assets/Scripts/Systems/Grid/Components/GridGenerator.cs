using System;
using System.Collections;
using System.Collections.Generic;
using Systems.Coordinators;
using UnityEngine;

namespace Systems.Grid.Components
{
    public class GridGenerator
    {
        private readonly GenerationProgressTracker _progressTracker;
        private readonly float _maxMsPerFrame;

        public GridGenerator(GenerationProgressTracker progressTracker, float maxMsPerFrame)
        {
            _progressTracker = progressTracker;
            _maxMsPerFrame = maxMsPerFrame;
        }

        public IEnumerator CreateDataRoutine(AxialHexGrid grid, int radius)
        {
            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                coord => grid.CreateTileData(coord.x, coord.y),
                GenerationProgressTracker.TaskTiles
            );
        }

        public IEnumerator BuildNeighborsRoutine(AxialHexGrid grid, int radius)
        {
            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                axialCoord => {
                TileData data = grid.GetTile(axialCoord);
                if (data == null) return;

                TileData[] neighbours = new TileData[6];
                for (int i = 0; i < 6; i++)
                {
                    Vector2Int neighborCoord = data.GetNeighborCoordinate(i);
                    neighbours[i] = grid.GetTile(neighborCoord);
                }
                data.SetNeighbours(neighbours);
            }, GenerationProgressTracker.TaskNeighbors);
        }

        private IEnumerator ProcessInBatches<T>(IEnumerable<T> items, Action<T> action, string workUnit)
        {
            float budgetSeconds = _maxMsPerFrame / 1000f;
            float lastYieldTime = Time.realtimeSinceStartup;
            int batchCount = 0;

            foreach (var item in items)
            {
                action(item);
                batchCount++;

                if (batchCount % 50 == 0 && Time.realtimeSinceStartup - lastYieldTime > budgetSeconds)
                {
                    _progressTracker.UpdateProgress(workUnit, batchCount);
                    batchCount = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            if (batchCount > 0)
                _progressTracker.UpdateProgress(workUnit, batchCount);
        }
    }
}