using System.Collections.Generic;
using Systems.Decoration.Components;
using Systems.Grid.Components;

namespace Systems.EventBus.Events
{
    public class PlayerMovedEvent : GameEvent
    {
        public TileData NewTile { get; }
        public PlayerMovedEvent(TileData tile) => NewTile = tile;
    }

    public class PlayerDestinationReachedEvent : GameEvent
    {
        public TileData Tile { get; }
        public PlayerDestinationReachedEvent(TileData tile) => Tile = tile;
    }

    public class DrawPathRequest : GameEvent
    {
        public TileDecorator Target { get; }
        public DrawPathRequest(TileDecorator target) => Target = target;
    }

    public class PathCreatedEvent : GameEvent
    {
        public List<TileData> Path { get; }
        public PathCreatedEvent(List<TileData> path) => Path = path;
    }

    public class PathClearedEvent : GameEvent { }
}
