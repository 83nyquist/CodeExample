using UnityEngine;

namespace Core.Components
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        /*
    CAVEATS
        There are a few important issues to be aware of with this approach to creating singletons in Unity:

    LEAKING SINGLETON OBJECTS
        If a MonoBehaviour references a singleton during its OnDestroy or OnDisable while running 
        in the editor, the singleton object that was instantiated at runtime will leak into the scene when playback is stopped. 
        OnDestroy and OnDisable are called by Unity when cleaning up the scene in an attempt to return the scene 
        to its pre-playmode state. If a singleton object is destroyed before another scripts references it 
        through its Instance property, the singleton object will be re-instantiated after Unity expected it 
        to have been permanently destroyed. Unity will warn you of this in very clear language, so keep an eye out for it. 
        One possible solution is to set a boolean flag during OnApplicationQuit that is used to conditionally 
        bypass all singleton references included in OnDestroy and OnDisable.

    EXECUTION ORDER
        The order in which objects have their Awake and Start methods called is not predictable by default. 
        Persistent singletons are especially susceptible to execution ordering issues. 
        If multiple copies of a singleton exist in the scene, one may destroy the other copies after those copies 
        have had their Awake methods called. If game state is changed during Awake, this may cause unexpected behavior. 
        As a general rule, Awake should only ever be used to set up the internal state of an object. 
        Any external object communication should occur during Start. Persistent singletons require strict use of this convention.
    */

        public bool persistent = false;
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (persistent)
            {
                DontDestroyOnLoad(this.gameObject);
                if (_instance == null)
                {
                    _instance = this as T;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}