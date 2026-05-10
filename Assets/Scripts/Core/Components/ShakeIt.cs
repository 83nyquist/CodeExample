using System.Collections;
using UnityEngine;

namespace Core.Components
{
    public class ShakeIt : MonoBehaviour
    {
        public float shakeAmount;//The amount to shake this frame.
        public float shakeDuration;//The duration this frame.

        //Readonly values...
        float shakePercentage;//A percentage (0-1) representing the amount of shake to be applied when setting rotation.
        float startAmount;//The initial shake amount (to determine percentage), set when ShakeIt is called.
        float startDuration;//The initial shake duration, set ShakeIt is called.

        bool isRunning = false; //Is the coroutine running right now?
        bool isRepeat;
        public bool smooth;//Smooth rotation?
        public float smoothAmount = 5f;//Amount to smooth
        private float repeatInterval;

        /// <summary>
        /// Internal method to trigger the shake sequence using current public variables.
        /// </summary>
        void Shake()
        {
            startAmount = shakeAmount;
            startDuration = shakeDuration;
            if (!isRunning) StartCoroutine(ShakeRoutine());
        }

        /// <summary>
        /// Starts a shake effect with specific parameters.
        /// </summary>
        public void Shake(float amount, float duration, bool isRepeat = false, float repeatInterval = 0.5f)
        {
            shakeAmount += amount;
            startAmount = shakeAmount;
            shakeDuration += duration;
            startDuration = shakeDuration;
            this.isRepeat = isRepeat;
            this.repeatInterval = repeatInterval;
            if (!isRunning) StartCoroutine(ShakeRoutine());
        }

        /// <summary>
        /// Coroutine that applies rotation jitter based on shake intensity.
        /// </summary>
        IEnumerator ShakeRoutine()
        {
            isRunning = true;

            while (shakeDuration > 0.01f)
            {
                Vector3 rotationAmount = Random.insideUnitSphere * shakeAmount;
                rotationAmount.z = 0;
                shakePercentage = shakeDuration / startDuration;
                shakeAmount = startAmount * shakePercentage;
                shakeDuration = Mathf.Lerp(shakeDuration, 0, Time.deltaTime);
                if (smooth)
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotationAmount), Time.deltaTime * smoothAmount);
                else
                    transform.localRotation = Quaternion.Euler(rotationAmount);
                yield return null;
            }

            Stop();

            if (isRepeat)
            {
                StartCoroutine(Utilities.WaitAndExecute(repeatInterval, () => Shake(shakeAmount, shakeDuration, true, repeatInterval)));
            }
        }

        /// <summary>
        /// Stops the shaking sequence from repeating.
        /// </summary>
        public void EndRepeat()
        {
            isRepeat = false;
            Stop();
        }

        /// <summary>
        /// Resets the transform rotation and sets running state to false.
        /// </summary>
        private void Stop()
        {
            transform.localRotation = Quaternion.identity;
            isRunning = false;
        }
    }
}
