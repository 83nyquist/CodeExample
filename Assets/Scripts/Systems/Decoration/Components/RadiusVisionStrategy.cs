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

        public RadiusVisionStrategy(AxialHexGrid grid, PlayerSettings settings, int radius)
        {
            _grid = grid;
            _settings = settings;
            _radius = radius;
        }

        public VisionContext CalculateVision(TileData origin)
        {
            var visionTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _settings.visionRadius);
            var activeTiles = _grid.GetTilesInRadius(origin.AxialCoordinates, _radius);
            
            return new VisionContext(new HashSet<TileData>(visionTiles), new HashSet<TileData>(activeTiles));
        }
    }
}