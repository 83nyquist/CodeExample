using Systems.Grid.Components;

namespace Systems.Grid.Pathfinding
{
    public class PathRecord
    {
        /// <summary> The tile associated with this path record. </summary>
        public TileData Tile;
        /// <summary> The preceding tile in the calculated path. </summary>
        public TileData Parent;
        /// <summary> The accumulated cost from the start to this tile. </summary>
        public float CostG;
        /// <summary> The estimated cost from this tile to the destination. </summary>
        public float CostH;
        /// <summary> The total estimated cost (G + H). </summary>
        public float CostF => CostG + CostH;
    }
}
