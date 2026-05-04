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

        private TileDecoratorAnimator _animator;
        
        public void Initialize(AxialHexGrid grid, TileData data, Transform parent, GameObject sourcePrefab, TileDecoratorAnimator animator)
        {
            axialHexGrid = grid;
            tileData = data;
            SourcePrefab = sourcePrefab;
            _animator = animator;
            
            transform.SetParent(parent);

            if (data != null && grid != null)
            {
                name = $"TileDecorator: {data.X}_{data.Z}";
                Vector3 targetPos = grid.AxialToWorld(data.X, data.Z);
                
                if (_animator != null) _animator.Register(transform, targetPos);
                else transform.position = targetPos;

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
            if (_animator != null) _animator.Cancel(transform);
            transform.SetParent(parent);
            enabled = false;
        }
    }
}