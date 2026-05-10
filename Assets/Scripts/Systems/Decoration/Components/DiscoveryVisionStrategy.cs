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

        /// <summary>
        /// Initializes the discovery-based vision strategy.
        /// </summary>
        public DiscoveryVisionStrategy(AxialHexGrid grid, PlayerSettings settings)
        {
            _grid = grid;
            _settings = settings;
        }

        /// <summary>
        /// Calculates the vision set based on radius and the active set based on all discovered tiles.
        /// </summary>
        /// <param name="origin">The current player position.</param>
        public VisionContext CalculateVision(TileData origin)
        {
            var visionTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _settings.VisionRadius);
            var visionSet = new HashSet<TileData>(visionTiles);
            var activeSet = new HashSet<TileData>(visionSet);

            foreach (var tile in _grid.Tiles.Values)
                if (tile.IsDiscovered) activeSet.Add(tile);

            return new VisionContext(visionSet, activeSet);
        }
    }
}