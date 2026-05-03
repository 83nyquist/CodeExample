using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using UnityEngine;

namespace Systems.Decoration
{
    public class TileDecorator : MonoBehaviour
    {
        [SerializeField] private AxialHexGrid axialHexGrid;
        [SerializeField] private TileData tileData;
        
        public TileData TileData => tileData;
        
        public void Initialize(AxialHexGrid grid, TileData data, Transform parent)
        {
            axialHexGrid = grid;
            tileData = data;
            name = $"TileDecorator: {data.X}_{data.Z}";
            
            transform.SetParent(parent);
            transform.position = this.axialHexGrid.AxialToWorld(tileData.X, tileData.Z);
            transform.rotation = Quaternion.Euler(tileData.Rotation);
            enabled = true;
        }
        
        public void Return(Transform parent)
        {
            transform.SetParent(parent);
            enabled = false;
        }
    }
}