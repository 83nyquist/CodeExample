using Systems.Grid.Components;

namespace Systems.Grid.Pathfinding
{
    public class PathRecord
    {
        public TileData Tile;
        public TileData Parent;
        public float CostG;
        public float CostH;
        public float CostF => CostG + CostH;
    }
}
