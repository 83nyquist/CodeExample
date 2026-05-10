using System.Collections.Generic;
using UnityEngine;

namespace Systems.Decoration.Components
{
    public class TileDecoratorAnimator : MonoBehaviour
    {
        [SerializeField] private Vector3 spawnOffset = new Vector3(0, -5, 0);
        [SerializeField] private float transitionDuration = 0.5f;

        private class AnimationTask
        {
            public Vector3 StartPos;
            public Vector3 TargetPos;
            public float Elapsed;
        }

        private readonly Dictionary<Transform, AnimationTask> _activeAnimations = new();
        private readonly List<Transform> _completedTasks = new();

        /// <summary>
        /// Starts a movement animation from an offset position to the target position.
        /// </summary>
        public void Register(Transform targetTransform, Vector3 targetPosition)
        {
            if (transitionDuration <= 0 || !Application.isPlaying)
            {
                targetTransform.position = targetPosition;
                return;
            }

            _activeAnimations[targetTransform] = new AnimationTask
            {
                StartPos = targetPosition + spawnOffset,
                TargetPos = targetPosition,
                Elapsed = 0f
            };
            
            targetTransform.position = targetPosition + spawnOffset;
        }

        /// <summary>
        /// Immediately stops tracking an animation for a specific transform.
        /// </summary>
        public void Cancel(Transform targetTransform)
        {
            _activeAnimations.Remove(targetTransform);
        }

        /// <summary>
        /// Updates all active animations and cleans up completed tasks.
        /// </summary>
        private void Update()
        {
            if (_activeAnimations.Count == 0) return;

            _completedTasks.Clear();
            foreach (var kvp in _activeAnimations)
            {
                var transform = kvp.Key;
                var task = kvp.Value;

                task.Elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(task.Elapsed / transitionDuration);
                transform.position = Vector3.Lerp(task.StartPos, task.TargetPos, t);

                if (t >= 1.0f) _completedTasks.Add(transform);
            }

            foreach (var finished in _completedTasks)
                _activeAnimations.Remove(finished);
        }
    }
}