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

        /// <summary>
        /// Initializes component references and subscribes to game events.
        /// </summary>
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

        /// <summary>
        /// Cleans up the player state when a world cleanup is requested.
        /// </summary>
        private void OnWorldCleanup(WorldCleanupEvent e)
        {
            Stop();
            DeSpawn();
        }

        /// <summary>
        /// Caches grid references when the grid is initialized.
        /// </summary>
        private void OnGridInitialized(GridInitializationFinishedEvent e)
        {
            _gridTiles = e.Tiles;
            _hexSize = e.HexSize;
        }

        /// <summary>
        /// Positions the player at the origin when world generation is finished.
        /// </summary>
        private void OnGenerationFinished(WorldGenerationFinishedEvent e)
        {
            Stop();
            if (_gridTiles != null && _gridTiles.TryGetValue(Vector2Int.zero, out var origin))
            {
                ReturnToOrigin(origin);
            }
        }

        /// <summary>
        /// Synchronizes spawning and movement logic with game state changes.
        /// </summary>
        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (e.State == GameState.Playing) Spawn();
            else if (e.State == GameState.Loading)
            {
                Stop();
                DeSpawn();
            }
        }

        /// <summary> Sets the leader character item. </summary>
        private void OnCharacterSelected(CommanderSelectedRequest e) => selectedLeader = e.Character;
        
        /// <summary> Triggers the respawn logic. </summary>
        private void OnRespawnRequest(RespawnRequest e) => Respawn();
        
        /// <summary> Updates the current tile tracking when destination is reached. </summary>
        private void OnDestinationReached(PlayerDestinationReachedEvent e) => _currentTile = e.Tile;
        
        /// <summary> Caches the latest calculated path. </summary>
        private void OnPathCreated(PathCreatedEvent e) => _latestPath = e.Path;
        
        /// <summary> Clears the cached path. </summary>
        private void OnPathCleared(PathClearedEvent e) => _latestPath = null;

        /// <summary>
        /// Initiates movement if a valid path exists.
        /// </summary>
        private void OnMoveRequest(PlayerMoveRequest e)
        {
            if (_latestPath != null) _vanguardMover.TraversePath(_latestPath);
        }

        /// <summary>
        /// Instantiates the selected character prefab and configures animation events.
        /// </summary>
        public void Spawn()
        {
            if (selectedLeader == null) return;
            
            foreach (Transform child in transform) { Destroy(child.gameObject); }

            GameObject go = Instantiate(selectedLeader.gamePrefab, transform);
            _vanguardMover.Animator = go.GetComponent<Animator>();
            Publish(new CharacterAnimationEventsChangedEvent(go.GetComponent<CharacterAnimationEvents>()));
        }

        /// <summary>
        /// Removes the character instance and resets visual references.
        /// </summary>
        public void DeSpawn()
        {
            _vanguardMover.Animator = null;
            Publish(new CharacterAnimationEventsChangedEvent(null));
            _destroyChildren.Activate();
        }

        /// <summary>
        /// Resets player position to the world origin.
        /// </summary>
        public void Respawn()
        {
            Stop();
            if (_gridTiles != null && _gridTiles.TryGetValue(Vector2Int.zero, out var origin))
                ReturnToOrigin(origin);
        }

        /// <summary> Commands the mover to stop all movement coroutines. </summary>
        public void Stop() => _vanguardMover.StopMoving();

        /// <summary>
        /// Snaps the player transform to a specific tile and publishes a movement event.
        /// </summary>
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