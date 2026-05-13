using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enumerations;
using NUnit.Framework;
using Systems.Grid.Components;
using UnityEngine;

namespace Tests.Editor.Systems.Grid
{
    public class HexGeometryTests
    {
        [Test]
        public void GetNeighborCoordinate_East_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.East);
            Assert.AreEqual(new Vector2Int(1, 0), result);
        }

        [Test]
        public void GetNeighborCoordinate_NorthEast_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.NorthEast);
            Assert.AreEqual(new Vector2Int(1, -1), result);
        }

        [Test]
        public void GetNeighborCoordinate_NorthWest_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.NorthWest);
            Assert.AreEqual(new Vector2Int(0, -1), result);
        }

        [Test]
        public void GetNeighborCoordinate_West_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.West);
            Assert.AreEqual(new Vector2Int(-1, 0), result);
        }

        [Test]
        public void GetNeighborCoordinate_SouthWest_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.SouthWest);
            Assert.AreEqual(new Vector2Int(-1, 1), result);
        }

        [Test]
        public void GetNeighborCoordinate_SouthEast_ReturnsCorrectOffset()
        {
            Vector2Int result = HexGeometry.GetNeighborCoordinate(0, 0, Directions.Axial.SouthEast);
            Assert.AreEqual(new Vector2Int(0, 1), result);
        }

        [Test]
        public void GetNeighborCoordinate_AllDirections_RoundTripToOrigin()
        {
            Vector2Int origin = new Vector2Int(5, -3);
            foreach (Directions.Axial dir in Enum.GetValues(typeof(Directions.Axial)))
            {
                Vector2Int neighbor = HexGeometry.GetNeighborCoordinate(origin.x, origin.y, dir);
                Directions.Axial opposite = GetOppositeDirection(dir);
                Vector2Int back = HexGeometry.GetNeighborCoordinate(neighbor.x, neighbor.y, opposite);
                Assert.AreEqual(origin, back);
            }
        }

        [Test]
        public void GetNeighborCoordinate_InvalidDirection_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => HexGeometry.GetNeighborCoordinate(0, 0, (Directions.Axial)99));
        }

        [Test]
        public void GetCoordinatesInRingRange_Start0End0_ReturnsOrigin()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(0, 0).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Vector2Int.zero, result[0]);
        }

        [Test]
        public void GetCoordinatesInRingRange_Start1End1_ReturnsSixTiles()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(1, 1).ToList();
            Assert.AreEqual(6, result.Count);
        }

        [Test]
        public void GetCoordinatesInRingRange_Start0End1_ReturnsSevenTiles()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(0, 1).ToList();
            Assert.AreEqual(7, result.Count);
        }

        [Test]
        public void GetCoordinatesInRingRange_Start0End2_ReturnsNineteenTiles()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(0, 2).ToList();
            Assert.AreEqual(19, result.Count);
        }

        [Test]
        public void GetCoordinatesInRingRange_Ring2_HasTwelveTiles()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(2, 2).ToList();
            Assert.AreEqual(12, result.Count);
        }

        [Test]
        public void GetCoordinatesInRingRange_RingK_HasCorrectCount()
        {
            for (int k = 1; k <= 5; k++)
            {
                List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(k, k).ToList();
                Assert.AreEqual(6 * k, result.Count, $"Ring {k} should have {6 * k} tiles");
            }
        }

        [Test]
        public void GetCoordinatesInRingRange_NoDuplicateCoordinates()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(0, 5).ToList();
            Assert.AreEqual(result.Count, result.Distinct().Count());
        }

        [Test]
        public void GetCoordinatesInRingRange_AllCoordinates_HaveCorrectHexDistance()
        {
            for (int ring = 1; ring <= 4; ring++)
            {
                foreach (Vector2Int coord in HexGeometry.GetCoordinatesInRingRange(ring, ring))
                {
                    float distance = HexDistance(Vector2Int.zero, coord);
                    Assert.AreEqual(ring, distance, $"Coordinate {coord} in ring {ring} has distance {distance}");
                }
            }
        }

        [Test]
        public void GetCoordinatesInRingRange_StartRadiusGreaterThanEndRadius_ReturnsEmpty()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(3, 2).ToList();
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetCoordinatesInRingRange_NegativeRadii_ReturnsEmpty()
        {
            List<Vector2Int> result = HexGeometry.GetCoordinatesInRingRange(-2, -1).ToList();
            Assert.IsEmpty(result);
        }

        private static Directions.Axial GetOppositeDirection(Directions.Axial direction)
        {
            return direction switch
            {
                Directions.Axial.East => Directions.Axial.West,
                Directions.Axial.NorthEast => Directions.Axial.SouthWest,
                Directions.Axial.NorthWest => Directions.Axial.SouthEast,
                Directions.Axial.West => Directions.Axial.East,
                Directions.Axial.SouthWest => Directions.Axial.NorthEast,
                Directions.Axial.SouthEast => Directions.Axial.NorthWest,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private static float HexDistance(Vector2Int a, Vector2Int b)
        {
            Vector3Int cubeA = new Vector3Int(a.x, -a.x - a.y, a.y);
            Vector3Int cubeB = new Vector3Int(b.x, -b.x - b.y, b.y);
            return (Math.Abs(cubeA.x - cubeB.x) + Math.Abs(cubeA.y - cubeB.y) + Math.Abs(cubeA.z - cubeB.z)) / 2f;
        }
    }
}
