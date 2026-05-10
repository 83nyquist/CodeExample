using System.Collections;
using UnityEngine;

namespace Core.Components
{
    public class RotateAnimation : MonoBehaviour
    {
        public bool StartImidiatly;
        private bool _isRotating;

        public float _Angle;
        public float _Period;
        private float _Time;
        private float shakeDuration;
        private float actualShakeDuration;
        private float repeatInterval;

        /// <summary>
        /// Starts the shake animation once at the beginning if StartImidiatly is set.
        /// </summary>
        void Start()
        {
            if (StartImidiatly)
            {
                ShakeOnce(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Updates the rotation state based on a sine wave phase if rotation is active.
        /// </summary>
        void Update()
        {
            if (_isRotating)
            {
                _Time = _Time + Time.deltaTime;
                float phase = Mathf.Sin(_Time / _Period);
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, phase * _Angle));
            }
        }

        /// <summary>
        /// Initiates a rotation animation for a specific duration.
        /// </summary>
        public void ShakeOnce(float shakeDuration, float repeatInterval = 0)
        {
            this.shakeDuration = shakeDuration;
            this.repeatInterval = repeatInterval;
            actualShakeDuration = shakeDuration;
            StartCoroutine(ShakeRoutine());
        }

        /// <summary>
        /// Coroutine that handles the rotation logic over the specified shake duration.
        /// </summary>
        IEnumerator ShakeRoutine()
        {
            _isRotating = true;
            while (actualShakeDuration > 0.01f)
            {
                _Time = _Time + Time.deltaTime;
                float phase = Mathf.Sin(_Time / _Period);
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, phase * _Angle));
                actualShakeDuration -= Time.deltaTime;
                yield return null;
            }
            Stop();
            if (repeatInterval > 0)
            {
                StartCoroutine(Utilities.WaitAndExecute(repeatInterval, () => ShakeOnce(shakeDuration, repeatInterval)));
            }
        }

        /// <summary>
        /// Resets the local rotation and stops the animation.
        /// </summary>
        public void Stop()
        {
            transform.localRotation = Quaternion.identity;
            _isRotating = false;
        }

        /// <summary>
        /// Disables the repeat interval and stops the current animation.
        /// </summary>
        public void EndRepeat()
        {
            repeatInterval = 0;

            Stop();
        }
    }
}
