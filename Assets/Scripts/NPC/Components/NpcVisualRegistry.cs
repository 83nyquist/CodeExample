using NPC.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NPC.Components
{
    public class NpcVisualRegistry
    {
        private readonly GameObject _prefab;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Transform _parent;
        
        private GameObject[] _visuals;
        private Animator[] _animators;
        private Vector3[] _lastPositions;  // Track last position to detect movement
        
        public NpcVisualRegistry(GameObject prefab, float moveSpeed, float rotationSpeed, Transform parent)
        {
            _prefab = prefab;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _parent = parent;
        }
        
        public void PrepareRegistry(int totalCount)
        {
            _visuals = new GameObject[totalCount];
            _animators = new Animator[totalCount];
            _lastPositions = new Vector3[totalCount];
        }

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
        
        public void UpdateVisuals(NativeArray<NpcData> npcs, System.Func<int2, Vector3> hexToWorld, float deltaTime)
        {
            for (int i = 0; i < _visuals.Length; i++)
            {
                if (_visuals[i] == null) continue;
                
                UpdatePositionAndRotation(i, npcs[i].Position, hexToWorld, deltaTime);
                UpdateVisibility(i, npcs[i].IsVisible);
                UpdateAnimatorState(i);  // Update based on actual movement, not NpcData.IsMoving
            }
        }
        
        private void UpdatePositionAndRotation(int index, int2 targetPosition, System.Func<int2, Vector3> hexToWorld, float deltaTime)
        {
            Vector3 targetPos = hexToWorld(targetPosition);
            Vector3 currentPos = _visuals[index].transform.position;
            
            if (Vector3.Distance(currentPos, targetPos) > 0.01f)
            {
                // Moving
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
                // Arrived at destination
                _visuals[index].transform.position = targetPos;
            }
        }
        
        private void UpdateVisibility(int index, bool isVisible)
        {
            if (_visuals[index].activeSelf != isVisible)
                _visuals[index].SetActive(isVisible);
        }
        
        private void UpdateAnimatorState(int index)
        {
            if (_animators[index] == null) return;
            
            // Check if position changed since last frame
            Vector3 currentPos = _visuals[index].transform.position;
            bool isMoving = Vector3.Distance(currentPos, _lastPositions[index]) > 0.001f;
            
            // Only update animator if state changed
            bool currentAnimatorState = _animators[index].GetBool("IsMoving");
            if (currentAnimatorState != isMoving)
            {
                _animators[index].SetBool("IsMoving", isMoving);
            }
            
            // Store current position for next frame
            _lastPositions[index] = currentPos;
        }
        
        public void UpdateAnimatorStateFromData(int index, bool isMoving)
        {
            if (_animators[index] != null && _animators[index].GetBool("IsMoving") != isMoving)
                _animators[index].SetBool("IsMoving", isMoving);
        }
        
        public bool TryGetAnimator(int index, out Animator animator)
        {
            animator = null;
            if (index < 0 || index >= _animators.Length) return false;
            animator = _animators[index];
            return animator != null;
        }
        
        public void Dispose()
        {
            if (_visuals == null) return;
    
            // Destroy all visual GameObjects
            for (int i = 0; i < _visuals.Length; i++)
            {
                if (_visuals[i] != null)
                    Object.Destroy(_visuals[i]);
            }
    
            _visuals = null;
            _animators = null;
            _lastPositions = null;
        }
    }
}