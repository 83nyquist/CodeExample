using System.Collections.Generic;
using Data;
using Systems.Grid;
using Systems.Grid.Components;
using UnityEngine;
using Zenject;

namespace Systems.Decoration.Components
{
    public class DecoratorFactory : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        [Inject] private TileDecoratorAnimator _animator;
        
        [SerializeField] private TileSet tileSet;
        [SerializeField] private Transform poolParent;
        [SerializeField] private Transform activeParent;
        [SerializeField] private int preWarm = 10;
        
        private Dictionary<GameObject, Queue<TileDecorator>> _pools = new Dictionary<GameObject, Queue<TileDecorator>>();
        private Dictionary<TileData, TileDecorator> _activeTiles = new Dictionary<TileData, TileDecorator>();
        
        public TileSet TileSet => tileSet;
        
        /// <summary>
        /// Sets up default parents and initializes the object pools.
        /// </summary>
        private void Awake()
        {
            if (poolParent == null)
                poolParent = transform;
                
            if (activeParent == null)
                activeParent = transform;

            InitializePools();
        }
        
        /// <summary>
        /// Triggers the initial pre-warming of the pools.
        /// </summary>
        private void InitializePools()
        {
            PreWarmPools(preWarm);
        }
        
        /// <summary>
        /// Retrieves a TileDecorator for the given TileData, either from a pool or by instantiating a new one.
        /// </summary>
        public TileDecorator GetTileDecorator(TileData tileData)
        {
            if (tileData == null) return null;
                
            GameObject prefab = null;
            
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
            
            decorator.Initialize(_axialHexGrid, tileData, activeParent, prefab, _animator);
            
            _activeTiles[tileData] = decorator;
            return decorator;
        }
        
        /// <summary>
        /// Returns a TileDecorator to its respective pool and clears its association with tile data.
        /// </summary>
        public void ReturnTileDecorator(TileDecorator decorator)
        {
            if (decorator == null) return;
                
            TileData tileData = decorator.TileData;
            GameObject source = decorator.SourcePrefab;

            if (tileData != null)
            {
                _activeTiles.Remove(tileData);
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
        
        /// <summary>
        /// Instantiates a new decorator instance and ensures it has the required component.
        /// </summary>
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
            var active = new List<TileDecorator>(_activeTiles.Values);
            foreach (var decorator in active)
            {
                ReturnTileDecorator(decorator);
            }
        }

        /// <summary>
        /// Populates pools with a set number of instances for all tile types and variations.
        /// </summary>
        public void PreWarmPools(int preWarmCount = 5)
        {
            foreach (Enumerations.TileType type in System.Enum.GetValues(typeof(Enumerations.TileType)))
            {
                int variations = tileSet.GetVariationCount(type);
                for (int v = 0; v < variations; v++)
                {
                    GameObject prefab = tileSet.GetTilePrefab(type, v);
                    WarmSpecificPrefab(prefab, preWarmCount);
                }

                GameObject shroud = tileSet.GetShroudedPrefab(type);
                if (shroud != null) WarmSpecificPrefab(shroud, preWarmCount);
            }
        }

        /// <summary>
        /// Instantiates and pools a specific number of instances for a given prefab.
        /// </summary>
        private void WarmSpecificPrefab(GameObject prefab, int count)
        {
            if (prefab == null) return;
            if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<TileDecorator>();

            for (int i = 0; i < count; i++)
            {
                TileDecorator decorator = CreateNewDecorator(prefab);
                decorator.Initialize(null, null, poolParent, prefab, null);
                decorator.gameObject.SetActive(false);
                _pools[prefab].Enqueue(decorator);
            }
        }
    }
}