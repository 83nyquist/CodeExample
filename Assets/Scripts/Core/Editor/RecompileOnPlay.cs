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
        /// <summary>
        /// Refreshes the asset database when the editor is about to exit edit mode to ensure code is recompiled.
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                AssetDatabase.Refresh();
            }
        }
    }
}