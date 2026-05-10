using System;
using System.Collections;
using System.Collections.Generic;
using Systems.EventBus;
using UnityEngine;

namespace Systems.Grid.Components
{
    public class GridGenerator
    {
        private readonly float _maxMsPerFrame;

        /// <summary>
        /// Initializes the generator with a specified performance budget per frame.
        /// </summary>
        public GridGenerator(float maxMsPerFrame)
        {
            _maxMsPerFrame = maxMsPerFrame;
        }

        /// <summary>
        /// Coroutine to create initial TileData structures for the grid in batches.
        /// </summary>
        public IEnumerator CreateDataRoutine(AxialHexGrid grid, int radius, int totalTiles)
        {
            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                totalTiles,
                coord => grid.CreateTileData(coord.x, coord.y)
            );
        }

        /// <summary>
        /// Coroutine to establish neighbor references for every tile in the grid in batches.
        /// </summary>
        public IEnumerator BuildNeighborsRoutine(AxialHexGrid grid, int radius, int totalTiles)
        {
            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                totalTiles,
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
                }
            );
        }

        /// <summary>
        /// Generic batch processor that executes an action over a collection while respecting a time budget.
        /// </summary>
        private IEnumerator ProcessInBatches<T>(IEnumerable<T> items, int totalCount, Action<T> action)
        {
            float budgetSeconds = _maxMsPerFrame / 1000f;
            float lastYieldTime = Time.realtimeSinceStartup;
            int batchCount = 0;
            int totalBatchProcessed = 0;

            foreach (var item in items)
            {
                action(item);
                batchCount++;
                totalBatchProcessed++;

                if (batchCount % 50 == 0 && Time.realtimeSinceStartup - lastYieldTime > budgetSeconds)
                {
                    EventBusSystem.Publish(new ReportWorkProgressRequest(batchCount, 0));
                    batchCount = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            if (batchCount > 0)
            {
                EventBusSystem.Publish(new ReportWorkProgressRequest(batchCount, 0));
            }
        }
    }
}