using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Input;
using Systems.Grid;
using UnityEngine;
using Zenject;

namespace Vanguard
{
    public class VanguardMover : MonoBehaviour
    {
        [Inject] private InputHandler _inputHandler;
        [Inject] private VanguardController _vanguardController;
     
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 12f;

        public event Action<TileData> OnDestinationReached;
        public event Action<TileData> OnPathNodeReached;
    
        private TileData _currentTile;
        private InputLock _inputLock;
        private Coroutine _moveCoroutine;
    
        private void Awake()
        {
            _inputLock = _inputHandler.RegisterInputLock(this);
        }

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

        public void StopMoving()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            SetIsMoving(false);
        }

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

            OnDestinationReached?.Invoke(_currentTile);
        }

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

        private IEnumerator MoveToTile(TileData tile)
        {
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
            
            OnPathNodeReached?.Invoke(tile);
            transform.position = destination;
        }

        private void SetIsMoving(bool isMoving)
        {
            _inputLock.IsLocked = isMoving;
            if (animator != null)
            {
                animator.SetBool(IsMoving, isMoving);
            }
        }
    }
}
