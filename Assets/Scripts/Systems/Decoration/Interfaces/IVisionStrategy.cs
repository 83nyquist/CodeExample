using Systems.Decoration.Structs;
using Systems.Grid.Components;

namespace Systems.Decoration.Interfaces
{
    public interface IVisionStrategy
    {
        VisionContext CalculateVision(TileData origin);
    }
}