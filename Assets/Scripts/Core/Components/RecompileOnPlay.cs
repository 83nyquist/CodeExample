using UnityEditor;

namespace Core.Components
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
                // Force Unity to recompile before entering Play Mode
                AssetDatabase.Refresh();
            }
        }
    }
}