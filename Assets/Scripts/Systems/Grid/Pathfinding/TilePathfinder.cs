using System;
using System.Collections.Generic;
using Systems.Grid.Components;

namespace Systems.Grid.Pathfinding
{
    public static class TilePathfinder
    {
        public static List<TileData> FindPath(
            TileData origin,
            TileData target,
            Func<TileData, bool> canTraverse = null)
        {
            if (origin == null || target == null)
            {
                return null;
            }

            canTraverse ??= tile => tile != null;

            if (!canTraverse(origin) || !canTraverse(target))
            {
                return null;
            }

            Dictionary<TileData, PathRecord> records = new Dictionary<TileData, PathRecord>();
            List<TileData> openSet = new List<TileData>();
            HashSet<TileData> closedSet = new HashSet<TileData>();

            records[origin] = new PathRecord
            {
                Tile = origin,
                Parent = null,
                CostG = 0f,
                CostH = origin.DistanceTo(target)
            };

            openSet.Add(origin);

            while (openSet.Count > 0)
            {
                TileData current = GetLowestCostTile(openSet, records);

                if (current == target)
                {
                    return BuildPath(origin, target, records);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (TileData neighbour in current.Neighbours)
                {
                    if (neighbour == null)
                    {
                        continue;
                    }

                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    if (!canTraverse(neighbour))
                    {
                        continue;
                    }

                    float newCostG = records[current].CostG + 1f;

                    bool hasRecord = records.TryGetValue(neighbour, out PathRecord neighbourRecord);

                    if (!hasRecord)
                    {
                        neighbourRecord = new PathRecord
                        {
                            Tile = neighbour
                        };

                        records[neighbour] = neighbourRecord;
                    }

                    if (!hasRecord || newCostG < neighbourRecord.CostG)
                    {
                        neighbourRecord.Parent = current;
                        neighbourRecord.CostG = newCostG;
                        neighbourRecord.CostH = neighbour.DistanceTo(target);

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        private static TileData GetLowestCostTile(List<TileData> openSet, Dictionary<TileData, PathRecord> records)
        {
            TileData bestTile = openSet[0];
            PathRecord bestRecord = records[bestTile];

            for (int i = 1; i < openSet.Count; i++)
            {
                TileData tile = openSet[i];
                PathRecord record = records[tile];

                if (record.CostF < bestRecord.CostF ||
                    record.CostF == bestRecord.CostF && record.CostH < bestRecord.CostH)
                {
                    bestTile = tile;
                    bestRecord = record;
                }
            }

            return bestTile;
        }

        private static List<TileData> BuildPath(
            TileData origin,
            TileData target,
            Dictionary<TileData, PathRecord> records)
        {
            List<TileData> path = new List<TileData>();
            TileData current = target;

            while (current != null)
            {
                path.Add(current);

                if (current == origin)
                {
                    break;
                }

                current = records[current].Parent;
            }

            path.Reverse();
            return path;
        }
    }
}
