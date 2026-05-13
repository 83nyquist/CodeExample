#if UNITY_EDITOR
using UnityEditor;

namespace Core.Editor
{
    [InitializeOnLoad]
    public static class RecompileOnPlay
    {
        static RecompileOnPlay()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif