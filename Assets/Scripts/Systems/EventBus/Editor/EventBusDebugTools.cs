using UnityEditor;
using UnityEngine;

using Systems.EventBus.Enums;

namespace Systems.EventBus
{
    public class EventBusDebugTools : EditorWindow
    {
        [MenuItem("Tools/Event Bus/Set All Subscribers Log Level")]
        static void ShowWindow()
        {
            var window = GetWindow<EventBusDebugTools>();
            window.titleContent = new GUIContent("Event Bus Logging");
            window.Show();
        }
    
        void OnGUI()
        {
            GUILayout.Label("Set Log Level for All EventBusSubscribers", EditorStyles.boldLabel);
        
            if (GUILayout.Button("Set to None"))
                SetAllLogLevel(EventBusLogLevel.None);
            
            if (GUILayout.Button("Set to Warning"))
                SetAllLogLevel(EventBusLogLevel.Warning);
            
            if (GUILayout.Button("Set to Verbose"))
                SetAllLogLevel(EventBusLogLevel.Verbose);
        }
    
        void SetAllLogLevel(EventBusLogLevel level)
        {
            var subscribers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var sub in subscribers)
            {
                var field = sub.GetType().GetField("_logLevel", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(EventBusLogLevel))
                {
                    field.SetValue(sub, level);
                    EditorUtility.SetDirty(sub);
                }
            }
            Debug.Log($"Set all EventBusSubscribers to {level}");
        }
    }
}