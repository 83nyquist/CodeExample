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
        //public enum InterpolationTypes { Cube3, Bez2 }
        public enum Modes { PlayOnce, Repeate }
        public enum TimedActionsModes { Ignore, Controller, Item }
    
        [Header("Settings")]

        [Tooltip("The speed in witch the prefab traverse the bezier path at any given point of the animation duration. (Time 0 -> 1)")]
        public AnimationCurve Speed;

        [Tooltip("Scale of the prefab at any given point of the animation duration, (Time 0 -> 1)")]
        public AnimationCurve Scale;

        [Tooltip("Rotate along the Z axis based on the time of the animation. (Time 0 -> 1, Scale 0 -> 360)")]
        public AnimationCurve RotationZ;

        [Tooltip("Specify if each individual item will only play once or repeate when reacing their destination.")]
        public Modes Mode = Modes.PlayOnce;

        //[Tooltip("The type of calculation used to determine the bezier path. Cube3 utilizes both handles to enable oscillating behavior, while Bez2 only use the StartHandle.")]
        //public InterpolationTypes InterpolationType = InterpolationTypes.Cube3;

        [Tooltip("Set the behaviour of the timed actions. Add a new action to the TimedActions list from code. " +
                 "\nIgnore: Do nothing " +
                 "\nController: Fire each action once the first time the first prefab hits its mark." +
                 "\nItem: Fire each action everytime an item hits its mark.")]
        public TimedActionsModes TimedActionsMode = TimedActionsModes.Controller;

        [Tooltip("Enable to Run the animation in the Controllers Start function.")]
        public bool AutoRun = false;

        [Tooltip("Amount of prefabs to spawn for one given animation.")]
        public int SpawnAmount = 50;

        [Tooltip("The interval at wich each item spawns.")]
        public float SpawnInterval = 0.1f;

    
        [Space]
        [Header("Gameobjects")]

        [Tooltip("Show pretty lines. (Require select Gameobjects, check tooltips.)")]
        public bool ShowDebugging = true;

        [Tooltip("Prefab to spawn along the path.")]
        public GameObject Prefab;

        [Tooltip("Container to hold all spawned instances of the prefab. (OPTIONAL)")]
        public Transform ParentGameObject;

        [Tooltip("Starting Gameobject that denotes the start of the path.")]
        public GameObject StartGameObject;

        [Tooltip("Start node handle that dictates the arc of the prefab when exiting the start node.")]
        public GameObject StartHandleGameObject;

        [Tooltip("End node handle that dictates the arc of the prefab when entering the end node. (NOT REQUIRED IN BEZ2 MODE)")]
        public GameObject EndHandleGameObject;

        [Tooltip("Ending Gameobject that denotes the end of the path")]
        public GameObject EndGameObject;
    

        [Space]
        [Header("Handles")]
    
        [Tooltip("Enables a variance in where the items are spawned and where they end up relative to the start and end object.")]
        public bool SpawnHandleVariance;
    
        public Vector3 StartHandleVariance = new Vector3(0, 0, 0);
        public Vector3 EndHandleVariance = new Vector3(0, 0, 0);
    
        //Actions. Pref used from code
        public UnityAction OnRun;
        public UnityAction<BezierItem> OnItemComplete;
        public UnityAction OnSequenceComplete;

        //Events. Pref used from inspector
        public UnityEvent OnRunEvent;
        public UnityEvent OnSequenceCompleteEvent;

        public List<TimedAction> TimedActions = new List<TimedAction>();
        public List<TimedAction> TimedEvents = new List<TimedAction>();

        private Vector3 _startVector3;
        private Vector3 _endVector3;
        private List<BezierItem> _items = new List<BezierItem>();

        void Start()
        {
            if (AutoRun)
            {
                Run();
            }
        }

        public void Run(GameObject startObject)
        {
            StartGameObject = startObject;
            _startVector3 = StartGameObject.transform.position;
            Run();
        }

        public void Run(Vector3 startVector3)
        {
            _startVector3 = startVector3;
            Run();
        }

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

        IEnumerator SequentialInitiation()
        {
            for (int i = 0; i < SpawnAmount; i++)
            {
                GameObject tmp = Instantiate(Prefab, _startVector3, Quaternion.identity);
                if (ParentGameObject != null) { tmp.transform.SetParent(ParentGameObject); }
                ///tmp.transform.localScale = Vector3.one;

                BezierItem item = tmp.AddComponent<BezierItem>();
                item.Start = _startVector3;
                item.End = _endVector3;
                item.OnComplete += OnItemComplete;
                item.Mode = Mode;
                _items.Add(item);

                if (TimedActionsMode == TimedActionsModes.Item)
                {
                    //Copy the action to the item
                    foreach (TimedAction ev in TimedActions)
                    {
                        item.TimedActions.Add(new TimedAction(ev.Time, ev.GetAction()));
                    }
                }

                float startScale = Scale.Evaluate(0f);
                tmp.transform.localScale = new Vector3(startScale, startScale, startScale);

                item.HandleA = GenerateStartHandle();
                item.HandleB = GenerateEndHandle();
                //item.HandleB = (InterpolationType == InterpolationTypes.Cube3) ? GenerateEndHandle() : Vector3.zero;

                item.Run(this);
                yield return new WaitForSeconds(SpawnInterval);
            }

            yield return null;
        }
    
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

            //Draw Path
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

            //Draw Handle guides
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

        public TimedAction(float time, UnityAction<BezierItem> action)
        {
            Triggered = false;
            Time = time;
            _action = action;
        }

        public UnityAction<BezierItem> GetAction()
        {
            return _action;
        }

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

        public TimedEvent(float time, UnityEvent<BezierItem> ev)
        {
            Triggered = false;
            Time = time;
            _ev = ev;
        }

        public UnityEvent<BezierItem> GetEvent()
        {
            return _ev;
        }

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