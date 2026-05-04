using System.Collections.Generic;
using Data;
using Systems.Decoration.Interfaces;
using Systems.Decoration.Structs;
using Systems.Grid;
using Systems.Grid.Components;

namespace Systems.Decoration.Components
{
    public class DiscoveryVisionStrategy : IVisionStrategy
    {
        private readonly AxialHexGrid _grid;
        private readonly PlayerSettings _settings;

        public DiscoveryVisionStrategy(AxialHexGrid grid, PlayerSettings settings)
        {
            _grid = grid;
            _settings = settings;
        }

        public VisionContext CalculateVision(TileData origin)
        {
            var visionTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _settings.visionRadius);
            var visionSet = new HashSet<TileData>(visionTiles);
            var activeSet = new HashSet<TileData>(visionSet);

            foreach (var tile in _grid.Tiles.Values)
                if (tile.IsDiscovered) activeSet.Add(tile);

            return new VisionContext(visionSet, activeSet);
        }
    }
}