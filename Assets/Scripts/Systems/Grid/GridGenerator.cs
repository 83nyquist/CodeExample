using System;
using System.Collections;
using System.Collections.Generic;
using Core.Enumerations;
using UnityEngine;

namespace Systems.Grid
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
                WorkUnitTypes.Tiles
            );
        }

        public IEnumerator BuildNeighborsRoutine(AxialHexGrid grid, int radius)
        {
            var directions = (Directions.Axial[])Enum.GetValues(typeof(Directions.Axial));

            return ProcessInBatches(
                HexGeometry.GetCoordinatesInRingRange(0, radius),
                axialCoord => {
                TileData data = grid.GetTile(axialCoord);
                if (data == null) return;

                var res = new Dictionary<Directions.Axial, TileData>();
                foreach (var direction in directions)
                {
                    TileData neighbor = grid.GetTile(data.GetNeighborCoordinate(direction));
                    if (neighbor != null)
                        res[direction] = neighbor;
                }
                data.SetNeighbours(res);
            }, WorkUnitTypes.Neighbors);
        }

        private IEnumerator ProcessInBatches<T>(IEnumerable<T> items, Action<T> action, WorkUnitTypes unitType)
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
                    _progressTracker.UpdateProgress(unitType, batchCount);
                    batchCount = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            if (batchCount > 0)
                _progressTracker.UpdateProgress(unitType, batchCount);
        }
    }
}