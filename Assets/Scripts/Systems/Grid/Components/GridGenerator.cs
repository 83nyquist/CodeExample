using System;
using System.Collections;
using System.Collections.Generic;
using Systems.EventBus.Interfaces;
using Systems.EventBus.Events;
using UnityEngine;

namespace Systems.Grid.Components
{
    public class GridGenerator
    {
        private readonly float _maxMsPerFrame;
        private readonly IEventBus _eventBus;

        public GridGenerator(float maxMsPerFrame, IEventBus eventBus)
        {
            _maxMsPerFrame = maxMsPerFrame;
            _eventBus = eventBus;
        }

        public IEnumerator CreateDataRoutine(AxialHexGrid grid, int radius, int totalTiles)
        {
            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                totalTiles,
                coord => grid.CreateTileData(coord.x, coord.y)
            );
        }

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
                    _eventBus.Publish(new ReportWorkProgressRequest(batchCount, 0));
                    batchCount = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            if (batchCount > 0)
            {
                _eventBus.Publish(new ReportWorkProgressRequest(batchCount, 0));
            }
        }
    }
}
