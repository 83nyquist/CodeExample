using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enumerations;
using Systems.Grid.AlterationPasses;
using UnityEngine;

namespace Systems.Grid
{
    public class AxialHexGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Range(1, 200)] public int radius = 5;
        public float hexSize = 1.05f;
        public bool generateOnAwake = true;
        
        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;
        
        [SerializeField] private List<GridGeneratorPassWrapper> generatorPasses = new List<GridGeneratorPassWrapper>();
        
        // Core grid storage
        private Dictionary<Vector2Int, TileData> _tiles = new Dictionary<Vector2Int, TileData>();
        public Dictionary<Vector2Int, TileData> Tiles => _tiles;
        
        // Events
        public event Action<Dictionary<Vector2Int, TileData>> OnGridGenerated;
        
        private int _currentSeed;
        private List<IGridAlterationPass> _orderedPasses;
        
        private void Start()
        {
            if (generateOnAwake)
            {
                GenerateGrid();
            }
        }
        
        [ContextMenu("Generate Grid")]
        public void GenerateGrid()
        {
            SetSeed();
            ClearGrid();
            
            // Generate all hexes within radius
            foreach (Vector2Int axialCoord in GetCoordinatesInRadius())
            {
                CreateTileData(axialCoord.x, axialCoord.y);
            }
            
            BuildAllNeighbours();
            RunGeneratorPasses();
            
            Debug.Log($"Generated {_tiles.Count} hex tiles with radius {radius}");
            OnGridGenerated?.Invoke(_tiles);
        }
        
        private IEnumerable<Vector2Int> GetCoordinatesInRadius()
        {
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                
                for (int r = r1; r <= r2; r++)
                {
                    yield return new Vector2Int(q, r);
                }
            }
        }
        
        private void CreateTileData(int q, int r)
        {
            TileData tileData = new TileData(q, r);
            _tiles[new Vector2Int(q, r)] = tileData;
        }
        
        private void BuildAllNeighbours()
        {
            foreach (TileData data in _tiles.Values)
            {
                Dictionary<Directions.Axial, TileData> res = new Dictionary<Directions.Axial, TileData>();
                
                foreach (Directions.Axial direction in Enum.GetValues(typeof(Directions.Axial)))
                {
                    Vector2Int neighborCoord = data.GetNeighborCoordinate(direction);
                    TileData neighbor = GetTile(neighborCoord.x, neighborCoord.y);
                
                    if (neighbor != null)
                    {
                        res[direction] = neighbor;
                    }
                }
                
                data.SetNeighbours(res);
            }
        }
        
        private void SetSeed()
        {
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;
            Debug.Log($"[AxialHexGrid] Using seed: {_currentSeed}");
        }
        
        private void RunGeneratorPasses()
        {
            if (generatorPasses == null || generatorPasses.Count == 0)
            {
                Debug.LogWarning("No generator passes configured!");
                return;
            }
            
            _orderedPasses = generatorPasses
                .Where(w => w.pass != null)
                .Select(w => w.pass)
                .OrderBy(p => p.Priority)
                .ToList();
            
            foreach (var pass in _orderedPasses)
            {
                Debug.Log($"Running generation pass: {pass.PassName} (Priority: {pass.Priority})");
                pass.Execute(this, _currentSeed);
            }
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            _tiles?.Clear();
            _tiles ??= new Dictionary<Vector2Int, TileData>();
        }
        
        // Core query methods (kept because they're fundamental to grid operation)
        public TileData GetTile(int q, int r)
        {
            _tiles.TryGetValue(new Vector2Int(q, r), out TileData tile);
            return tile;
        }
        
        public TileData GetTile(Vector2Int axialCoord)
        {
            _tiles.TryGetValue(axialCoord, out TileData tile);
            return tile;
        }
        
        // Pass management
        public void AddGeneratorPass(IGridAlterationPass pass)
        {
            generatorPasses.Add(new GridGeneratorPassWrapper { pass = pass });
            _orderedPasses = null;
        }
        
        public bool RemoveGeneratorPass(IGridAlterationPass pass)
        {
            var wrapper = generatorPasses.Find(w => w.pass == pass);
            if (wrapper != null)
            {
                generatorPasses.Remove(wrapper);
                _orderedPasses = null;
                return true;
            }
            return false;
        }
        
        public List<IGridAlterationPass> GetGeneratorPasses()
        {
            return generatorPasses?.Where(w => w.pass != null).Select(w => w.pass).ToList() ?? new List<IGridAlterationPass>();
        }
        
        [System.Serializable]
        public class GridGeneratorPassWrapper
        {
            [SerializeReference]
            public IGridAlterationPass pass;
        }
    }
}