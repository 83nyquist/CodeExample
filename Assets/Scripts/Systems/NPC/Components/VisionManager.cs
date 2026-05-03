using Systems.Decoration;
using Unity.Collections;
using Unity.Mathematics;

namespace Systems.NPC.Components
{
    public class VisionManager
    {
        private readonly WorldDecorator _worldDecorator;
        private NativeHashSet<int2> _visibleTiles;
        private float _lastUpdateTime;
        private readonly float _updateInterval = 0.1f;
        
        public VisionManager(WorldDecorator worldDecorator, int initialCapacity)
        {
            _worldDecorator = worldDecorator;
            _visibleTiles = new NativeHashSet<int2>(initialCapacity, Allocator.Persistent);
            _lastUpdateTime = -_updateInterval;
        }
        
        public NativeHashSet<int2> GetVisibleTiles(float currentTime)
        {
            if (currentTime >= _lastUpdateTime + _updateInterval)
            {
                UpdateVisibleTiles();
                _lastUpdateTime = currentTime;
            }
            
            return _visibleTiles;
        }
        
        private void UpdateVisibleTiles()
        {
            _visibleTiles.Clear();
            
            if (_worldDecorator == null) return;
            
            var visibleTileData = _worldDecorator.GetVisibleTiles();
            if (visibleTileData == null) return;
            
            foreach (var tile in visibleTileData)
            {
                _visibleTiles.Add(new int2(tile.X, tile.Z));
            }
        }
        
        public void Dispose()
        {
            if (_visibleTiles.IsCreated)
                _visibleTiles.Dispose();
        }
    }
}