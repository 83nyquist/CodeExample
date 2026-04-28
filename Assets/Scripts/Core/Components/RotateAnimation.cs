using System.Collections;
using UnityEngine;

namespace Core.Components
{
    public class RotateAnimation : MonoBehaviour
    {
        public bool StartImidiatly;
        private bool _isRotating;
        //private bool _isPositive;
        //public float speed = 30;
        //public float maxRotation = 30;

        public float _Angle;
        public float _Period;

        private float _Time;

        private float shakeDuration;
        private float actualShakeDuration;
        private float repeatInterval;

        void Start()
        {
            if (StartImidiatly)
            {
                GetComponent<RotateAnimation>().ShakeOnce(0.5f, 0.5f);
            }
        }

        void Update()
        {
            if (_isRotating)
            {
                _Time = _Time + Time.deltaTime;
                float phase = Mathf.Sin(_Time / _Period);
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0, phase * _Angle));
            }
        }

        public void ShakeOnce(float shakeDuration, float repeatInterval = 0)
        {
            this.shakeDuration = shakeDuration;
            this.repeatInterval = repeatInterval;

            actualShakeDuration = shakeDuration;

            //if (!_isRotating) StartCoroutine(ShakeRoutine());
            StartCoroutine(ShakeRoutine());
        }

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

        public void Stop()
        {
            transform.localRotation = Quaternion.identity;
            _isRotating = false;
        }

        public void EndRepeat()
        {
            repeatInterval = 0;

            Stop();
        }
    }
}
