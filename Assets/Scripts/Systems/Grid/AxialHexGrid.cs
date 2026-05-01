using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Enumerations;
using Game.Data;
using Systems.Grid.AlterationPasses;
using UnityEngine;
using Zenject;

namespace Systems.Grid
{
    public class AxialHexGrid : MonoBehaviour
    {
        [Inject] private PlayerSettings _playerSettings;
        
        [Header("Grid Settings")]
        public float hexSize = 1.05f;
        public bool generateOnAwake = true;
        
        [Header("Async Spawning Settings")]
        [SerializeField] private float maxMsPerFrame = 5f; // Maximum milliseconds allowed per frame for grid logic

        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;
        
        [SerializeField] private List<GridGeneratorPassWrapper> generatorPasses = new List<GridGeneratorPassWrapper>();

        // Core grid storage
        private Dictionary<Vector2Int, TileData> _tiles = new Dictionary<Vector2Int, TileData>();
        public Dictionary<Vector2Int, TileData> Tiles => _tiles;

        private int _radius = 5;

        // Events
        public event Action<Dictionary<Vector2Int, TileData>> OnGridGenerated; // Fires when the entire grid is fully generated and passes are run
        public event Action<TileData> OnInitialNeighborsPopulated; // Fires after initial subset of neighbors are built

        
        private int _currentSeed;
        private List<IGridAlterationPass> _orderedPasses;
        
        private void Start()
        {
            if (generateOnAwake)
            {
                StartCoroutine(GenerateGridRoutine());
            }
        }

        private HashSet<Vector2Int> _neighborsBuiltFor = new HashSet<Vector2Int>();

        [ContextMenu("Generate Grid")]
        public void GenerateGrid()
        {
            _radius = _playerSettings.gridRadius;
            SetSeed();
            ClearGrid();
            
            StartCoroutine(GenerateGridRoutine());
        }

        private IEnumerator GenerateGridRoutine()
        {
            _radius = _playerSettings.gridRadius;
            SetSeed();
            ClearGrid();

            // 1. Create ALL TileData objects first.
            // We generate the entire grid's underlying data first so that generator passes
            // have the full context they need to avoid "ruined" generations.
            yield return StartCoroutine(CreateDataForRangeRoutine(0, _radius));

            // 2. Run generator passes now on the full data set.
            // This ensures Elevation, Moisture, and TileTypes are correctly calculated for all tiles.
            RunGeneratorPasses();

            // 3. Determine the initial radius needed for starting the game visuals
            int initialRadius = Mathf.Max(50, 2 * _playerSettings.visionRadius);

            // 4. Build neighbors for the initial subset and fire the event.
            // Because Step 2 already ran, the WorldDecorator will see the correct TileTypes immediately.
            yield return StartCoroutine(BuildNeighboursRoutine(initialRadius, true));

            // 5. Build neighbors for the rest of the grid in the background.
            yield return StartCoroutine(BuildNeighboursRoutine(_radius, false));

            Debug.Log($"Generated {_tiles.Count} hex tiles with radius {_radius}");
            OnGridGenerated?.Invoke(_tiles);
        }

        private IEnumerator CreateDataForRangeRoutine(int startRadius, int endRadius)
        {
            float budgetSeconds = maxMsPerFrame / 1000f;
            float startTime = Time.realtimeSinceStartup;

            foreach (Vector2Int coord in GetCoordinatesInRingRange(startRadius, endRadius))
            {
                CreateTileData(coord.x, coord.y);
                
                if (Time.realtimeSinceStartup - startTime > budgetSeconds)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
            }
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
        
        private IEnumerator BuildNeighboursRoutine(int targetRadius, bool fireEvent)
        {
            float budgetSeconds = maxMsPerFrame / 1000f;
            float startTime = Time.realtimeSinceStartup;

            // Use the spiral generator instead of iterating the whole dictionary
            // This makes the initial subset building much faster for large grids
            foreach (Vector2Int axialCoord in GetCoordinatesInRingRange(0, targetRadius))
            {
                if (_neighborsBuiltFor.Contains(axialCoord)) continue;
                
                TileData data = GetTile(axialCoord);
                if (data == null) continue;

                var res = new Dictionary<Directions.Axial, TileData>();
                foreach (Directions.Axial direction in Enum.GetValues(typeof(Directions.Axial)))
                {
                    Vector2Int neighborCoord = data.GetNeighborCoordinate(direction);
                    TileData neighbor = GetTile(neighborCoord.x, neighborCoord.y);
                    if (neighbor != null)
                        res[direction] = neighbor;
                }

                data.SetNeighbours(res);
                _neighborsBuiltFor.Add(axialCoord);

                if (Time.realtimeSinceStartup - startTime > budgetSeconds)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
            }

            if (fireEvent)
            {
                // Find the center tile to pass to the event
                TileData centerTile = GetTile(Vector2Int.zero);
                OnInitialNeighborsPopulated?.Invoke(centerTile);
            }
        }
        
        private void SetSeed()
        {
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;
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
                pass.Execute(this, _currentSeed);
            }
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
            
            TileData centerTile = GetTile(center);
            if (centerTile != null) results.Add(centerTile);

            for (int k = 1; k <= radius; k++)
            {
                Vector2Int current = center + new Vector2Int(0, -k);
                Vector2Int[] directions = {
                    new Vector2Int(1, 0),   // East
                    new Vector2Int(0, 1),   // SouthEast
                    new Vector2Int(-1, 1),  // SouthWest
                    new Vector2Int(-1, 0),  // West
                    new Vector2Int(0, -1),  // NorthWest
                    new Vector2Int(1, -1)   // NorthEast
                };

                foreach (var dir in directions)
                {
                    for (int i = 0; i < k; i++)
                    {
                        TileData tile = GetTile(current);
                        if (tile != null) results.Add(tile);
                        current += dir;
                    }
                }
            }
            return results;
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