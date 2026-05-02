using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Enumerations;
using Game.Data;
using Input;
using Systems.Grid.AlterationPasses;
using UnityEngine;
using UserInterface;
using Zenject;

namespace Systems.Grid
{
    public class AxialHexGrid : MonoBehaviour
    {
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private InputHandler _inputHandler;
        [Inject] private GenerationProgressTracker _progressTracker;
        [Inject] private UiManager _uiManager;
        
        [Header("Grid Settings")]
        public float hexSize = 1.05f;
        public bool generateOnAwake = true;
        
        [Header("Async Spawning Settings")]
        [SerializeField] private float maxMsPerFrame = 5f; // Maximum milliseconds allowed per frame for grid logic

        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;
        
        [SerializeField] private List<GridGeneratorPassWrapper> augmentationPasses = new List<GridGeneratorPassWrapper>();

        // Core grid storage
        private Dictionary<Vector2Int, TileData> _tiles = new Dictionary<Vector2Int, TileData>();
        public Dictionary<Vector2Int, TileData> Tiles => _tiles;

        private int _radius = 5;

        // Events
        public event Action<Dictionary<Vector2Int, TileData>> OnGridGenerated;
        public event Action OnAugmentationPassesComplete;
        
        private InputLock _inputLock;
        
        private int _currentSeed;
        private List<IGridAlterationPass> _orderedPasses;

        private HashSet<Vector2Int> _neighborsBuiltFor = new HashSet<Vector2Int>();
        
        private void Start()
        {
            _inputLock = _inputHandler.RegisterInputLock(this);
            
            if (generateOnAwake)
            {
                StartCoroutine(GenerateGridRoutine());
            }
        }

        public void GenerateGrid()
        {
            StartCoroutine(GenerateGridRoutine());
        }

        private IEnumerator GenerateGridRoutine()
        {
            _inputLock.IsLocked = true;
            _radius = _playerSettings.gridRadius;
            
            _uiManager.ShowLoadingScrean();
            _progressTracker.Initialize(_radius, _playerSettings.populationSize);
            
            SetSeed();
            ClearGrid();

            // 1. Create ALL TileData objects first.
            // We generate the entire grid's underlying data first so that generator passes
            // have the full context they need to avoid "ruined" generations.
            yield return StartCoroutine(CreateDataForRangeRoutine(0, _radius));

            // 2. Build neighbors for the ENTIRE grid. 
            // This must happen before passes so passes can use neighbor-aware logic (clustering/smoothing).
            yield return StartCoroutine(BuildNeighboursRoutine(_radius));

            // 3. Run generator passes now that the full neighbor graph is linked.
            RunAugmentationPasses();

            Debug.Log($"Generated {_tiles.Count} hex tiles with radius {_radius}");
            
            OnGridGenerated?.Invoke(_tiles);
            
            _inputLock.IsLocked = false;
        }

        private IEnumerator CreateDataForRangeRoutine(int startRadius, int endRadius)
        {
            float budgetSeconds = maxMsPerFrame / 1000f;
            float startTime = Time.realtimeSinceStartup;

            int batchCount = 0;

            foreach (Vector2Int coord in GetCoordinatesInRingRange(startRadius, endRadius))
            {
                CreateTileData(coord.x, coord.y);
                batchCount++;
                
                if (Time.realtimeSinceStartup - startTime > budgetSeconds)
                {
                    _progressTracker.UpdateProgress(WorkUnitTypes.TileCreation, batchCount);
                    batchCount = 0;
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
            }
            
            if (batchCount > 0)
                _progressTracker.UpdateProgress(WorkUnitTypes.TileCreation, batchCount);
        }

        private IEnumerable<Vector2Int> GetCoordinatesInRingRange(int startRadius, int endRadius)
        {
            if (startRadius == 0)
            {
                yield return Vector2Int.zero;
                startRadius = 1;
            }

            for (int k = startRadius; k <= endRadius; k++)
            {
                // Start at the top-most hex of the current ring
                Vector2Int current = new Vector2Int(0, -k);
                Vector2Int[] directions = {
                    new Vector2Int(1, 0),   // Move East
                    new Vector2Int(0, 1),   // Move SouthEast
                    new Vector2Int(-1, 1),  // Move SouthWest
                    new Vector2Int(-1, 0),  // Move West
                    new Vector2Int(0, -1),  // Move NorthWest
                    new Vector2Int(1, -1)   // Move NorthEast
                };

                foreach (var dir in directions)
                {
                    for (int i = 0; i < k; i++)
                    {
                        yield return current;
                        current += dir;
                    }
                }
            }
        }
        
        private void CreateTileData(int q, int r)
        {
            TileData tileData = new TileData(q, r);
            _tiles[new Vector2Int(q, r)] = tileData;
        }
        
        private IEnumerator BuildNeighboursRoutine(int targetRadius)
        {
            float budgetSeconds = maxMsPerFrame / 1000f;
            float lastYieldTime = Time.realtimeSinceStartup;
            int batchCount = 0;

            // Cache the enum values so we don't use Reflection inside the loop
            Directions.Axial[] directions = (Directions.Axial[])Enum.GetValues(typeof(Directions.Axial));

            // Use the spiral generator instead of iterating the whole dictionary
            // This makes the initial subset building much faster for large grids
            foreach (Vector2Int axialCoord in GetCoordinatesInRingRange(0, targetRadius))
            {
                if (_neighborsBuiltFor.Contains(axialCoord)) continue;
                
                TileData data = GetTile(axialCoord);
                if (data == null) continue;

                var res = new Dictionary<Directions.Axial, TileData>();
                foreach (Directions.Axial direction in directions)
                {
                    Vector2Int neighborCoord = data.GetNeighborCoordinate(direction);
                    TileData neighbor = GetTile(neighborCoord.x, neighborCoord.y);
                    if (neighbor != null)
                        res[direction] = neighbor;
                }

                data.SetNeighbours(res);
                _neighborsBuiltFor.Add(axialCoord);
                batchCount++;
                
                // Check budget every 50 iterations (similar to your NPC batching) 
                // to reduce calls to Time.realtimeSinceStartup
                if (batchCount % 50 == 0 && Time.realtimeSinceStartup - lastYieldTime > budgetSeconds)
                {
                    _progressTracker.UpdateProgress(WorkUnitTypes.NeighborHookup, batchCount);
                    batchCount = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }
            
            if (batchCount > 0)
                _progressTracker.UpdateProgress(WorkUnitTypes.NeighborHookup, batchCount);
        }
        
        private void SetSeed()
        {
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;
        }
        
        private void RunAugmentationPasses()
        {
            if (augmentationPasses == null || augmentationPasses.Count == 0)
            {
                Debug.LogWarning("No augmentation passes configured!");
                return;
            }
            
            _orderedPasses = augmentationPasses
                .Where(w => w.pass != null)
                .Select(w => w.pass)
                .OrderBy(p => p.Priority)
                .ToList();
            
            foreach (var pass in _orderedPasses)
            {
                pass.Execute(this, _currentSeed);
            }
            
            OnAugmentationPassesComplete?.Invoke();
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            _tiles?.Clear();
            _tiles ??= new Dictionary<Vector2Int, TileData>();
            _neighborsBuiltFor?.Clear();
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

        public List<TileData> GetTilesInRadius(Vector2Int center, int radius)
        {
            List<TileData> results = new List<TileData>();

            foreach (Vector2Int relCoord in GetCoordinatesInRingRange(0, radius))
            {
                Vector2Int absoluteCoord = center + relCoord;
                TileData tile = GetTile(absoluteCoord);
                if (tile != null) results.Add(tile);
            }
            return results;
        }
        
        // Pass management
        public void AddGeneratorPass(IGridAlterationPass pass)
        {
            augmentationPasses.Add(new GridGeneratorPassWrapper { pass = pass });
            _orderedPasses = null;
        }
        
        public bool RemoveGeneratorPass(IGridAlterationPass pass)
        {
            var wrapper = augmentationPasses.Find(w => w.pass == pass);
            if (wrapper != null)
            {
                augmentationPasses.Remove(wrapper);
                _orderedPasses = null;
                return true;
            }
            return false;
        }
        
        public List<IGridAlterationPass> GetGeneratorPasses()
        {
            return augmentationPasses?.Where(w => w.pass != null).Select(w => w.pass).ToList() ?? new List<IGridAlterationPass>();
        }
        
        [System.Serializable]
        public class GridGeneratorPassWrapper
        {
            [SerializeReference]
            public IGridAlterationPass pass;
        }
    }
}