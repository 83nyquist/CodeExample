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

        void Shake()
        {

            startAmount = shakeAmount;//Set default (start) values
            startDuration = shakeDuration;//Set default (start) values

            if (!isRunning) StartCoroutine(ShakeRoutine());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
        }

        public void Shake(float amount, float duration, bool isRepeat = false, float repeatInterval = 0.5f)
        {

            shakeAmount += amount;//Add to the current amount.
            startAmount = shakeAmount;//Reset the start amount, to determine percentage.
            shakeDuration += duration;//Add to the current time.
            startDuration = shakeDuration;//Reset the start time.
            this.isRepeat = isRepeat;
            this.repeatInterval = repeatInterval;

            if (!isRunning) StartCoroutine(ShakeRoutine());//Only call the coroutine if it isn't currently running. Otherwise, just set the variables.
        }


        IEnumerator ShakeRoutine()
        {
            isRunning = true;

            while (shakeDuration > 0.01f)
            {
                Vector3 rotationAmount = Random.insideUnitSphere * shakeAmount;//A Vector3 to add to the Local Rotation
                rotationAmount.z = 0;//Don't change the Z; it looks funny.

                shakePercentage = shakeDuration / startDuration;//Used to set the amount of shake (% * startAmount).

                shakeAmount = startAmount * shakePercentage;//Set the amount of shake (% * startAmount).
                shakeDuration = Mathf.Lerp(shakeDuration, 0, Time.deltaTime);//Lerp the time, so it is less and tapers off towards the end.


                if (smooth)
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotationAmount), Time.deltaTime * smoothAmount);
                else
                    transform.localRotation = Quaternion.Euler(rotationAmount);//Set the local rotation the be the rotation amount.

                yield return null;
            }

            Stop();

            if (isRepeat)
            {
                StartCoroutine(Utilities.WaitAndExecute(repeatInterval, () => Shake(shakeAmount, shakeDuration, true, repeatInterval)));
            }
        }

        public void EndRepeat()
        {
            isRepeat = false;

            Stop();
        }

        private void Stop()
        {
            transform.localRotation = Quaternion.identity;//Set the local rotation to 0 when done, just to get rid of any fudging stuff.
            isRunning = false;
        }
    }
}
