using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Systems.BezierInterpolator
{
    public class BezierItem : MonoBehaviour
    {
        public bool IsActive;

        [Range(0, 1)] 
        public float BezierTime;
        public Transform Parent;
        public Vector3 Start;
        public Vector3 HandleA;
        public Vector3 HandleB;
        public Vector3 End;
        public BezierController.Modes Mode;
        public UnityAction<BezierItem> OnComplete;

        private BezierController Owner;
        private float _frameIncrement;
        private Vector3 _targetPos;

        public List<TimedAction> TimedActions = new List<TimedAction>();

        void Update()
        {
            if (IsActive)
            {
                Interpolate();
            }
        }

        public void Interpolate()
        {
            _targetPos = Cube3(Start, HandleA, HandleB, End, BezierTime);

            transform.position = Vector3.Lerp(transform.position, _targetPos, 1);

            _frameIncrement = Owner.Speed.Evaluate(BezierTime);

            if (_frameIncrement <= 0)
            {
                _frameIncrement = 0.1f;
            }

            BezierTime += _frameIncrement * Time.deltaTime;
            CheckEvents(BezierTime);

            float scale = Owner.Scale.Evaluate(BezierTime);
            transform.localScale = new Vector3(scale, scale, scale);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Owner.RotationZ.Evaluate(BezierTime));

            if (BezierTime > 1)
            {
                EndInterpolation();
            }
        }

        public void Run(BezierController owner)
        {
            Owner = owner;
            BezierTime = 0;
            IsActive = true;
        }

        public void CheckEvents(float time)
        {
            if (Owner.TimedActionsMode == BezierController.TimedActionsModes.Ignore)
            {
                return;
            }

            if (Owner.TimedActionsMode == BezierController.TimedActionsModes.Controller)
            {
                foreach (TimedAction ev in Owner.TimedActions)
                {
                    ev.CheckAction(time, this);
                }
            }
            else if (Owner.TimedActionsMode == BezierController.TimedActionsModes.Item)
            {
                foreach (TimedAction ev in TimedActions)
                {
                    ev.CheckAction(time, this);
                }
            }
        }

        public void EndInterpolation()
        {
            if (OnComplete != null)
            {
                OnComplete.Invoke(this);
            }

            if (Mode == BezierController.Modes.PlayOnce)
            {
                Owner.ItemDestroyed(this);
                Destroy(gameObject);
            }
            else
            {
                BezierTime = 0;
            }

            IsActive = false;
        }

        public static Vector3 Cube3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return (((-p0 + 3 * (p1 - p2) + p3) * t + (3 * (p0 + p2) - 6 * p1)) * t + 3 * (p1 - p0)) * t + p0;
        }

        //public static Vector3 Bezier2(Vector3 s, Vector3 p, Vector3 e, float t)
        //{
        //    float rt = 1 - t;
        //    return rt * rt * s + 2 * rt * t * p + t * t * e;
        //}
    }
}
