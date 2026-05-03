using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Grid.Passes.Alteration
{
    [System.Serializable]
    public class RotationAlterationPass : BaseAlterationPass
    {
        [Header("RotationAlterationPass")]
        public override string PassName => "Rotation Pass";
    
        public override void Execute(AxialHexGrid grid, int seed)
        {
            System.Random random = new System.Random(seed);
        
            foreach (var kvp in grid.Tiles)
            {
                TileData tile = kvp.Value;
            
                // To retain "pointy side up" orientation, we rotate in 60-degree increments.
                // Since the grid uses X and Z coordinates, we rotate around the Y-axis.
                float yRotation = random.Next(0, 6) * 60f;
                tile.Rotation = new Vector3(0, yRotation, 0);
            }
        
            Debug.Log($"[RotationPass] Assigned rotations to {grid.Tiles.Count} tiles");
        }
    }
}