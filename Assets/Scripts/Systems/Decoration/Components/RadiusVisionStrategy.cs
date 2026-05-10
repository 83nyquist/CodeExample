using System.Collections.Generic;
using Data;
using Systems.Decoration.Interfaces;
using Systems.Decoration.Structs;
using Systems.Grid;
using Systems.Grid.Components;

namespace Systems.Decoration.Components
{
    public class RadiusVisionStrategy : IVisionStrategy
    {
        private readonly AxialHexGrid _grid;
        private readonly PlayerSettings _settings;
        private readonly int _radius;

        /// <summary>
        /// Initializes the radius vision strategy.
        /// </summary>
        public RadiusVisionStrategy(AxialHexGrid grid, PlayerSettings settings, int radius)
        {
            _grid = grid;
            _settings = settings;
            _radius = radius;
        }

        /// <summary>
        /// Calculates vision and active tile sets based on a fixed radius.
        /// </summary>
        /// <param name="origin">The center point for the calculation.</param>
        /// <returns>A VisionContext containing the vision and active tile sets.</returns>
        public VisionContext CalculateVision(TileData origin)
        {
            var visionTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _settings.VisionRadius);
            var activeTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _radius);
            
            return new VisionContext(new HashSet<TileData>(visionTiles), new HashSet<TileData>(activeTiles));
        }
    }
}