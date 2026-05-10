using Systems.Decoration;
using Unity.Collections;
using Unity.Mathematics;

namespace Systems.NonPlayerCharacters.Components
{
    public class VisionManager
    {
        private readonly WorldDecorator _worldDecorator;
        private NativeHashSet<int2> _visibleTiles;
        private float _lastUpdateTime;
        private readonly float _updateInterval = 0.1f;

        /// <summary>
        /// Initializes a new instance of the VisionManager, allocating native memory for tile tracking.
        /// </summary>
        /// <param name="worldDecorator">The source of world visibility data.</param>
        /// <param name="initialCapacity">The expected number of visible tiles to pre-allocate.</param>
        public VisionManager(WorldDecorator worldDecorator, int initialCapacity)
        {
            _worldDecorator = worldDecorator;
            _visibleTiles = new NativeHashSet<int2>(initialCapacity, Allocator.Persistent);
            _lastUpdateTime = -_updateInterval;
        }

        /// <summary>
        /// Retrieves the set of visible tiles. Updates the collection from the decorator if the 
        /// refresh interval has been reached.
        /// </summary>
        /// <param name="currentTime">The current game time used for interval checking.</param>
        /// <returns>A NativeHashSet containing the coordinates of all visible tiles.</returns>
        public NativeHashSet<int2> GetVisibleTiles(float currentTime)
        {
            if (currentTime >= _lastUpdateTime + _updateInterval)
            {
                UpdateVisibleTiles();
                _lastUpdateTime = currentTime;
            }
            
            return _visibleTiles;
        }

        /// <summary>
        /// Synchronizes the native collection with the current state of the world decorator.
        /// </summary>
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

        /// <summary>
        /// Releases the persistent native memory allocated for the visibility set.
        /// </summary>
        public void Dispose()
        {
            if (_visibleTiles.IsCreated)
                _visibleTiles.Dispose();
        }
    }
}