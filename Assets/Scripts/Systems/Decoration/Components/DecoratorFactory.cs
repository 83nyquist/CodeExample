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
                
            TileType type = tileData.type;

            // The TileData now explicitly tells us which index to use. 
            // If it's -1, TileSet.GetTilePrefab handles the fallback to index 0.
            GameObject prefab = tileSet.GetTilePrefab(type, tileData.VariationIndex);
            
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

            if (tileData != null) _activeTiles.Remove(tileData);
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