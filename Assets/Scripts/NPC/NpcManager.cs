using System.Collections.Generic;
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
        [SerializeField] private int npcCount = 50;
        [SerializeField] private float minMoveInterval = 1f;
        [SerializeField] private float maxMoveInterval = 3f;
        [SerializeField] private GameObject npcVisualPrefab;
        [SerializeField] private float moveSpeed = 5f;
        
        private NativeHexGrid _nativeGrid;
        private NativeArray<NpcData> _npcs;
        private GameObject[] _npcVisuals;
        private Animator[] _npcAnimators;
        
        private JobHandle _jobHandle;
        private bool _isJobRunning;
        
        // Cache for visible tiles each frame
        private NativeHashSet<int2> _visibleTiles;
        private float _lastVisionUpdate;
        private readonly float _visionUpdateInterval = 0.1f; // Update vision 5 times per second
        
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
            SpawnNPCs();
            
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
        
        private void SpawnNPCs()
        {
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
        
        private void UpdateNPCVisuals()
        {
            if (_isJobRunning) return;
    
            for (int i = 0; i < _npcVisuals.Length; i++)
            {
                if (_npcVisuals[i] != null && i < _npcs.Length)
                {
                    Vector3 targetPos = HexToWorld(_npcs[i].Position);
                    Vector3 currentPos = _npcVisuals[i].transform.position;
            
                    // Only position and rotation - no animator calls here
                    if (Vector3.Distance(currentPos, targetPos) > 0.01f)
                    {
                        // Movement
                        _npcVisuals[i].transform.position = Vector3.Lerp(
                            currentPos, 
                            targetPos, 
                            moveSpeed * Time.deltaTime
                        );
                
                        // Rotation
                        Vector3 moveDirection = (targetPos - currentPos).normalized;
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                        _npcVisuals[i].transform.rotation = Quaternion.Slerp(
                            _npcVisuals[i].transform.rotation,
                            targetRotation,
                            moveSpeed * Time.deltaTime
                        );
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
            
                    bool isMoving = Vector3.Distance(currentPos, targetPos) > 0.01f;
            
                    // Use cached animator - no GetComponent call!
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
        
        void OnGUI()
        {
            if (!_npcs.IsCreated) 
            {
                GUILayout.Label("NPC System: Waiting for grid...");
                return;
            }
            
            GUILayout.BeginArea(new Rect(Screen.width - 300, 15, 290, 150));
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.Label($"NPCs: {npcCount}");
            GUILayout.Label($"Move Interval: {minMoveInterval}-{maxMoveInterval}s");
            GUILayout.Label($"Walkable Tiles: {GetWalkableTileCount()}");
            
            int visibleCount = 0;
            for (int i = 0; i < _npcs.Length; i++)
            {
                if (_npcs[i].IsVisible) visibleCount++;
            }
            GUILayout.Label($"Visible NPCs: {visibleCount}/{npcCount}");
            
            GUILayout.Label($"Multithreading: ACTIVE ✓");
            GUILayout.Label($"FPS: {1f/Time.deltaTime:F0}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private int GetWalkableTileCount()
        {
            if (!_nativeGrid.Tiles.IsCreated) return 0;
            
            int count = 0;
            for (int i = 0; i < _nativeGrid.Tiles.Length; i++)
            {
                if (_nativeGrid.Tiles[i].IsWalkable) count++;
            }
            return count;
        }
    }
}