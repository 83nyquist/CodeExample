using System.Collections.Generic;
using Character;
using Coordinators;
using Core.Components;
using Systems.EventBus;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Extensions;
using UnityEngine;
using Zenject;

namespace Vanguard
{
    public class VanguardController : EventBusSubscriber
    {
        [Inject] private AxialHexGrid _axialHexGrid;
        private VanguardMover _vanguardMover;

        [SerializeField] private CharacterItem selectedLeader;

        private DestroyChildren _destroyChildren;
        private TileData _currentTile;
        private IReadOnlyDictionary<Vector2Int, TileData> _gridTiles;
        private List<TileData> _latestPath;
        private float _hexSize;
        private bool _isResetting;

        public TileData CurrentTile => _currentTile;

        private void Awake()
        {
            _vanguardMover = GetComponent<VanguardMover>();
            _destroyChildren = GetComponent<DestroyChildren>();
            DeSpawn();

            Subscribe<WorldGenerationFinishedEvent>(OnGenerationFinished);
            Subscribe<GridInitializationFinishedEvent>(OnGridInitialized);
            Subscribe<PlayerDestinationReachedEvent>(OnDestinationReached);
            Subscribe<RespawnRequest>(OnRespawnRequest);
            Subscribe<CommanderSelectedRequest>(OnCharacterSelected);
            Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            Subscribe<WorldCleanupEvent>(OnWorldCleanup);
            Subscribe<PlayerMoveRequest>(OnMoveRequest);
            Subscribe<PathCreatedEvent>(OnPathCreated);
            Subscribe<PathClearedEvent>(OnPathCleared);
        }

        private void OnWorldCleanup(WorldCleanupEvent e)
        {
            Stop();
            DeSpawn();
        }

        private void OnGridInitialized(GridInitializationFinishedEvent e)
        {
            _gridTiles = e.Tiles;
            _hexSize = e.HexSize;
        }

        private void OnGenerationFinished(WorldGenerationFinishedEvent e)
        {
            Stop();
            if (_gridTiles != null && _gridTiles.TryGetValue(Vector2Int.zero, out var origin))
            {
                ReturnToOrigin(origin);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (e.State == GameState.Playing) Spawn();
            else if (e.State == GameState.Loading)
            {
                Stop();
                DeSpawn();
            }
        }

        private void OnCharacterSelected(CommanderSelectedRequest e) => selectedLeader = e.Character;
        private void OnRespawnRequest(RespawnRequest e) => Respawn();
        private void OnDestinationReached(PlayerDestinationReachedEvent e) => _currentTile = e.Tile;
        private void OnPathCreated(PathCreatedEvent e) => _latestPath = e.Path;
        private void OnPathCleared(PathClearedEvent e) => _latestPath = null;

        private void OnMoveRequest(PlayerMoveRequest e)
        {
            if (_latestPath != null) _vanguardMover.TraversePath(_latestPath);
        }

        public void Spawn()
        {
            if (selectedLeader == null) return;
            
            // Defensive check: Ensure we don't double-spawn if DeSpawn hasn't finished yet
            foreach (Transform child in transform) { Destroy(child.gameObject); }

            GameObject go = Instantiate(selectedLeader.gamePrefab, transform);
            _vanguardMover.Animator = go.GetComponent<Animator>();
            Publish(new CharacterAnimationEventsChangedEvent(go.GetComponent<CharacterAnimationEvents>()));
        }

        public void DeSpawn()
        {
            _vanguardMover.Animator = null;
            Publish(new CharacterAnimationEventsChangedEvent(null));
            _destroyChildren.Activate();
        }

        public void Respawn()
        {
            Stop();
            if (_gridTiles != null && _gridTiles.TryGetValue(Vector2Int.zero, out var origin))
                ReturnToOrigin(origin);
        }

        public void Stop() => _vanguardMover.StopMoving();

        private void ReturnToOrigin(TileData origin)
        {
            _currentTile = origin;
            Vector3 pos = _axialHexGrid.AxialToWorld(origin.X, origin.Z);
            pos.y = origin.Elevation;
            transform.position = pos;
            Publish(new PlayerMovedEvent(_currentTile));
        }
    }
}