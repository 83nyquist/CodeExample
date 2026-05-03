using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using UnityEngine;

namespace Systems.Decoration.Components
{
    public class TileDecorator : MonoBehaviour
    {
        [SerializeField] private AxialHexGrid axialHexGrid;
        [SerializeField] private TileData tileData;
        
        public GameObject SourcePrefab { get; private set; }
        public TileData TileData => tileData;
        
        public void Initialize(AxialHexGrid grid, TileData data, Transform parent, GameObject sourcePrefab)
        {
            axialHexGrid = grid;
            tileData = data;
            SourcePrefab = sourcePrefab;
            
            transform.SetParent(parent);

            if (data != null && grid != null)
            {
                name = $"TileDecorator: {data.X}_{data.Z}";
                transform.position = grid.AxialToWorld(data.X, data.Z);
                transform.rotation = Quaternion.Euler(data.Rotation);
            }
            else
            {
                name = "TileDecorator (Pooled)";
            }

            enabled = true;
        }
        
        public void Return(Transform parent)
        {
            transform.SetParent(parent);
            enabled = false;
        }
    }
}