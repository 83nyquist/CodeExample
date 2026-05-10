using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Core.Systems.BezierInterpolator
{
    [Serializable]
    public class BezierController : MonoBehaviour
    {
        public enum Modes { PlayOnce, Repeate }
        public enum TimedActionsModes { Ignore, Controller, Item }
    
        [Header("Settings")]
        public AnimationCurve Speed;
        public AnimationCurve Scale;
        public AnimationCurve RotationZ;
        public Modes Mode = Modes.PlayOnce;
        public TimedActionsModes TimedActionsMode = TimedActionsModes.Controller;
        public bool AutoRun = false;
        public int SpawnAmount = 50;
        public float SpawnInterval = 0.1f;
        
        [Space]
        [Header("Gameobjects")]
        public bool ShowDebugging = true;
        public GameObject Prefab;
        public Transform ParentGameObject;
        public GameObject StartGameObject;
        public GameObject StartHandleGameObject;
        public GameObject EndHandleGameObject;
        public GameObject EndGameObject;

        [Space]
        [Header("Handles")]
        public bool SpawnHandleVariance;
        public Vector3 StartHandleVariance = new Vector3(0, 0, 0);
        public Vector3 EndHandleVariance = new Vector3(0, 0, 0);

        public UnityAction OnRun;
        public UnityAction<BezierItem> OnItemComplete;
        public UnityAction OnSequenceComplete;
        
        public UnityEvent OnRunEvent;
        public UnityEvent OnSequenceCompleteEvent;

        public List<TimedAction> TimedActions = new List<TimedAction>();
        public List<TimedAction> TimedEvents = new List<TimedAction>();

        private Vector3 _startVector3;
        private Vector3 _endVector3;
        private List<BezierItem> _items = new List<BezierItem>();
        /// <summary>
        /// Executes AutoRun if enabled.
        /// </summary>
        void Start()
        {
            if (AutoRun)
            {
                Run();
            }
        }

        /// <summary>
        /// Starts the sequence from a specific GameObject position.
        /// </summary>
        public void Run(GameObject startObject)
        {
            StartGameObject = startObject;
            _startVector3 = StartGameObject.transform.position;
            Run();
        }

        /// <summary>
        /// Starts the sequence from a specific world position.
        /// </summary>
        public void Run(Vector3 startVector3)
        {
            _startVector3 = startVector3;
            Run();
        }

        /// <summary>
        /// Triggers the sequential initiation of Bezier items.
        /// </summary>
        public void Run()
        {
            if (OnRun != null)
            {
                OnRun.Invoke();
            }

            if (OnRunEvent != null)
            {
                OnRunEvent.Invoke();
            }

            _endVector3 = EndGameObject.transform.position;
            StartCoroutine(SequentialInitiation());
        }

        /// <summary>
        /// Spawns items over time at the defined spawn interval.
        /// </summary>
        IEnumerator SequentialInitiation()
        {
            for (int i = 0; i < SpawnAmount; i++)
            {
                GameObject tmp = Instantiate(Prefab, _startVector3, Quaternion.identity);
                if (ParentGameObject != null) { tmp.transform.SetParent(ParentGameObject); }

                BezierItem item = tmp.AddComponent<BezierItem>();
                item.Start = _startVector3;
                item.End = _endVector3;
                item.OnComplete += OnItemComplete;
                item.Mode = Mode;
                _items.Add(item);

                if (TimedActionsMode == TimedActionsModes.Item)
                {
                    foreach (TimedAction ev in TimedActions)
                    {
                        item.TimedActions.Add(new TimedAction(ev.Time, ev.GetAction()));
                    }
                }

                float startScale = Scale.Evaluate(0f);
                tmp.transform.localScale = new Vector3(startScale, startScale, startScale);

                item.HandleA = GenerateStartHandle();
                item.HandleB = GenerateEndHandle();

                item.Run(this);
                yield return new WaitForSeconds(SpawnInterval);
            }

            yield return null;
        }
        /// <summary>
        /// Removes destroyed items from tracking and signals sequence completion if empty.
        /// </summary>
        public void ItemDestroyed(BezierItem item)
        {
            _items.Remove(item);

            if (_items.Count <= 0)
            {
                if (OnSequenceComplete != null)
                {
                    OnSequenceComplete.Invoke();
                    OnSequenceComplete = null;
                }

                if (OnSequenceCompleteEvent != null)
                {
                    OnSequenceCompleteEvent.Invoke();
                    OnSequenceCompleteEvent = null;
                }
            }
        }

        /// <summary>
        /// Generates a world position for the start handle including optional variance.
        /// </summary>
        private Vector3 GenerateStartHandle()
        {
            if (!SpawnHandleVariance)
            {
                return StartHandleGameObject.transform.position;
            }

            return StartHandleGameObject.transform.position + new Vector3(
                Random.Range(-StartHandleVariance.x, StartHandleVariance.x),
                Random.Range(-StartHandleVariance.y, StartHandleVariance.y),
                Random.Range(-StartHandleVariance.z, StartHandleVariance.z));
        }

        /// <summary>
        /// Generates a world position for the end handle including optional variance.
        /// </summary>
        private Vector3 GenerateEndHandle()
        {
            if (!SpawnHandleVariance)
            {
                return EndHandleGameObject.transform.position;
            }

            return EndHandleGameObject.transform.position + new Vector3(
                Random.Range(-EndHandleVariance.x, EndHandleVariance.x),
                Random.Range(-EndHandleVariance.y, EndHandleVariance.y),
                Random.Range(-EndHandleVariance.z, EndHandleVariance.z));
        }

        /// <summary>
        /// Draws the Bezier path and handle guides in the editor.
        /// </summary>
        void OnDrawGizmos()
        {
            if (!ShowDebugging)
            {
                return;
            }
        
            Color oldColor = Gizmos.color;

            if (StartGameObject == null ||
                StartHandleGameObject == null ||
                EndHandleGameObject == null ||
                EndGameObject == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 old = BezierItem.Cube3(
                StartGameObject.transform.position,
                StartHandleGameObject.transform.position,
                EndHandleGameObject.transform.position,
                EndGameObject.transform.position,
                0);
            Vector3 next;

            for (float i = 0; i <= 1.05; i += 0.1f)
            {
                next = BezierItem.Cube3(
                    StartGameObject.transform.position,
                    StartHandleGameObject.transform.position,
                    EndHandleGameObject.transform.position,
                    EndGameObject.transform.position,
                    i);

                Gizmos.DrawLine(old, next);
                old = next;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(StartGameObject.transform.position, StartHandleGameObject.transform.position);
            Gizmos.DrawLine(EndGameObject.transform.position, EndHandleGameObject.transform.position);

            Gizmos.DrawIcon(StartHandleGameObject.transform.position, "Start Handle");
            Gizmos.DrawIcon(EndHandleGameObject.transform.position, "End Handle");

            Gizmos.color = oldColor;
        }
    }

    [Serializable]
    public class TimedAction : ScriptableObject
    {
        public float Time;
        public bool Triggered;

        private UnityAction<BezierItem> _action;
        /// <summary>
        /// Initializes a new timed action for a Bezier sequence.
        /// </summary>
        public TimedAction(float time, UnityAction<BezierItem> action)
        {
            Triggered = false;
            Time = time;
            _action = action;
        }

        /// <summary>
        /// Returns the action delegate.
        /// </summary>
        public UnityAction<BezierItem> GetAction()
        {
            return _action;
        }

        /// <summary>
        /// Checks if the provided time exceeds the action threshold and triggers the action once.
        /// </summary>
        public void CheckAction(float time, BezierItem item)
        {
            if (!Triggered)
            {
                if (time >= Time)
                {
                    Triggered = true;
                    _action.Invoke(item);
                }
            }
        }
    }

    [Serializable]
    public class TimedEvent : ScriptableObject
    {
        public float Time;
        public bool Triggered;

        private UnityEvent<BezierItem> _ev;
        /// <summary>
        /// Initializes a new timed event for a Bezier sequence.
        /// </summary>
        public TimedEvent(float time, UnityEvent<BezierItem> ev)
        {
            Triggered = false;
            Time = time;
            _ev = ev;
        }

        /// <summary>
        /// Returns the event delegate.
        /// </summary>
        public UnityEvent<BezierItem> GetEvent()
        {
            return _ev;
        }

        /// <summary>
        /// Checks if the provided time exceeds the event threshold and triggers the event once.
        /// </summary>
        public void CheckEvent(float time, BezierItem item)
        {
            if (!Triggered)
            {
                if (time >= Time)
                {
                    Triggered = true;
                    _ev.Invoke(item);
                }
            }
        }
    }
}