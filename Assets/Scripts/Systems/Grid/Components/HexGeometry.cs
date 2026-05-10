using System.Collections.Generic;
using Core.Enumerations;
using UnityEngine;

namespace Systems.Grid.Components
{
    public static class HexGeometry
    {
        /// <summary>
        /// Generates axial coordinates in a spiral pattern within a specified ring range.
        /// </summary>
        /// <returns>An enumerable of Vector2Int coordinates (Q, R).</returns>
        public static IEnumerable<Vector2Int> GetCoordinatesInRingRange(int startRadius, int endRadius)
        {
            if (startRadius == 0)
            {
                yield return Vector2Int.zero;
                startRadius = 1;
            }

            for (int k = startRadius; k <= endRadius; k++)
            {
                Vector2Int current = new Vector2Int(0, -k);
                Vector2Int[] directions = {
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, -1)
                };

                foreach (var dir in directions)
                {
                    for (int i = 0; i < k; i++)
                    {
                        yield return current;
                        current += dir;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the coordinate of a neighbor in a specific axial direction.
        /// </summary>
        public static Vector2Int GetNeighborCoordinate(int x, int z, Directions.Axial direction)
        {
            return direction switch
            {
                Directions.Axial.East => new Vector2Int(x + 1, z),
                Directions.Axial.NorthEast => new Vector2Int(x + 1, z - 1),
                Directions.Axial.NorthWest => new Vector2Int(x, z - 1),
                Directions.Axial.West => new Vector2Int(x - 1, z),
                Directions.Axial.SouthWest => new Vector2Int(x - 1, z + 1),
                Directions.Axial.SouthEast => new Vector2Int(x, z + 1),
                _ => throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}