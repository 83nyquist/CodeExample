using UnityEngine;

namespace Core.Components
{
    public class PunchGui : MonoBehaviour
    {
        private readonly float _distanceThreshold = 0.0001f;

        private Vector3 _startScale;
        private Vector3 _targetScale;

        private bool _isPunching;
        private bool _midReached;

        private float _speed;
        private float _lingerDuration;
        private float _punchFactor;

        private Vector2 _moveDirection = Vector2.zero;
        
        [SerializeField]
        private RectTransform rectTransform;

        
        void Update ()
        {
            if (_isPunching)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, _targetScale, Time.deltaTime * _speed);

                if (Vector3.Distance(transform.localScale, _targetScale) < _distanceThreshold)
                {
                    if (_midReached)
                    {
                        Stop();
                    }
                    else
                    {
                        _targetScale = _startScale;
                        _midReached = true;
                    }
                }
            }
              
            if (_midReached)
            {
                if (_moveDirection != Vector2.zero && rectTransform != null)
                {
                    // Calculate the movement based on vector magnitude
                    rectTransform.anchoredPosition += _moveDirection * Time.deltaTime;
                }
            }
        }

        public void Punch(
            float punchFactor, 
            float speed,
            float lingerDuration = 0)
        {
            _punchFactor = punchFactor;
            _speed = speed;
            _lingerDuration = lingerDuration;
            
            _startScale = transform.localScale;
            _targetScale = transform.localScale * _punchFactor;

            _midReached = false;
            _isPunching = true;
        }
        
        public void SetMoveDirection(Vector2 direction)
        {
            _moveDirection = direction;
        }

        public void Stop()
        {
            transform.localScale = _startScale;

            _isPunching = false;
            Destroy(gameObject, _lingerDuration);
        }
    }
}
