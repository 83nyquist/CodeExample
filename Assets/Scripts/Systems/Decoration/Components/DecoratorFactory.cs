using System.Collections.Generic;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using Zenject;

namespace Systems.Decoration.Components
{
    public class DecoratorFactory : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        
        [SerializeField] private TileSet tileSet;
        [SerializeField] private Transform poolParent;
        [SerializeField] private Transform activeParent;
        [SerializeField] private int preWarm = 10;
        
        private Dictionary<GameObject, Queue<TileDecorator>> _pools = new Dictionary<GameObject, Queue<TileDecorator>>();
        private Dictionary<TileData, TileDecorator> _activeTiles = new Dictionary<TileData, TileDecorator>();
        
        public TileSet TileSet => tileSet;
        
        private void Awake()
        {
            if (poolParent == null)
                poolParent = transform;
                
            if (activeParent == null)
                activeParent = transform;

            InitializePools();
        }
        
        private void InitializePools()
        {
            PreWarmPools(preWarm);
        }
        
        /// <summary>
        /// Get a TileDecorator for the given TileData (creates from pool or instantiates new)
        /// </summary>
        public TileDecorator GetTileDecorator(TileData tileData)
        {
            if (tileData == null) return null;
                
            GameObject prefab = null;
            
            // Choose prefab based on visibility state
            if (tileData.IsInVision)
                prefab = tileSet.GetTilePrefab(tileData.type, tileData.VariationIndex);
            else if (tileData.IsDiscovered)
                prefab = tileSet.GetShroudedPrefab(tileData.type);
            
            if (prefab == null) return null;
            
            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<TileDecorator>();

            TileDecorator decorator;
            if (_pools[prefab].Count > 0)
            {
                decorator = _pools[prefab].Dequeue();
                decorator.gameObject.SetActive(true);
            }
            else
            {
                decorator = CreateNewDecorator(prefab);
            }
            
            decorator.Initialize(_axialHexGrid, tileData, activeParent, prefab);
            
            _activeTiles[tileData] = decorator;
            return decorator;
        }
        
        /// <summary>
        /// Return a TileDecorator to the pool
        /// </summary>
        public void ReturnTileDecorator(TileDecorator decorator)
        {
            if (decorator == null) return;
                
            TileData tileData = decorator.TileData;
            GameObject source = decorator.SourcePrefab;

            if (tileData != null)
            {
                _activeTiles.Remove(tileData);
                // Clear reference to avoid stale data
                tileData.SetDecorator(null);
            }
            decorator.Return(poolParent);
            decorator.gameObject.SetActive(false);
            
            if (source != null)
            {
                if (!_pools.ContainsKey(source)) _pools[source] = new Queue<TileDecorator>();
                _pools[source].Enqueue(decorator);
            }
        }
        
        private TileDecorator CreateNewDecorator(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, activeParent);
            TileDecorator decorator = instance.GetComponent<TileDecorator>() ?? instance.AddComponent<TileDecorator>();
            return decorator;
        }
        
        /// <summary>
        /// Returns all active decorators to their respective pools.
        /// </summary>
        public void CleanupActiveDecorators()
        {
            // Create a copy to safely iterate while the original collection is modified by ReturnTileDecorator
            var active = new List<TileDecorator>(_activeTiles.Values);
            foreach (var decorator in active)
            {
                ReturnTileDecorator(decorator);
            }
        }

        /// <summary>
        /// Pre-warm pools by creating instances upfront
        /// </summary>
        public void PreWarmPools(int preWarmCount = 5)
        {
            foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
            {
                // Pre-warm variations
                int variations = tileSet.GetVariationCount(type);
                for (int v = 0; v < variations; v++)
                {
                    GameObject prefab = tileSet.GetTilePrefab(type, v);
                    WarmSpecificPrefab(prefab, preWarmCount);
                }

                // Pre-warm shrouded version
                GameObject shroud = tileSet.GetShroudedPrefab(type);
                if (shroud != null) WarmSpecificPrefab(shroud, preWarmCount);
            }
        }

        private void WarmSpecificPrefab(GameObject prefab, int count)
        {
            if (prefab == null) return;
            if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<TileDecorator>();

            for (int i = 0; i < count; i++)
            {
                TileDecorator decorator = CreateNewDecorator(prefab);
                decorator.Initialize(null, null, poolParent, prefab);
                decorator.gameObject.SetActive(false);
                _pools[prefab].Enqueue(decorator);
            }
        }
    }
}