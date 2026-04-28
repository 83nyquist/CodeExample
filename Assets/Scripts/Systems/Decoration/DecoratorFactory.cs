using Systems.Grid;
using System.Collections.Generic;
using Core.Components;
using UnityEngine;
using Zenject;

namespace Systems.Decoration
{
    public class DecoratorFactory : MonoBehaviour
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        
        [SerializeField] private TileSet tileSet;
        [SerializeField] private Transform poolParent;
        [SerializeField] private Transform activeParent;
        
        [SerializeField] private bool enableTileVariations = true;
        [SerializeField] private int preWarm = 10;
        
        private int _currentSeed;
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
            if (tileData == null)
                return null;
                
            TileType type = tileData.type;
            Vector2Int coordinates = tileData.AxialCoordinates;
            
            // Get prefab to check if it exists
            GameObject prefab = GetTilePrefabForType(type, coordinates);
            if (prefab == null)
                return null;
            
            TileDecorator decorator = null;
            
            // Try to get from pool
            if (_pools[type].Count > 0)
            {
                decorator = _pools[type].Dequeue();
                decorator.gameObject.SetActive(true);
            }
            else
            {
                // Create new instance
                GameObject instance = Instantiate(prefab, activeParent);
                decorator = instance.GetComponent<TileDecorator>();
                if (decorator == null)
                    decorator = instance.AddComponent<TileDecorator>();
            }
            
            // Set up the decorator
            decorator.Initialize(_axialHexGrid, tileData, activeParent);
            
            // Apply visual variation if needed
            ApplyTileVariation(decorator, type, coordinates);
            
            // Store in active dictionary
            _activeTiles[tileData] = decorator;
            
            return decorator;
        }
        
        /// <summary>
        /// Return a TileDecorator to the pool
        /// </summary>
        public void ReturnTileDecorator(TileDecorator decorator)
        {
            if (decorator == null)
                return;
                
            TileData tileData = decorator.TileData;
            if (tileData != null && _activeTiles.ContainsKey(tileData))
            {
                _activeTiles.Remove(tileData);
            }
            
            // Reset decorator state
            decorator.Return(poolParent);
            
            // Move to pool parent and disable
            decorator.transform.SetParent(poolParent);
            decorator.gameObject.SetActive(false);
            
            // Return to appropriate pool
            TileType type = decorator.TileData?.type ?? TileType.Ground;
            if (!_pools.ContainsKey(type))
                _pools[type] = new Queue<TileDecorator>();
                
            _pools[type].Enqueue(decorator);
        }
        
        [ContextMenu("Clear Active")]
        public void ClearActive()
        {
            // activeParent.Clear();
            activeParent.GetComponent<DestroyChildren>().Activate();
        }
        
        [ContextMenu("Clear Pool")]
        public void ClearPool()
        {
            // activeParent.Clear();
            poolParent.GetComponent<DestroyChildren>().Activate();
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
                    GameObject instance = Instantiate(prefab, poolParent);
                    instance.SetActive(false);
                    
                    TileDecorator decorator = instance.GetComponent<TileDecorator>();
                    if (decorator == null)
                        decorator = instance.AddComponent<TileDecorator>();
                        
                    _pools[type].Enqueue(decorator);
                }
            }
        }
        
        private GameObject GetTilePrefabForType(TileType type, Vector2Int coordinates)
        {
            if (TileSet == null)
                return null;
                
            if (enableTileVariations)
            {
                int variationCount = TileSet.GetVariationCount(type);
                
                if (variationCount > 0)
                {
                    int variationSeed = GetVariationSeed(coordinates);
                    int variationIndex = Mathf.Abs(variationSeed % variationCount);
                    return TileSet.GetTilePrefab(type, variationIndex);
                }
            }
            
            return TileSet.GetTilePrefab(type, -1);
        }
        
        private int GetVariationSeed(Vector2Int coordinates)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + coordinates.x;
                hash = hash * 31 + coordinates.y;
                hash = hash * 31 + _currentSeed;
                return hash;
            }
        }
        
        private void ApplyTileVariation(TileDecorator decorator, TileType type, Vector2Int coordinates)
        {
            // Apply any visual variations (color tint, scale, etc.)
            int seed = GetVariationSeed(coordinates);
            System.Random random = new System.Random(seed);
            
            // Example variations - customize as needed
            if (decorator.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                // Slight color variation for natural tiles
                if (type != TileType.Water)
                {
                    float variation = (float)random.NextDouble() * 0.1f - 0.05f;
                    spriteRenderer.color += new Color(variation, variation, variation, 0);
                }
            }
        }
    }
}