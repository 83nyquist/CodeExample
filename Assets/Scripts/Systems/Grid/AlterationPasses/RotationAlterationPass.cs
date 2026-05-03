using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Grid.AlterationPasses
{
    [System.Serializable]
    public class RotationAlterationPass : BaseAlterationPass
    {
        public override string PassName => "Rotation Pass";
    
        public override void Execute(AxialHexGrid grid, int seed)
        {
            System.Random random = new System.Random(seed);
            float[] validRotations = { 0f, 60f, 120f, 180f, 240f, 300f };
        
            foreach (var kvp in grid.Tiles)
            {
                TileData tile = kvp.Value;
            
                // Store final Vector3 rotation directly
                float zRotation = validRotations[random.Next(0, 6)];
                tile.Rotation = new Vector3(0, 0, zRotation);
            }
        
            Debug.Log($"[RotationPass] Assigned rotations to {grid.Tiles.Count} tiles");
        }
    }
}