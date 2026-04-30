using System;
using System.Collections.Generic;
using Game;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Systems.Grid;
using Systems.Decoration;
using Zenject;
using Random = UnityEngine.Random;

namespace NPC
{
    public class NpcManager : MonoBehaviour
    {
        [Inject] private AxialHexGrid _hexGrid;
        [Inject] private WorldDecorator _worldDecorator;  // Inject WorldDecorator
        
        [Header("NPC Settings")]
        private int npcCount = 50;
        [SerializeField] private float minMoveInterval = 1f;
        [SerializeField] private float maxMoveInterval = 3f;
        [SerializeField] private GameObject npcVisualPrefab;
        [SerializeField] private float moveSpeed = 5f;
        
        private NativeHexGrid _nativeGrid;
        private NativeArray<NpcData> _npcs;
        private NativeArray<NpcData>.ReadOnly _npcsReadOnly;
        private GameObject[] _npcVisuals;
        private Animator[] _npcAnimators;
        
        private JobHandle _jobHandle;
        private bool _isJobRunning;
        
        // Cache for visible tiles each frame
        private NativeHashSet<int2> _visibleTiles;
        private float _lastVisionUpdate;
        private readonly float _visionUpdateInterval = 0.1f; // Update vision 5 times per second
        
        public int NpcCount => npcCount;
        public bool IsNpcsCreated => _npcs.IsCreated;
        
        private float _countUpdateTimer = 0;
        private float _countUpdateInterval = 0.2f;
        private int _cachedVisibleCount;
        public event Action<int> OnVisibleAgentsCountChanged;
        
        void Awake()
        {
            _hexGrid.OnGridGenerated += OnGridGenerated;
        }
        
        private void OnDestroy()
        {
            if (_hexGrid != null)
            {
                _hexGrid.OnGridGenerated -= OnGridGenerated;
            }
            
            CleanupNPCs();
            
            if (_visibleTiles.IsCreated)
            {
                _visibleTiles.Dispose();
            }
        }
        
        private void OnGridGenerated(Dictionary<Vector2Int, TileData> tiles)
        {
            Debug.Log($"NpcManager: Grid generated with {tiles.Count} tiles. Cleaning up and respawning NPCs...");
            
            CleanupNPCs();
            BuildNativeGrid();
            SpawnNpcs();
            
            // Initialize visible tiles set
            if (_visibleTiles.IsCreated)
                _visibleTiles.Dispose();
            _visibleTiles = new NativeHashSet<int2>(_hexGrid.Tiles.Count, Allocator.Persistent);
        }
        
        private void CleanupNPCs()
        {
            if (_isJobRunning)
            {
                _jobHandle.Complete();
                _isJobRunning = false;
            }
            
            if (_npcs.IsCreated)
                _npcs.Dispose();
            
            if (_nativeGrid.Tiles.IsCreated)
                _nativeGrid.Dispose();
            
            if (_npcVisuals != null)
            {
                foreach (GameObject npc in _npcVisuals)
                {
                    if (npc != null)
                        Destroy(npc);
                }
            }
            _npcVisuals = null;
            _npcAnimators = null; 
        }
        
        private void BuildNativeGrid()
        {
            _nativeGrid = new NativeHexGrid(_hexGrid.Tiles.Count, Allocator.Persistent);
            
            int index = 0;
            foreach (var kvp in _hexGrid.Tiles)
            {
                TileData tile = kvp.Value;
                
                BlittableTileData blittable = new BlittableTileData
                {
                    Coordinates = new int2(tile.X, tile.Z),
                    MovementCost = tile.IsWalkable ? (byte)1 : byte.MaxValue,
                    TerrainType = (byte)tile.type,
                    NeighborIndices = 0
                };
                
                _nativeGrid.Tiles[index] = blittable;
                _nativeGrid.PositionToIndex.Add(new int2(tile.X, tile.Z), index);
                index++;
            }
            
            Debug.Log($"Native grid built with {index} tiles");
        }
        
        private void SpawnNpcs()
        {
            npcCount = GameSettings.PopulationSize;
            _npcs = new NativeArray<NpcData>(npcCount, Allocator.Persistent);
            _npcVisuals = new GameObject[npcCount];
            _npcAnimators = new Animator[npcCount]; 
            
            List<int2> walkableTiles = new List<int2>();
            for (int i = 0; i < _nativeGrid.Tiles.Length; i++)
            {
                if (_nativeGrid.Tiles[i].IsWalkable)
                {
                    walkableTiles.Add(_nativeGrid.Tiles[i].Coordinates);
                }
            }
            
            if (walkableTiles.Count == 0)
            {
                Debug.LogError("No walkable tiles found for NPC spawning!");
                return;
            }
            
            for (int i = 0; i < npcCount; i++)
            {
                int2 startPos = walkableTiles[Random.Range(0, walkableTiles.Count)];
                
                _npcs[i] = new NpcData
                {
                    Position = startPos,
                    Timer = Random.Range(0f, maxMoveInterval),
                    Id = i,
                    IsVisible = false
                };
                
                Vector3 worldPos = HexToWorld(startPos);
                if (npcVisualPrefab != null)
                {
                    _npcVisuals[i] = Instantiate(npcVisualPrefab, worldPos, Quaternion.identity, transform);
                    _npcVisuals[i].name = $"NPC_{i}";
                    
                    _npcAnimators[i] = _npcVisuals[i].GetComponent<Animator>();
                }
            }
            
            Debug.Log($"Spawned {npcCount} NPCs on {walkableTiles.Count} walkable tiles");
        }
        
        private void UpdateVisibleTiles()
        {
            if (_worldDecorator == null) return;
            
            // Clear and rebuild visible tiles set from WorldDecorator
            _visibleTiles.Clear();
            
            // Get the HashSet<TileData> of visible/decorated tiles from WorldDecorator
            HashSet<TileData> visibleTileData = _worldDecorator.GetVisibleTiles(); // Or whatever the property/method is called
            
            if (visibleTileData == null) return;
            
            // Convert TileData to int2 coordinates for the job
            foreach (TileData tile in visibleTileData)
            {
                _visibleTiles.Add(new int2(tile.X, tile.Z));
            }
        }
        
        void Update()
        {
            if (!_npcs.IsCreated) return;
            
            if (Time.time >= _lastVisionUpdate)
            {
                _lastVisionUpdate = Time.time + _visionUpdateInterval;
                UpdateVisibleTiles();
            }
            
            HandleVisibleAgentsCountChanged();
            
            // Complete previous job first
            if (_isJobRunning && _jobHandle.IsCompleted)
            {
                _jobHandle.Complete();
                _isJobRunning = false;
            }
    
            // Update animator flags NOW (using completed job data)
            UpdateAnimatorFlags();
    
            // Schedule new job
            var job = new NpcJob
            {
                NPCs = _npcs,
                DeltaTime = Time.deltaTime,
                MinInterval = minMoveInterval,
                MaxInterval = maxMoveInterval,
                RandomSeed = (uint)Random.Range(1, 999999),
                Grid = _nativeGrid,
                VisibleTiles = _visibleTiles
            };
    
            _jobHandle = job.Schedule(_npcs.Length, 64);
            _isJobRunning = true;
            JobHandle.ScheduleBatchedJobs();
        }
        
        void LateUpdate()
        {
            if (_isJobRunning)
            {
                _jobHandle.Complete();
                _isJobRunning = false;
            }
            
            UpdateNPCVisuals();
        }
        [SerializeField] private float rotationSpeed = 15f; 
        private void UpdateNPCVisuals()
        {
            if (_isJobRunning) return;

            for (int i = 0; i < _npcVisuals.Length; i++)
            {
                if (_npcVisuals[i] != null && i < _npcs.Length)
                {
                    Vector3 targetPos = HexToWorld(_npcs[i].Position);
                    Vector3 currentPos = _npcVisuals[i].transform.position;
            
                    float distanceToTarget = Vector3.Distance(currentPos, targetPos);
            
                    // Use MoveTowards for precise stopping
                    if (distanceToTarget > 0.01f)
                    {
                        // This will move exactly and stop when reached
                        _npcVisuals[i].transform.position = Vector3.MoveTowards(
                            currentPos, 
                            targetPos, 
                            moveSpeed * Time.deltaTime
                        );
                
                        // Rotation while moving
                        Vector3 moveDirection = (targetPos - currentPos).normalized;
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                        _npcVisuals[i].transform.rotation = Quaternion.Slerp(
                            _npcVisuals[i].transform.rotation,
                            targetRotation,
                            rotationSpeed * Time.deltaTime  // Use dedicated rotation speed
                        );
                    }
                    else
                    {
                        // Snap to exact position when close enough
                        _npcVisuals[i].transform.position = targetPos;
                    }
            
                    // Visibility
                    bool shouldBeVisible = _npcs[i].IsVisible;
                    if (_npcVisuals[i].activeSelf != shouldBeVisible)
                    {
                        _npcVisuals[i].SetActive(shouldBeVisible);
                    }
                }
            }
        }
        
        private void UpdateAnimatorFlags()
        {
            for (int i = 0; i < _npcVisuals.Length; i++)
            {
                if (_npcVisuals[i] != null && i < _npcs.Length)
                {
                    Vector3 targetPos = HexToWorld(_npcs[i].Position);
                    Vector3 currentPos = _npcVisuals[i].transform.position;
            
                    // Now this will properly become false when MoveTowards finishes
                    bool isMoving = Vector3.Distance(currentPos, targetPos) > 0.01f;
            
                    Animator animator = _npcAnimators[i];
                    if (animator != null && animator.GetBool("IsMoving") != isMoving)
                    {
                        animator.SetBool("IsMoving", isMoving);
                    }
                }
            }
        }
        
        private Vector3 HexToWorld(int2 coord)
        {
            return _hexGrid.AxialToWorld(coord.x, coord.y);
        }
        
        private void HandleVisibleAgentsCountChanged()
        {
            _countUpdateTimer += Time.deltaTime;
    
            if (_countUpdateTimer >= _countUpdateInterval)
            {
                _countUpdateTimer = 0;
        
                // Check if job is done before reading
                if (_jobHandle.IsCompleted)
                {
                    _jobHandle.Complete();
                    int newCount = CalculateVisibleCount();
                    if (newCount != _cachedVisibleCount)
                    {
                        _cachedVisibleCount = newCount;
                        OnVisibleAgentsCountChanged?.Invoke(newCount);
                    }
                }
            }
        }
        
        private int CalculateVisibleCount()
        {
            int count = 0;
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i].IsVisible) count++;
            }
            return count;
        }
    }
}