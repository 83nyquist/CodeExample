using System.Collections.Generic;
using Systems.Grid.Components;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.NonPlayerCharacters.Components
{
    public class NpcVisualRegistry
    {
        private readonly GameObject _prefab;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Transform _parent;
        
        private readonly HashSet<int2> _visibleCoords = new HashSet<int2>();
        private bool _forceVisible;

        private GameObject[] _visuals;
        private Animator[] _animators;
        private Vector3[] _lastPositions;
        
        /// <summary>
        /// Initializes the registry with prefab and movement settings.
        /// </summary>
        public NpcVisualRegistry(GameObject prefab, float moveSpeed, float rotationSpeed, Transform parent)
        {
            _prefab = prefab;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _parent = parent;
        }
        
        /// <summary>
        /// Prepares internal arrays based on the total population size.
        /// </summary>
        public void PrepareRegistry(int totalCount)
        {
            _visuals = new GameObject[totalCount];
            _animators = new Animator[totalCount];
            _lastPositions = new Vector3[totalCount];
        }

        /// <summary>
        /// Instantiates GameObjects for a specific range of NPCs and caches their components.
        /// </summary>
        public void CreateVisualsInRange(NativeSlice<NpcData> npcSlice, int startIndex, System.Func<int2, Vector3> hexToWorld)
        {
            if (_visuals == null) return;

            for (int i = 0; i < npcSlice.Length; i++)
            {
                int globalIndex = startIndex + i;
                Vector3 worldPos = hexToWorld(npcSlice[i].Position);
                _visuals[globalIndex] = Object.Instantiate(_prefab, worldPos, Quaternion.identity, _parent);
                _visuals[globalIndex].name = $"NPC_{npcSlice[i].Id}";
                _animators[globalIndex] = _visuals[globalIndex].GetComponent<Animator>();
                _lastPositions[globalIndex] = worldPos;
            }
        }
        
        /// <summary>
        /// Caches the current vision set coordinates to determine visibility during the visual update loop.
        /// </summary>
        public void UpdateVisibilityStates(NativeArray<NpcData> npcs, HashSet<TileData> visionSet, bool forceVisible)
        {
            _forceVisible = forceVisible;
            _visibleCoords.Clear();
            
            if (visionSet != null)
            {
                foreach (var tile in visionSet)
                {
                    // Mapping TileData X/Z to the int2 coordinate used by NPCs
                    _visibleCoords.Add(new int2(tile.X, tile.Z));
                }
            }
        }

        /// <summary>
        /// Updates all active NPC GameObjects, handling position interpolation, rotation, and visibility.
        /// </summary>
        public void UpdateVisuals(NativeArray<NpcData> npcs, System.Func<int2, Vector3> hexToWorld, float deltaTime)
        {
            for (int i = 0; i < _visuals.Length; i++)
            {
                if (_visuals[i] == null) continue;
                
                UpdatePositionAndRotation(i, npcs[i].Position, hexToWorld, deltaTime);

                bool isVisible = _forceVisible || _visibleCoords.Contains(npcs[i].Position);
                UpdateVisibility(i, isVisible);

                UpdateAnimatorState(i);
            }
        }
        
        /// <summary>
        /// Interpolates the GameObject transform towards the target hex position.
        /// </summary>
        private void UpdatePositionAndRotation(int index, int2 targetPosition, System.Func<int2, Vector3> hexToWorld, float deltaTime)
        {
            Vector3 targetPos = hexToWorld(targetPosition);
            Vector3 currentPos = _visuals[index].transform.position;
            
            if (Vector3.Distance(currentPos, targetPos) > 0.01f)
            {
                _visuals[index].transform.position = Vector3.MoveTowards(currentPos, targetPos, _moveSpeed * deltaTime);
                
                Vector3 moveDirection = (targetPos - currentPos).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                
                _visuals[index].transform.rotation = Quaternion.Slerp(
                    _visuals[index].transform.rotation,
                    targetRotation,
                    _rotationSpeed * deltaTime
                );
            }
            else
            {
                _visuals[index].transform.position = targetPos;
            }
        }
        
        /// <summary>
        /// Sets the active state of the NPC GameObject based on visibility calculations.
        /// </summary>
        private void UpdateVisibility(int index, bool isVisible)
        {
            if (_visuals[index].activeSelf != isVisible)
                _visuals[index].SetActive(isVisible);
        }
        
        /// <summary>
        /// Updates the animator's "IsMoving" parameter based on delta movement.
        /// </summary>
        private void UpdateAnimatorState(int index)
        {
            if (_animators[index] == null) return;
            
            Vector3 currentPos = _visuals[index].transform.position;
            bool isMoving = Vector3.Distance(currentPos, _lastPositions[index]) > 0.001f;
            
            bool currentAnimatorState = _animators[index].GetBool("IsMoving");
            if (currentAnimatorState != isMoving)
            {
                _animators[index].SetBool("IsMoving", isMoving);
            }
            
            _lastPositions[index] = currentPos;
        }
        
        /// <summary>
        /// Directly updates the animator state using simulation data.
        /// </summary>
        public void UpdateAnimatorStateFromData(int index, bool isMoving)
        {
            if (_animators[index] != null && _animators[index].GetBool("IsMoving") != isMoving)
                _animators[index].SetBool("IsMoving", isMoving);
        }
        
        /// <summary>
        /// Attempts to retrieve the animator component for a specific NPC index.
        /// </summary>
        public bool TryGetAnimator(int index, out Animator animator)
        {
            animator = null;
            if (index < 0 || index >= _animators.Length) return false;
            animator = _animators[index];
            return animator != null;
        }
        
        /// <summary>
        /// Destroys all NPC GameObjects and clears internal registries.
        /// </summary>
        public void Dispose()
        {
            if (_visuals == null) return;
    
            for (int i = 0; i < _visuals.Length; i++)
            {
                if (_visuals[i] != null)
                    Object.Destroy(_visuals[i]);
            }
    
            _visuals = null;
            _animators = null;
            _lastPositions = null;
            _visibleCoords.Clear();
        }
    }
}