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
        
        private Dictionary<TileType, Queue<TileDecorator>> _pools = new Dictionary<TileType, Queue<TileDecorator>>();
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
            foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
            {
                _pools[type] = new Queue<TileDecorator>();
            }
            
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
            
            TileDecorator decorator;
            if (_pools[type].Count > 0)
            {
                decorator = _pools[type].Dequeue();
                decorator.gameObject.SetActive(true);
            }
            else
            {
                decorator = CreateNewDecorator(prefab);
            }
            
            decorator.Initialize(_axialHexGrid, tileData, activeParent);
            
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
            if (tileData != null) _activeTiles.Remove(tileData);
            
            decorator.Return(poolParent);
            
            decorator.transform.SetParent(poolParent);
            decorator.gameObject.SetActive(false);
            
            TileType type = decorator.TileData?.type ?? TileType.PrimaryGround;
            _pools[type].Enqueue(decorator);
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
                GameObject prefab = TileSet.GetTilePrefab(type, -1);
                if (prefab == null) continue;
                
                for (int i = 0; i < preWarmCount; i++)
                {
                    TileDecorator decorator = CreateNewDecorator(prefab);
                    decorator.gameObject.SetActive(false);
                    decorator.transform.SetParent(poolParent);
                    _pools[type].Enqueue(decorator);
                }
            }
        }
    }
}