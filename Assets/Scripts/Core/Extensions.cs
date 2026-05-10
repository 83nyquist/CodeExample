using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Core
{
    /// <summary>
    /// Extension methods for convenience.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Executes an action on the subject if it is not null. Handles Unity-specific null checks.
        /// </summary>
        public static void IfNotNull<T>(this T subject, Action<T> method)
            where T : class
        {
            if (IsNull(subject) || method == null)
            {
                return;
            }

            var subjectType = typeof(T);

            if (subjectType.IsValueType)
            {
                if (Nullable.GetUnderlyingType(subjectType) == null)
                {
                    return;
                }
            }

            if (subject is Object)
            {
                var s = subject as Object;

                if (s == null)
                {
                    return;
                }
            }

            method.Invoke(subject);
        }

        /// <summary>
        /// Executes a function on the subject and returns the result if not null; otherwise returns default.
        /// </summary>
        public static TR IfNotNull<T, TR>(this T subject, Func<T, TR> method)
            where T : class
        {
            if (IsNull(subject) || method == null)
            {
                return default(TR);
            }

            var subjectType = typeof(T);

            if (subjectType.IsValueType)
            {
                if (Nullable.GetUnderlyingType(subjectType) == null)
                {
                    return default(TR);
                }
            }

            return method.Invoke(subject);
        }

        /// <summary>
        /// Performs a generic null check using the default equality comparer.
        /// </summary>
        private static bool IsNull<T>(T obj)
            where T : class
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }

        /// <summary>
        /// Invokes a Func and returns the result, or default if the Func is null.
        /// </summary>
        public static T SafeInvoke<T>(this Func<T> funcT)
        {
            return funcT == null ? default(T) : funcT.Invoke();
        }

        /// <summary>
        /// Invokes a Func with input and returns the result, or default if the Func is null.
        /// </summary>
        public static TR SafeInvoke<T, TR>(this Func<T, TR> funcT, T input)
        {
            return funcT == null ? default(TR) : funcT.Invoke(input);
        }

        /// <summary>
        /// Invokes an Action if it is not null.
        /// </summary>
        public static void SafeInvoke(this Action action)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke();
        }

        /// <summary>
        /// Invokes an Action with an argument if it is not null.
        /// </summary>
        public static void SafeInvoke<T>(this Action<T> action, T argument)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke(argument);
        }

        /// <summary>
        /// Invokes a UnityAction if it is not null.
        /// </summary>
        public static void SafeInvoke(this UnityAction uAction)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke();
        }

        /// <summary>
        /// Invokes a UnityAction with a parameter if it is not null.
        /// </summary>
        public static void SafeInvoke<T>(this UnityAction<T> uAction, T eventParameter)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke(eventParameter);
        }

        /// <summary>
        /// Invokes a UnityAction with two parameters if it is not null.
        /// </summary>
        public static void SafeInvoke<T1, T2>(this UnityAction<T1, T2> uAction, T1 eventParameter1, T2 eventParameter2)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke(eventParameter1, eventParameter2);
        }
    
        /// <summary>
        /// Invokes an Action with two parameters if it is not null.
        /// </summary>
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 param1, T2 param2)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke(param1, param2);
        }

        /// <summary>
        /// Invokes a UnityEvent if it is not null.
        /// </summary>
        public static void SafeInvoke(this UnityEvent uEvent)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke();
        }

        /// <summary>
        /// Invokes a generic UnityEvent with a parameter if it is not null.
        /// </summary>
        public static void SafeInvoke<T>(this UnityEvent<T> uEvent, T eventParameter)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke(eventParameter);
        }

        /// <summary>
        /// Invokes a UnityEvent with two parameters if it is not null.
        /// </summary>
        public static void SafeInvoke<T1, T2>(this UnityEvent<T1, T2> uEvent, T1 eventParameter1, T2 eventParameter2)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke(eventParameter1, eventParameter2);
        }

        /// <summary>
        /// Invokes an EventHandler if it is not null.
        /// </summary>
        public static void SafeInvoke(this EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null)
            {
                eventHandler.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Unsubscribes and then subscribes an action to a UnityEvent to ensure a single subscription.
        /// </summary>
        public static void SingleSubcribe(this UnityEvent uEvnt, UnityAction uAction)
        {
            if (uEvnt == null || uAction == null)
            {
                return;
            }

            uEvnt.RemoveListener(uAction);
            uEvnt.AddListener(uAction);
        }

        /// <summary>
        /// Unsubscribes and then subscribes a generic action to a UnityEvent to ensure a single subscription.
        /// </summary>
        public static void SingleSubcribe<T>(this UnityEvent<T> uEvnt, UnityAction<T> uAction)
        {
            if (uEvnt == null)
            {
                return;
            }

            uEvnt.RemoveListener(uAction);
            uEvnt.AddListener(uAction);
        }

        /// <summary>
        /// Adds a listener to a UnityEvent that automatically removes itself after one invocation.
        /// </summary>
        public static void AddOneTimeListener(this UnityEvent unityEvent, UnityAction unityAction)
        {
            if (unityEvent == null)
            {
                return;
            }

            UnityAction oneTimeListener = null;
            oneTimeListener = () =>
            {
                unityAction.SafeInvoke();
                unityEvent.RemoveListener(oneTimeListener);
            };
            unityEvent.AddListener(oneTimeListener);
        }
        
        /// <summary>
        /// Resets an EventHandler reference to null if the instance is valid.
        /// </summary>
        public static void SafeUnsubscribe<T>(this object instance, ref EventHandler<T> eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        /// <summary>
        /// Resets an EventHandler reference to null if the instance is valid.
        /// </summary>
        public static void SafeUnsubscribe(this object instance, ref EventHandler eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        /// <summary>
        /// Resets an Action reference to null if the instance is valid.
        /// </summary>
        public static void SafeUnsubscribe<T>(this object instance, ref Action<T> eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        /// <summary>
        /// Resets an Action reference to null if the instance is valid.
        /// </summary>
        public static void SafeUnsubscribe(this object instance, ref Action eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        /// <summary>
        /// Safely attempts to get a component, logging an error if the GameObject is null.
        /// </summary>
        public static TComponent GetComponentSafe<TComponent>(this GameObject @this)
            where TComponent : Component
        {
            if (@this == null)
            {
                Debug.LogError("GameObject is null");
                return default(TComponent);
            }

            return @this.GetComponent<TComponent>();
        }

        /// <summary>
        /// Returns an existing component or adds a new one if it doesn't exist.
        /// </summary>
        public static TComponent GetOrAddComponent<TComponent>(this GameObject @this)
            where TComponent : Component
        {
            return @this.GetComponent<TComponent>().NullCoalesceAssign(@this.AddComponent<TComponent>);
        }

        /// <summary>
        /// Returns an existing component on the component's GameObject or adds a new one.
        /// </summary>
        public static TComponent GetOrAddComponent<TComponent>(this Component @this)
            where TComponent : Component
        {
            return @this.GetComponent<TComponent>().NullCoalesceAssign(@this.gameObject.AddComponent<TComponent>);
        }

        /// <summary>
        /// Returns an existing component of a specific type or adds a new one.
        /// </summary>
        public static Component GetOrAddComponent(this Component @this, Type componentType)
        {
            if (!componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException("componentType", "Requested type is not of type " + typeof(Component).Name);
            }

            return @this.GetComponent(componentType).NullCoalesceAssign(@this.gameObject.AddComponent(componentType));
        }

        /// <summary>
        /// Gets a component that is attached to a child of the component gameobject, guaranteed to not be attached to the component itself.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to get.</typeparam>
        /// <param name="component">This <see cref="Component"/></param>
        /// <param name="includeInactive">Whether to include inactive objects in the search.</param>
        /// <returns>Thr first component found in the children of this component.</returns>
        public static TComponent GetChildComponent<TComponent>(this Component component, bool includeInactive = false)
            where TComponent : Component
        {
            if (component == null)
            {
                return null;
            }

            return component.GetChildComponents<TComponent>(includeInactive).FirstOrDefault();
        }

        /// <summary>
        /// Gets a component that is attached to a child of the component gameobject, guaranteed to not be attached to the component itself.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to get.</typeparam>
        /// <param name="gameObject">This <see cref="GameObject"/></param>
        /// <param name="includeInactive">Whether to include inactive objects in the search.</param>
        /// <returns>The first component found in the children of this component.</returns>
        public static TComponent GetChildComponent<TComponent>(this GameObject gameObject, bool includeInactive = false)
            where TComponent : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            return gameObject.transform.GetChildComponents<TComponent>(includeInactive).FirstOrDefault();
        }

        /// <summary>
        /// Gets the components that are attached to children of the component gameobject, guaranteed to not be attached to the components own gameobject.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to get.</typeparam>
        /// <param name="component">This <see cref="Component"/></param>
        /// <param name="includeInactive">Whether to include inactive objects in the search.</param>
        /// <returns>The first component found in the children of this component.</returns>
        public static IEnumerable<TComponent> GetChildComponents<TComponent>(this Component component, bool includeInactive = false)
            where TComponent : class
        {
            var components = new List<TComponent>();

            if (component == null)
            {
                return null;
            }

            var childCount = component.transform.childCount;

            if (childCount == 0)
            {
                Debug.LogWarning("No children to get components from", component);
                return components;
            }

            var transform = component.transform;
            for (var i = 0; i < childCount; i++)
            {
                components.AddRange(transform.GetChild(i).GetComponentsInChildren<TComponent>(includeInactive));
            }

            return components;
        }

        /// <summary>
        /// Gets the components that are attached to children of the component gameobject, guaranteed to not be attached to the components own gameobject.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to get.</typeparam>
        /// <param name="gameObject">This <see cref="GameObject"/></param>
        /// <param name="includeInactive">Whether to include inactive objects in the search.</param>
        /// <returns>The first component found in the children of this component.</returns>
        public static IEnumerable<TComponent> GetChildComponents<TComponent>(this GameObject gameObject, bool includeInactive = false)
            where TComponent : class
        {
            if (gameObject == null)
            {
                return null;
            }

            return gameObject.transform.GetChildComponents<TComponent>(includeInactive);
        }

        /// <summary>
        /// Assigns a GameObject from a source if the current reference is null.
        /// </summary>
        public static GameObject NullCoalesceAssign(this GameObject gameObject, Func<GameObject> gameObjectSource)
        {
            if (gameObject == null)
            {
                gameObject = gameObjectSource.SafeInvoke();
            }

            return gameObject;
        }

        /// <summary>
        ///  Unity Editor-safe null coalescing operator substitute
        /// </summary>
        /// <typeparam name="TComponent">The type of the component, inferred.</typeparam>
        /// <param name="component">The component variable to null coalesce.</param>
        /// <param name="componentSource">The source of the component in case it needs to be fetched.</param>
        /// <returns>Component variable that has been null coalesced by using a component source </returns>
        public static TComponent NullCoalesceAssign<TComponent>(this TComponent component, Component componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                var potentialComponent = componentSource as TComponent;
                
                if (potentialComponent != null)
                {
                    component = potentialComponent;
                }
                else
                {
                    component = componentSource.GetComponent<TComponent>();
                }
            }

            return component;
        }

        /// <summary>
        /// Assigns a component from another component source if the current reference is null.
        /// </summary>
        public static TComponent NullCoalesceAssign<TComponent, TOtherComponent>(this TComponent component, TOtherComponent componentSource)
            where TComponent : Component
            where TOtherComponent : Component
        {
            if (component == null)
            {
                var potentialComponent = componentSource as TComponent;
                if (potentialComponent != null)
                {
                    component = potentialComponent;
                }
                else
                {
                    component = componentSource.GetComponent<TComponent>();
                }
            }

            return component;
        }

        /// <summary>
        /// Assigns a component from a Func source if the current reference is null.
        /// </summary>
        public static TComponent NullCoalesceAssign<TComponent>(this TComponent component, Func<TComponent> componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                component = componentSource.SafeInvoke();
            }

            return component;
        }

        /// <summary>
        /// Assigns a component from a GameObject source if the current reference is null.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component, inferred.</typeparam>
        /// <param name="component">The component variable to null coalesce.</param>
        /// <param name="componentSource">The source of the component in case it needs to be fetched.</param>
        /// <returns>Component variable that has been null coalesced by using a component source </returns>
        public static TComponent NullCoalesceAssign<TComponent>(this TComponent component, GameObject componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                component = componentSource.GetComponent<TComponent>();
            }

            return component;
        }

        /// <summary>
        /// Returns the existing component or fetches it from a source if null.
        /// </summary>
        public static TComponent NullCoalesce<TComponent>(this TComponent component, Component componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                return componentSource.GetComponent<TComponent>();
            }

            return component;
        }

        /// <summary>
        /// Returns the existing component or fetches it from a Func source if null.
        /// </summary>
        public static TComponent NullCoalesce<TComponent>(this TComponent component, Func<TComponent> componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                return componentSource.SafeInvoke();
            }

            return component;
        }

        /// <summary>
        /// Returns the existing component or fetches it from a GameObject source if null.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component, inferred.</typeparam>
        /// <param name="component">The component variable to null coalesce.</param>
        /// <param name="componentSource">The source of the component in case it needs to be fetched.</param>
        /// <returns>Component variable that has been null coalesced by using a component source </returns>
        public static TComponent NullCoalesce<TComponent>(this TComponent component, GameObject componentSource)
            where TComponent : Component
        {
            if (component == null)
            {
                return componentSource.GetComponent<TComponent>();
            }

            return component;
        }

        /// <summary>
        /// Calculates the number of steps up the parent chain to reach the root.
        /// </summary>
        public static int GetHierarchyDepth(this Transform @this)
        {
            var ancestors = 0;
            var temp = @this;
            while (temp.parent != null)
            {
                temp = temp.parent;
                ancestors++;
            }

            return ancestors;
        }

        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        /// <summary>
        /// Checks if a Component has been destroyed.
        /// </summary>
        /// <param name="component">Component reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this object component)
        {
            return component == null || component.Equals(null);
        }

        /// <summary>
        /// Sets a GameObject to be active in the hierarchy by iterating up the chain of ancestors.
        /// </summary>
        /// <param name="gameObject">This <see cref="GameObject"/></param>
        public static void SetActiveInHierarchy(this GameObject gameObject)
        {
            var transform = gameObject.transform;
            while (!gameObject.activeInHierarchy)
            {
                transform.gameObject.SetActive(true);
                transform = transform.parent;
            }
        }

        /// <summary>
        /// Instantiates a prefab as a child of a parent, maintaining local transform state.
        /// </summary>
        public static GameObject Instantiate(this MonoBehaviour @this, GameObject prefab, GameObject parent)
        {
#if UNITY_5_4_OR_NEWER
            var newTextObject = Object.Instantiate(prefab, parent.transform, false) as GameObject;
#else
                var newTextObject = Object.Instantiate(prefab);
                newTextObject.transform.SetParent(parent.transform, false);
#endif

            return newTextObject;
        }

        /// <summary>
        /// Starts a coroutine that waits for a specific duration before invoking an action.
        /// </summary>
        public static void WaitSecondsThenInvoke(this MonoBehaviour @this, float delaySeconds, Action action)
        {
            @this.StartCoroutine(WaitSecondsThen(delaySeconds, action));
        }

        /// <summary>
        /// Starts a coroutine that executes an action at the end of the current frame.
        /// </summary>
        public static void ExecuteOnEndOfFrame(this MonoBehaviour @this, Action action)
        {
            @this.StartCoroutine(WaitForEndOfFrameThen(action));
        }

        /// <summary>
        /// Starts a coroutine that executes an action after a specified number of frames.
        /// </summary>
        public static void ExecuteAfterFrames(this MonoBehaviour @this, Action action, int frameFuture)
        {
            @this.StartCoroutine(ExecuteOnFrameIndex(action, Time.frameCount + Mathf.Abs(frameFuture)));
        }

        /// <summary>
        /// Coroutine that waits for a specified time and then executes an action.
        /// </summary>
        private static IEnumerator WaitSecondsThen(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action();
        }

        /// <summary>
        /// Coroutine that waits for the end of the frame and then executes an action.
        /// </summary>
        private static IEnumerator WaitForEndOfFrameThen(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        /// <summary>
        /// Coroutine that waits until a specific frame index is reached and then executes an action.
        /// </summary>
        private static IEnumerator ExecuteOnFrameIndex(Action action, int frameIndex)
        {
            while (Time.frameCount < frameIndex)
            {
                yield return null;
            }

            action();
        }

        /// <summary>
        /// Clamps both x and y values between 0 and 1.
        /// </summary>
        public static Vector2 Clamp01(this Vector2 vector2)
        {
            return new Vector2(Mathf.Clamp01(vector2.x), Mathf.Clamp01(vector2.y));
        }

        /// <summary>
        /// Clamps both x and y values between a minimum float and maximum float value.
        /// </summary>
        public static Vector2 Clamp(this Vector2 vector2, float min, float max)
        {
            return new Vector2(Mathf.Clamp(vector2.x, min, max), Mathf.Clamp(vector2.y, min, max));
        }

        /// <summary>
        /// Clamps both x and y values to a clipping rect.
        /// </summary>
        public static Vector2 Clamp(this Vector2 vector2, Rect rect)
        {
            return new Vector2(Mathf.Clamp(vector2.x, rect.xMin, rect.xMax), Mathf.Clamp(vector2.y, rect.yMin, rect.yMax));
        }

        /// <summary>
        /// Clamps both x, y and z values between 0 and 1.
        /// </summary>
        public static Vector3 Clamp01(this Vector3 vector3)
        {
            return new Vector3(Mathf.Clamp01(vector3.x), Mathf.Clamp01(vector3.y), Mathf.Clamp01(vector3.z));
        }

        /// <summary>
        /// Clamps both x, y and z values between a minimum float and maximum float value.
        /// </summary>
        public static Vector3 Clamp(this Vector3 vector3, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(vector3.x, min, max),
                Mathf.Clamp(vector3.y, min, max),
                Mathf.Clamp(vector3.z, min, max));
        }

        /// <summary>
        /// Checks if a list is either null or has no elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsDefault<T>(this T value)
            where T : struct
        {
            return value.Equals(default(T));
        }

        public static bool IsEven(this int integer)
        {
            return integer % 2 == 0;
        }

        public static bool IsOdd(this int integer)
        {
            return !integer.IsEven();
        }

        public static bool IsAnyOf<T>(this T value, params T[] values)
            where T : struct, IComparable
        {
            return values.Any(v => EqualityComparer<T>.Default.Equals(value, v));
        }

        /// <summary>
        /// Compares two floats for approximate equality using an epsilon.
        /// </summary>
        public static bool Equals(this float floatValue, float other)
        {
            return NearlyEqual(floatValue, other, 0.0000001f);
        }

        /// <summary>
        /// Compares two doubles for approximate equality using an epsilon.
        /// </summary>
        public static bool Equals(this double doubleValue, double other)
        {
            return NearlyEqual(doubleValue, other, 0.0000001);
        }

        /// <summary>
        /// Checks if two floats are nearly equal based on relative error.
        /// </summary>
        private static bool NearlyEqual(float a, float b, float epsilon)
        {
            // http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == b)
            {
                // shortcut, handles infinities
                return true;
            }

            if (a == 0 || b == 0 || diff < float.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.Epsilon);
            }

            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        /// <summary>
        /// Checks if two doubles are nearly equal based on relative error.
        /// </summary>
        private static bool NearlyEqual(double a, double b, double epsilon)
        {
            // http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }

            if (a == 0 || b == 0 || diff < double.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * double.Epsilon);
            }

            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        public static int LastIndex<T>(this ICollection<T> list)
        {
            return list.Count - 1;
        }

        public static int LastIndex<T>(this T[] array)
        {
            return array.Length - 1;
        }

        /// <summary>
        /// Removes the last element from a collection if it contains elements.
        /// </summary>
        public static void RemoveLast<T>(this ICollection<T> list)
        {
            if (list.Count > 0)
            {
                list.Remove(list.Last());
            }
        }

        /// <summary>
        ///  A no op method, designed to fake access to properties that trigger code
        ///  that must be run. This is called "coding to implementation" (as opposed to coding to interface)
        ///  and should be avoided if possible.
        /// </summary>
        /// <param name="obj">The object to access but do nothing with it.</param>
        public static void NoOp(this object obj)
        {
        }

        /// <summary>
        /// Return signed angle between vectors on a plane, where left oriented angles are negative.
        /// </summary>
        /// <param name="this">The "from" vector</param>
        /// <param name="other">The "to" vector</param>
        /// <param name="up">Defines what is "up", this is used to determine positive/negative angle values.</param>
        public static float AngleSigned(this Vector3 @this, Vector3 other, Vector3 up)
        {
            return Mathf.Atan2(Vector3.Dot(up, Vector3.Cross(@this, other)), Vector3.Dot(@this, other)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Clamps the pitch of a Quaternion to a given limit
        /// </summary>
        /// <param name="lookAngle">The quaternion to clamp</param>
        /// <param name="angleLimit">The pitch limit (angle-arc-distance from the horizon)</param>
        /// <returns>A pitch-clamped quaternion</returns>
        public static Quaternion ClampPitch(this Quaternion lookAngle, float angleLimit)
        {
            var eulerLookAngles = lookAngle.eulerAngles;
            var currentPitchAngle = eulerLookAngles.x;

            if (angleLimit < currentPitchAngle && currentPitchAngle < 90f + angleLimit)
            {
                lookAngle.eulerAngles = new Vector3(angleLimit, eulerLookAngles.y, eulerLookAngles.z);
            }
            else if (180f + angleLimit < currentPitchAngle && currentPitchAngle < 360 - angleLimit)
            {
                lookAngle.eulerAngles = new Vector3(-angleLimit, eulerLookAngles.y, eulerLookAngles.z);
            }

            return lookAngle;
        }

        /// <summary>
        /// Attempts to parse a string into an enum value, returning a default value if it fails.
        /// </summary>
        public static bool TryParseToEnum<TEnum>(string strEnumValue, out TEnum enumValue, TEnum defaultValue = default(TEnum))
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                enumValue = defaultValue;
                return false;
            }

            enumValue = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
            return true;
        }
        
        /// <summary>
        /// Subscribes a listener to an Action that will unsubscribe itself after one execution.
        /// </summary>
        public static void ListenOnce(Action action, Action listener)
        {
            void Wrapper()
            {
                action -= Wrapper;
                listener?.Invoke();
            }
            action += Wrapper;
        }
        
        /// <summary>
        /// Subscribes a generic listener to an Action that will unsubscribe itself after one execution.
        /// </summary>
        public static Action<T> ListenOnce<T>(Action<T> action, Action<T> listener)
        {
            void Wrapper(T param)
            {
                // Deregister itself after invocation
                action -= Wrapper;

                // Call the listener
                listener?.Invoke(param);
            }

            // Return the updated action
            return action + Wrapper;
        }

        /// <summary>
        /// Recursively searches for a child transform with the specified name.
        /// </summary>
        public static Transform FindChildByName(this Transform parent, string childName)
        {
            if (parent.name == childName)
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                Transform found = child.FindChildByName(childName);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// http://stackoverflow.com/a/4108907/5048754
        /// </summary>
        /// <param name="this">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        public static bool HasFlag(this Enum @this, Enum value)
        {
            if (@this == null)
            {
                return false;
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var enumType = @this.GetType();

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(enumType, value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(),
                    @this.GetType()));
            }

            var underlyingType = Enum.GetUnderlyingType(enumType);

            if (underlyingType == typeof(short))
            {
                var num = Convert.ToInt16(value);
                if (num == -1)
                {
                    // Unity adds an EVERYTHING field in EnumMaskField inspectors, which sets all bits to 1 (Two's Complement)
                    return true;
                }

                if (num == 0)
                {
                    // Unity adds a NONE field in EnumMaskField inspectors, which sets all bits to 0.
                    return false;
                }

                return (Convert.ToInt16(@this) & num) == num;
            }

            if (underlyingType == typeof(int))
            {
                var num = Convert.ToInt32(value);
                if (num == -1)
                {
                    // Unity adds an EVERYTHING field in EnumMaskField inspectors, which sets all bits to 1 (Two's Complement)
                    return true;
                }

                if (num == 0)
                {
                    // Unity adds a NONE field in EnumMaskField inspectors, which sets all bits to 0.
                    return false;
                }

                return (Convert.ToInt32(@this) & num) == num;
            }

            if (underlyingType == typeof(long))
            {
                var num = Convert.ToInt64(value);
                if (num == -1)
                {
                    // Unity adds an EVERYTHING field in EnumMaskField inspectors, which sets all bits to 1 (Two's Complement)
                    return true;
                }

                if (num == 0)
                {
                    // Unity adds a NONE field in EnumMaskField inspectors, which sets all bits to 0.
                    return false;
                }

                return (Convert.ToInt64(@this) & num) == num;
            }

            throw new ArgumentOutOfRangeException("this", @this, "Type " + underlyingType.Name + " is not supported.");
        }

        /// <summary>
        /// Converts a time span into a string representation of the form mm:ss
        /// </summary>
        /// <param name="timeSpan">The timespan to convert.</param>
        /// <returns>Formatted string</returns>
        public static string ToMinutesSecondsString(this TimeSpan timeSpan)
        {
            return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string ToHoursMinutesSecondsString(this TimeSpan timeSpan)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Adds multiple field entries to a WWWForm from a dictionary.
        /// </summary>
        public static void AddFields(this WWWForm form, Dictionary<string, object> keyValuePairs)
        {
            form.AddFields(keyValuePairs, fieldEntry => fieldEntry.Key, fieldEntry => fieldEntry.Value.ToString());
        }

        /// <summary>
        /// Adds multiple field entries to a WWWForm using provided selectors.
        /// </summary>
        public static void AddFields<T>(this WWWForm form, IEnumerable<T> elements, Func<T, string> fieldNameSelector, Func<T, string> fieldValueSelector)
        {
            foreach (var element in elements)
            {
                form.AddField(fieldNameSelector(element), fieldValueSelector(element));
            }
        }

        /// <summary>
        /// Adds multiple field entries to a WWWForm using provided selectors.
        /// </summary>
        public static void AddFields<T>(this WWWForm form, IEnumerable<T> elements, Func<T, string> fieldNameSelector, Func<T, int> fieldValueSelector)
        {
            foreach (var element in elements)
            {
                form.AddField(fieldNameSelector(element), fieldValueSelector(element));
            }
        }

        /// <summary>
        /// Searches through loaded assemblies to find a specific type by name.
        /// </summary>
        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            
            if (type != null)
            {
                return type;
            }

            if (typeName.Contains("."))
            {
                var assemblyName = typeName.Substring(0, typeName.IndexOf('.'));
                var assembly = Assembly.Load(assemblyName);
                
                if (assembly != null)
                {
                    type = assembly.GetType(typeName);
                    if (type != null) return type;
                }
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                // Load the referenced assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    continue;
                }

                // See if that assembly defines the named type
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            // The type just couldn't be found...
            return null;
        }

        public static bool IsInRange(this float @this, float minInclusive, float maxInclusive)
        {
            return @this.IsInRangeCompare(minInclusive, maxInclusive) == 0;
        }

        public static int IsInRangeCompare(this float @this, float minExclusive, float maxExclusive)
        {
            if (minExclusive <= @this && @this <= maxExclusive)
            {
                return 0;
            }

            if (@this < minExclusive)
            {
                return -1;
            }

            return 1;
        }

        public static bool IsBetween(this float @this, float minExclusive, float maxExclusive)
        {
            return @this.IsBetweenCompare(minExclusive, maxExclusive) == 0;
        }

        public static int IsBetweenCompare(this float @this, float a, float b)
        {
            if (a > b)
            {
                if (a > @this && @this > b)
                {
                    return 0;
                }

                if (@this < b)
                {
                    return -1;
                }
            }
            else if (a < b)
            {
                if (a < @this && @this < b)
                {
                    return 0;
                }

                if (@this < a)
                {
                    return -1;
                }
            }
            else
            {
                return int.MaxValue;
            }

            return 1;
        }

        /// <summary>
        /// WHAT'S IN THE BOX?! Checks if a world space point is inside of a specified <see cref="BoxCollider"/>.
        /// </summary>
        /// <param name="point">World point that might be inside the box.</param>
        /// <param name="box">The box that might contain the specified world point.</param>
        /// <returns>Whether or not the point is in the box.</returns>
        public static bool IsPointInBox(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point) - box.center;

            var halfX = box.size.x * 0.5f;
            var halfY = box.size.y * 0.5f;
            var halfZ = box.size.z * 0.5f;
            return point.x < halfX && point.x > -halfX &&
                   point.y < halfY && point.y > -halfY &&
                   point.z < halfZ && point.z > -halfZ;
        }

        public static Vector2 NormalizedToPositionUnclamped(this Rect rect, Vector2 normalizedPosition)
        {
            return new Vector2(
                Mathf.LerpUnclamped(rect.min.x, rect.max.x, normalizedPosition.x),
                Mathf.LerpUnclamped(rect.min.y, rect.max.y, normalizedPosition.y));
        }

        public static Vector2 PointToNormalizedUnclamped(this Rect rect, Vector2 point)
        {
            return new Vector2(InverseLerpUnclamped(rect.x, rect.xMax, point.x), InverseLerpUnclamped(rect.y, rect.yMax, point.y));
        }

        private static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (Math.Abs(a - b) > 0.00001f)
            {
                return (value - a) / (b - a);
            }

            return 0.0f;
        }

        public static Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angleDegrees)
        {
            var angleInRadians = angleDegrees * Mathf.Deg2Rad;
            var cosTheta = Mathf.Cos(angleInRadians);
            var sinTheta = Mathf.Sin(angleInRadians);
            return new Vector2(
                (cosTheta * (point.x - pivot.x)) - (sinTheta * (point.y - pivot.y)) + pivot.x,
                (sinTheta * (point.x - pivot.x)) + (cosTheta * (point.y - pivot.y)) + pivot.y);
        }

        public static Vector3[] ToPoints(this Rect @this)
        {
            return new Vector3[]
            {
                @this.position,
                @this.position + new Vector2(0f, @this.height),
                @this.position + new Vector2(@this.width, @this.height),
                @this.position + new Vector2(@this.width, 0f)
            };
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
