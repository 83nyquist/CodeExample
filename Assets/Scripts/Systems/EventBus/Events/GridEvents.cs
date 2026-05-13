using System.Collections.Generic;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.EventBus.Events
{
    public class GridClearedEvent : GameEvent { }

    public class GridInitializationFinishedEvent : GameEvent
    {
        public IReadOnlyDictionary<Vector2Int, TileData> Tiles { get; }
        public int TotalTiles { get; }
        public float HexSize { get; }

        public GridInitializationFinishedEvent(IReadOnlyDictionary<Vector2Int, TileData> tiles, float hexSize)
            => (Tiles, TotalTiles, HexSize) = (tiles, tiles.Count, hexSize);
    }

    public class VisibleTilesCountChangedEvent : GameEvent
    {
        public int Count { get; }
        public VisibleTilesCountChangedEvent(int count) => Count = count;
    }
}
