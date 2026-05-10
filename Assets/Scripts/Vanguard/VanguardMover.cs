using System.Collections;
using System.Collections.Generic;
using Systems.EventBus;
using Systems.Grid.Components;
using UnityEngine;

namespace Vanguard
{
    public class VanguardMover : EventBusSubscriber
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 12f;

        public Animator Animator { get; set; }
    
        private TileData _currentTile;
        private Coroutine _moveCoroutine;

        public TileData CurrentTile => _currentTile;

        /// <summary>
        /// Initiates the coroutine to traverse a provided list of tiles.
        /// </summary>
        public void TraversePath(List<TileData> path)
        {
            if (path == null || path.Count == 0)
            {
                return;
            }

            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(TraversePathRoutine(path));
        }

        /// <summary>
        /// Stops current movement and resets moving flags.
        /// </summary>
        public void StopMoving()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            SetIsMoving(false);
        }

        /// <summary>
        /// Coroutine that iterates through the path tiles and signals completion.
        /// </summary>
        private IEnumerator TraversePathRoutine(List<TileData> path)
        {
            SetIsMoving(true);

            foreach (TileData tile in path)
            {
                if (tile == null)
                {
                    continue;
                }

                yield return MoveToTile(tile);
                _currentTile = tile;
            }

            SetIsMoving(false);
            _moveCoroutine = null;

            EventBusSystem.Publish(new PlayerDestinationReachedEvent(_currentTile));
        }

        /// <summary>
        /// Rotates the transform to face the next destination point.
        /// </summary>
        private void FaceDestination(Vector3 destination)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Coroutine that interpolates the position towards a specific tile.
        /// </summary>
        private IEnumerator MoveToTile(TileData tile)
        {
            if (tile.Decorator == null)
            {
                Debug.LogError($"Attempting to move to tile {tile.AxialCoordinates} but it has no Decorator!");
                yield break;
            }

            Vector3 destination = tile.Decorator.transform.position;

            while (Vector3.Distance(transform.position, destination) > 0.01f)
            {
                FaceDestination(destination);

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }
            
            Publish(new PlayerMovedEvent(tile));
            transform.position = destination;
        }

        /// <summary>
        /// Sets animator parameters and manages input lock requests based on movement state.
        /// </summary>
        private void SetIsMoving(bool isMoving)
        {
            if (isMoving)
            {
                Publish(new InputLockRequest(ToString()));
            }
            else
            {
                Publish(new InputUnlockRequest(ToString()));
            }
            
            if (Animator != null)
            {
                Animator.SetBool(IsMoving, isMoving);
            }
        }
    }
}