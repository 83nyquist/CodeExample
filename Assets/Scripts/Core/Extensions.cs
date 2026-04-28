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
        #region Conditional Accessor

        public static void IfNotNull<T>(this T subject, Action<T> method)
            where T : class
        {
            if (IsNull(subject) || method == null)
            {
                return;
            }

            var subjectType = typeof(T);

            // Support nullable types
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

                // ReSharper disable HeuristicUnreachableCode
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (s == null)
                {
                    return;
                }

                // ReSharper restore HeuristicUnreachableCode
            }

            method.Invoke(subject);
        }

        // also does chaining
        public static TR IfNotNull<T, TR>(this T subject, Func<T, TR> method)
            where T : class
        {
            if (IsNull(subject) || method == null)
            {
                return default(TR);
            }

            var subjectType = typeof(T);

            // Support nullable types
            if (subjectType.IsValueType)
            {
                if (Nullable.GetUnderlyingType(subjectType) == null)
                {
                    return default(TR);
                }
            }

            return method.Invoke(subject);
        }

        private static bool IsNull<T>(T obj)
            where T : class
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }

        #endregion

        #region Delegate/EventHandler Extensions

        public static T SafeInvoke<T>(this Func<T> funcT)
        {
            return funcT == null ? default(T) : funcT.Invoke();
        }

        public static TR SafeInvoke<T, TR>(this Func<T, TR> funcT, T input)
        {
            return funcT == null ? default(TR) : funcT.Invoke(input);
        }

        public static void SafeInvoke(this Action action)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke();
        }

        public static void SafeInvoke<T>(this Action<T> action, T argument)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke(argument);
        }

        public static void SafeInvoke(this UnityAction uAction)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke();
        }

        public static void SafeInvoke<T>(this UnityAction<T> uAction, T eventParameter)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke(eventParameter);
        }

        public static void SafeInvoke<T1, T2>(this UnityAction<T1, T2> uAction, T1 eventParameter1, T2 eventParameter2)
        {
            if (uAction == null)
            {
                return;
            }

            uAction.Invoke(eventParameter1, eventParameter2);
        }
    
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 param1, T2 param2)
        {
            if (action == null)
            {
                return;
            }

            action.Invoke(param1, param2);
        }

        public static void SafeInvoke(this UnityEvent uEvent)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke();
        }

        public static void SafeInvoke<T>(this UnityEvent<T> uEvent, T eventParameter)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke(eventParameter);
        }

        public static void SafeInvoke<T1, T2>(this UnityEvent<T1, T2> uEvent, T1 eventParameter1, T2 eventParameter2)
        {
            if (uEvent == null)
            {
                return;
            }

            uEvent.Invoke(eventParameter1, eventParameter2);
        }

        public static void SafeInvoke(this EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler != null)
            {
                eventHandler.Invoke(sender, eventArgs);
            }
        }

        public static void SingleSubcribe(this UnityEvent uEvnt, UnityAction uAction)
        {
            if (uEvnt == null || uAction == null)
            {
                return;
            }

            uEvnt.RemoveListener(uAction);
            uEvnt.AddListener(uAction);
        }

        public static void SingleSubcribe<T>(this UnityEvent<T> uEvnt, UnityAction<T> uAction)
        {
            if (uEvnt == null)
            {
                return;
            }

            uEvnt.RemoveListener(uAction);
            uEvnt.AddListener(uAction);
        }

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
        
        public static void SafeUnsubscribe<T>(this object instance, ref EventHandler<T> eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        public static void SafeUnsubscribe(this object instance, ref EventHandler eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        public static void SafeUnsubscribe<T>(this object instance, ref Action<T> eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }

        public static void SafeUnsubscribe(this object instance, ref Action eventHandler)
        {
            if (instance != null && eventHandler != null)
            {
                eventHandler = null;
            }
        }


        #endregion / Delegate/EventHandler Extensions

        #region Component and GameObject Extensions

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

        public static TComponent GetOrAddComponent<TComponent>(this GameObject @this)
            where TComponent : Component
        {
            return @this.GetComponent<TComponent>().NullCoalesceAssign(@this.AddComponent<TComponent>);
        }

        public static TComponent GetOrAddComponent<TComponent>(this Component @this)
            where TComponent : Component
        {
            return @this.GetComponent<TComponent>().NullCoalesceAssign(@this.gameObject.AddComponent<TComponent>);
        }

        public static Component GetOrAddComponent(this Component @this, Type componentType)
        {
            if (!componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException("componentType", "Requested type is not of type " + typeof(Component).Name);
            }

            return @this.GetComponent(componentType).NullCoalesceAssign(@this.gameObject.AddComponent(componentType));
        }

        //public static TComponent GetOrAddComponentWithDependencies<TComponent>(this Component @this)
        //    where TComponent : Component
        //{
        //    var component = @this.GetComponent<TComponent>();

        //    if (component != null)
        //    {
        //        return component;
        //    }

        //    var dependencyGraph = BuildTypeDependencyGraph<TComponent>();

        //    Debug.Log("Has children?" + dependencyGraph.HasChildren);

        //    // Add components from the leaf-ends, this ensures dependencies are added in the correct order
        //    while (dependencyGraph.HasChildren)
        //    {
        //        var leafNodes = dependencyGraph.GetLeafNodes();

        //        foreach (var leafNode in leafNodes)
        //        {
        //            if (@this.GetComponent(leafNode.Value) == null)
        //            {
        //                @this.gameObject.AddComponent(leafNode.Value);
        //            }

        //            dependencyGraph.RemoveNode(leafNode);
        //        }
        //    }

        //    return @this.gameObject.AddComponent<TComponent>();
        //}

        //private static GraphNode<Type> BuildTypeDependencyGraph<TComponent>()
        //    where TComponent : Component
        //{
        //    Debug.Log("Building Dependency Graph For Type: " + typeof(TComponent));
        //    return BuildTypeDependencyGraph(typeof(TComponent), new GraphNode<Type>(typeof(TComponent), null));
        //}

        //private static GraphNode<Type> BuildTypeDependencyGraph(Type componentType, GraphNode<Type> currentParent)
        //{
        //    var requireComponentAttributes = componentType.GetCustomAttributes(typeof(RequireComponent), true);
        //    Debug.Log("Attribute Count: " + requireComponentAttributes.Length);

        //    var typedRequiredAttributes = requireComponentAttributes.OfType<RequireComponent>().ToList();
        //    Debug.Log("Typed Attribute Count: " + typedRequiredAttributes.Count);

        //    var actualRequirementTypes = typedRequiredAttributes.SelectMany(cr => new[] { cr.m_Type0, cr.m_Type1, cr.m_Type2 }).Where(t => t != null)
        //        .Distinct().ToList();

        //    Debug.Log("Actual Types Count: " + actualRequirementTypes.Count);

        //    foreach (var actualRequirementType in actualRequirementTypes)
        //    {
        //        Debug.Log("Type: " + actualRequirementType);
        //    }

        //    var componentRequirements = actualRequirementTypes;

        //    foreach (var componentRequirement in componentRequirements)
        //    {
        //        if (!currentParent.Root.ContainsValue(componentRequirement))
        //        {
        //            BuildTypeDependencyGraph(componentRequirement, new GraphNode<Type>(componentRequirement, currentParent));
        //        }
        //        else
        //        {
        //            if (currentParent.Ancestors.Any(gn => gn.Value == componentRequirement))
        //            {
        //                Debug.LogError("Circular dependency detected: " + componentType + " <-> " + componentRequirement);
        //            }
        //            else
        //            {
        //                // Move already built graph to deeper position in tree
        //                currentParent.Root.GetNodeWithValue(componentRequirement).Parent = currentParent;
        //            }
        //        }
        //    }

        //    return currentParent;
        //}

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

        //public static TComponent GetParentComponent<TComponent>(this GameObject gameObject, bool includeInactive = false)
        //    where TComponent : class // Cannot constrain to Component if we want to support interfaces
        //{
        //    if (gameObject == null)
        //    {
        //        return null;
        //    }

        //    // The unity built-in method actually searches itself as well
        //    return gameObject.transform.GetParentComponent<TComponent>(includeInactive);
        //}

        //public static TComponent GetParentComponent<TComponent>(this Component component, bool includeInactive = false)
        //    where TComponent : class // Cannot constrain to Component if we want to support interfaces
        //{
        //    if (component == null)
        //    {
        //        return null;
        //    }

        //    // The built-in method actually searches the object itself as well, thats why we need to search from the parent
        //    return component.transform.GetParentComponents<TComponent>(includeInactive).FirstOrDefault();
        //}

        //public static IEnumerable<TComponent> GetParentComponents<TComponent>(this Component component, bool includeInactive = false)
        //    where TComponent : class
        //{
        //    if (component.transform.IsRoot())
        //    {
        //        Debug.LogError("Object is root object and has no parents to get components from", component);
        //        return null;
        //    }

        //    return component.transform.parent.GetComponentsInParent<TComponent>(includeInactive);
        //}

        //public static IEnumerable<TComponent> GetParentComponents<TComponent>(this GameObject gameObject, bool includeInactive = false)
        //    where TComponent : class // Cannot constrain to Component if we want to support interfaces
        //{
        //    if (gameObject == null)
        //    {
        //        return null;
        //    }

        //    return gameObject.transform.GetParentComponents<TComponent>(includeInactive);
        //}

        public static GameObject NullCoalesceAssign(this GameObject gameObject, Func<GameObject> gameObjectSource)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
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
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator (??) does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build)
            // This is why resharper is complaining... a lot.

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression

            // If the object has been "destroyed" but is not actually null, null coalescing will not work as intended because it compares actual references
            if (component == null)
            {
                // Check if it really is null
                ////if (!ReferenceEquals(component, null)) // we dont need to, because we would want to assign it anways
                ////{
                var potentialComponent = componentSource as TComponent;
                if (potentialComponent != null)
                {
                    component = potentialComponent;
                }
                else
                {
                    component = componentSource.GetComponent<TComponent>();
                }

                ////}
            }

            // ReSharper restore HeuristicUnreachableCode
            return component;
        }

        public static TComponent NullCoalesceAssign<TComponent, TOtherComponent>(this TComponent component, TOtherComponent componentSource)
            where TComponent : Component
            where TOtherComponent : Component
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
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

            // ReSharper restore HeuristicUnreachableCode
            return component;
        }

        public static TComponent NullCoalesceAssign<TComponent>(this TComponent component, Func<TComponent> componentSource)
            where TComponent : Component
        {
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build
            // This is why resharper is complaining... a lot.
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression

            // If the object has been "destroyed" but is not actually null, null coalescing will not work as intended because it compares actual references
            if (component == null)
            {
                // Check if it really is null
                ////if (!ReferenceEquals(component, null)) // we dont need to, because we would want to assign it anways
                ////{
                component = componentSource.SafeInvoke();
                ////}
            }

            // ReSharper restore HeuristicUnreachableCode
            return component;
        }

        /// <summary>
        ///  Unity Editor-safe null coalescing operator subsitute, takes another gameobject as parameter to fetch the component from.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component, inferred.</typeparam>
        /// <param name="component">The component variable to null coalesce.</param>
        /// <param name="componentSource">The source of the component in case it needs to be fetched.</param>
        /// <returns>Component variable that has been null coalesced by using a component source </returns>
        public static TComponent NullCoalesceAssign<TComponent>(this TComponent component, GameObject componentSource)
            where TComponent : Component
        {
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build)
            // This is why resharper is complaining... a lot.
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (component == null)
            {
                // ReSharper disable HeuristicUnreachableCode
                // Check if it really is null
                ////if (!ReferenceEquals(component, null)) // we dont need to, because we would want to assign it anways
                ////{
                component = componentSource.GetComponent<TComponent>();
                ////}
            }

            // ReSharper restore HeuristicUnreachableCode
            return component;
        }

        public static TComponent NullCoalesce<TComponent>(this TComponent component, Component componentSource)
            where TComponent : Component
        {
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator (??) does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build)
            // This is why resharper is complaining... a lot.

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // ReSharper disable once ConvertIfStatementToReturnStatement

            // If the object has been "destroyed" but is not actually null, null coalescing will not work as intended because it compares actual references
            if (component == null)
            {
                return componentSource.GetComponent<TComponent>();
            }

            return component;
        }

        public static TComponent NullCoalesce<TComponent>(this TComponent component, Func<TComponent> componentSource)
            where TComponent : Component
        {
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build
            // This is why resharper is complaining... a lot.
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // ReSharper disable once ConvertIfStatementToReturnStatement

            // If the object has been "destroyed" but is not actually null, null coalescing will not work as intended because it compares actual references
            if (component == null)
            {
                return componentSource.SafeInvoke();
            }

            // ReSharper restore HeuristicUnreachableCode
            return component;
        }

        /// <summary>
        ///  Unity Editor-safe null coalescing operator subsitute, takes another gameobject as parameter to fetch the component from.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component, inferred.</typeparam>
        /// <param name="component">The component variable to null coalesce.</param>
        /// <param name="componentSource">The source of the component in case it needs to be fetched.</param>
        /// <returns>Component variable that has been null coalesced by using a component source </returns>
        public static TComponent NullCoalesce<TComponent>(this TComponent component, GameObject componentSource)
            where TComponent : Component
        {
            // Because UnityEngine.Object overloads the != and == operators, see link
            // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // http://forum.unity3d.com/threads/fun-with-null.148090/
            // http://forum.unity3d.com/threads/null-check-inconsistency-c.220649/
            // the null coalescing operator does not work as expected in some cases in the editor
            // (But should work fine in any code that is in a build)
            // This is why resharper is complaining... a lot.
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (component == null)
            {
                return componentSource.GetComponent<TComponent>();
            }

            return component;
        }

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
            // UnityEngine overloads the == operator for the GameObject type
            // and returns null when the object has been destroyed, but
            // actually the object might still be there but has not been cleaned up yet

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return gameObject == null && !ReferenceEquals(gameObject, null);

            // PS. Hans has sqeaky shoes.
        }

        /// <summary>
        /// Checks if a Component has been destroyed.
        /// </summary>
        /// <param name="component">Component reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this object component)
        {
            // UnityEngine overloads the == opeator for the UnityEngine.Object/GameObject type
            // and returns null when the object has been destroyed, but
            // actually the object is still there but has not been cleaned up yet

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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

        #endregion /GameObject Extensions

        #region MonoBehavior Extensions

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

        public static void WaitSecondsThenInvoke(this MonoBehaviour @this, float delaySeconds, Action action)
        {
            @this.StartCoroutine(WaitSecondsThen(delaySeconds, action));
        }

        public static void ExecuteOnEndOfFrame(this MonoBehaviour @this, Action action)
        {
            @this.StartCoroutine(WaitForEndOfFrameThen(action));
        }

        public static void ExecuteAfterFrames(this MonoBehaviour @this, Action action, int frameFuture)
        {
            @this.StartCoroutine(ExecuteOnFrameIndex(action, Time.frameCount + Mathf.Abs(frameFuture)));
        }

        private static IEnumerator WaitSecondsThen(float waitTime, Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action();
        }

        private static IEnumerator WaitForEndOfFrameThen(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        private static IEnumerator ExecuteOnFrameIndex(Action action, int frameIndex)
        {
            while (Time.frameCount < frameIndex)
            {
                yield return null;
            }

            action();
        }

        #endregion

        #region Vector extensions

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

        #endregion

        #region Readabilty

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

        public static bool Equals(this float floatValue, float other)
        {
            return NearlyEqual(floatValue, other, 0.0000001f);
        }

        public static bool Equals(this double doubleValue, double other)
        {
            return NearlyEqual(doubleValue, other, 0.0000001);
        }

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

        public static void RemoveLast<T>(this ICollection<T> list)
        {
            if (list.Count > 0)
            {
                list.Remove(list.Last());
            }
        }

        #endregion

        #region Bug Workarounds

        /// <summary>
        ///  A no op method, designed to fake access to properties that trigger code
        ///  that must be run. This is called "coding to implementation" (as opposed to coding to interface)
        ///  and should be avoided if possible.
        /// </summary>
        /// <param name="obj">The object to access but do nothing with it.</param>
        public static void NoOp(this object obj)
        {
            /*
        [http://docs.unity3d.com/Manual/DownloadingAssetBundles.html]
         *
            When you access the .assetBundle property, the downloaded data is extracted and the AssetBundle object is created.

            At this point, you are ready to load the objects contained in the bundle.
            The second parameter passed to LoadFromCacheOrDownload specifies which version of the AssetBundle to download.
            If the AssetBundle doesn't exist in the cache or has a version lower than requested, LoadFromCacheOrDownload will download the AssetBundle.
            Otherwise the AssetBundle will be loaded from cache.

            Please note that only up to one AssetBundle download can finish per frame when they are downloaded with WWW.LoadFromCacheOrDownload.
         */
        }

        #endregion /Bug Workarounds

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
        
        public static void ListenOnce(Action action, Action listener)
        {
            // Wrapped listener to automatically deregister itself
            void Wrapper()
            {
                // Unsubscribe first to ensure this is only called once
                action -= Wrapper;

                // Invoke the original listener
                listener?.Invoke();
            }

            // Subscribe the wrapped listener
            action += Wrapper;
        }
        
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

        public static Transform FindChildByName(this Transform parent, string childName)
        {
            // Base case: Check if current transform name matches
            if (parent.name == childName)
            {
                return parent;
            }

            // Recursively search all children
            foreach (Transform child in parent)
            {
                Transform found = child.FindChildByName(childName);
                if (found != null)
                {
                    return found;
                }
            }

            // Return null if no match is found
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

        // public static bool Successful(this WWW request)
        // {
        //     return string.IsNullOrEmpty(request.error) && request.text.Contains("success");
        // }

        public static void AddFields(this WWWForm form, Dictionary<string, object> keyValuePairs)
        {
            form.AddFields(keyValuePairs, fieldEntry => fieldEntry.Key, fieldEntry => fieldEntry.Value.ToString());
        }

        public static void AddFields<T>(this WWWForm form, IEnumerable<T> elements, Func<T, string> fieldNameSelector, Func<T, string> fieldValueSelector)
        {
            foreach (var element in elements)
            {
                form.AddField(fieldNameSelector(element), fieldValueSelector(element));
            }
        }

        public static void AddFields<T>(this WWWForm form, IEnumerable<T> elements, Func<T, string> fieldNameSelector, Func<T, int> fieldValueSelector)
        {
            foreach (var element in elements)
            {
                form.AddField(fieldNameSelector(element), fieldValueSelector(element));
            }
        }

        public static Type GetType(string typeName)
        {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(typeName);

            // If it worked, then we're done here
            if (type != null)
            {
                return type;
            }

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if (typeName.Contains("."))
            {
                // Get the name of the assembly (Assumption is that we are using
                // fully-qualified type names)
                var assemblyName = typeName.Substring(0, typeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    return null;
                }

                // Ask that assembly to return the proper Type
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            // If we still haven't found the proper type, we can enumerate all of the
            // loaded assemblies and see if any of them define the type
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
