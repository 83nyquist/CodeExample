using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Systems.Grid.Components;
using Systems.Grid.Pathfinding;

namespace Tests.Editor.Systems.Grid
{
    public class TilePathfinderTests
    {
        [Test]
        public void NullOrigin_ReturnsNull()
        {
            TileData target = new TileData(1, 0);
            Assert.IsNull(TilePathfinder.FindPath(null, target));
        }

        [Test]
        public void NullTarget_ReturnsNull()
        {
            TileData origin = new TileData(0, 0);
            Assert.IsNull(TilePathfinder.FindPath(origin, null));
        }

        [Test]
        public void BothNull_ReturnsNull()
        {
            Assert.IsNull(TilePathfinder.FindPath(null, null));
        }

        [Test]
        public void OriginEqualsTarget_ReturnsSingleTilePath()
        {
            TileData tile = new TileData(0, 0);
            List<TileData> path = TilePathfinder.FindPath(tile, tile);
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Count);
            Assert.AreSame(tile, path[0]);
        }

        [Test]
        public void DirectNeighbor_ReturnsTwoTilePath()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, b);
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Count);
            Assert.AreSame(a, path[0]);
            Assert.AreSame(b, path[1]);
        }

        [Test]
        public void LinearThreeTiles_FindsPath()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { null, null, null, b, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, c);
            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Count);
            Assert.AreSame(a, path[0]);
            Assert.AreSame(b, path[1]);
            Assert.AreSame(c, path[2]);
        }

        [Test]
        public void UnreachableTarget_ReturnsNull()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });
            c.SetNeighbours(new TileData[6]);

            Assert.IsNull(TilePathfinder.FindPath(a, c));
        }

        [Test]
        public void CanTraverseBlocksOrigin_ReturnsNull()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });

            Assert.IsNull(TilePathfinder.FindPath(a, b, tile => false));
        }

        [Test]
        public void CanTraverseBlocksTarget_ReturnsNull()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, b, tile => tile != b);
            Assert.IsNull(path);
        }

        [Test]
        public void BlockedIntermediateTile_FindsAlternativePath()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);
            TileData d = new TileData(0, 1);
            TileData e = new TileData(1, 1);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, d });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { null, null, null, b, e, null });
            d.SetNeighbours(new TileData[6] { e, null, a, null, null, null });
            e.SetNeighbours(new TileData[6] { null, c, null, d, null, null });

            HashSet<TileData> blocked = new HashSet<TileData>();
            List<TileData> path = TilePathfinder.FindPath(a, c, tile => !blocked.Contains(tile));

            Assert.IsNotNull(path);
            Assert.AreSame(a, path[0]);
            Assert.AreSame(c, path[^1]);
            Assert.IsTrue(IsContinuousPath(path));
        }

        [Test]
        public void ChoosesShortestPath()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);
            TileData d = new TileData(0, 1);
            TileData e = new TileData(1, 1);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, d });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { null, null, null, b, e, null });
            d.SetNeighbours(new TileData[6] { e, null, a, null, null, null });
            e.SetNeighbours(new TileData[6] { null, c, null, d, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, c);
            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Count);
            Assert.AreSame(a, path[0]);
            Assert.AreSame(b, path[1]);
            Assert.AreSame(c, path[2]);
        }

        [Test]
        public void AllTilesBlockedExceptPath_ReturnsNull()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { null, null, null, b, null, null });

            Assert.IsNull(TilePathfinder.FindPath(a, c, tile => false));
        }

        [Test]
        public void PathHasCorrectStartAndEnd()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);
            TileData d = new TileData(3, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { d, null, null, b, null, null });
            d.SetNeighbours(new TileData[6] { null, null, null, c, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, d);
            Assert.IsNotNull(path);
            Assert.AreSame(a, path[0]);
            Assert.AreSame(d, path[^1]);
        }

        [Test]
        public void PathIsContinuous()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);
            TileData d = new TileData(3, 0);
            TileData e = new TileData(4, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { d, null, null, b, null, null });
            d.SetNeighbours(new TileData[6] { e, null, null, c, null, null });
            e.SetNeighbours(new TileData[6] { null, null, null, d, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, e);
            Assert.IsNotNull(path);
            Assert.IsTrue(IsContinuousPath(path));
        }

        [Test]
        public void BlockedTarget_ReturnsNull()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });

            Assert.IsNull(TilePathfinder.FindPath(a, b, t => t != b));
        }

        [Test]
        public void DefaultCanTraverse_AllowsNonNullTiles()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { null, null, null, a, null, null });

            Assert.IsNotNull(TilePathfinder.FindPath(a, b));
        }

        [Test]
        public void PathThroughMultipleHops_IsCorrect()
        {
            TileData a = new TileData(0, 0);
            TileData b = new TileData(1, 0);
            TileData c = new TileData(2, 0);
            TileData d = new TileData(3, 0);

            a.SetNeighbours(new TileData[6] { b, null, null, null, null, null });
            b.SetNeighbours(new TileData[6] { c, null, null, a, null, null });
            c.SetNeighbours(new TileData[6] { d, null, null, b, null, null });
            d.SetNeighbours(new TileData[6] { null, null, null, c, null, null });

            List<TileData> path = TilePathfinder.FindPath(a, d);
            Assert.IsNotNull(path);
            Assert.IsTrue(path.SequenceEqual(new[] { a, b, c, d }));
        }

        private static bool IsContinuousPath(List<TileData> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (!path[i].Neighbours.Contains(path[i + 1]))
                    return false;
            }
            return true;
        }
    }
}
