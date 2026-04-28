using UnityEditor;
using UnityEngine;

namespace Core.Systems.BezierInterpolator.Editor
{
    [CustomEditor(typeof(BezierController))]
    [CanEditMultipleObjects]
    public class BezierControllerEditor : UnityEditor.Editor
    {
        private BezierController _script;

        public BezierController Script
        {
            get
            {
                if (_script == null)
                {
                    _script = target as BezierController;
                }

                return _script;
            }
        }

        void OnEnable()
        {
            if (Script.Speed == null)
            {
                Script.Speed = AnimationCurve.Linear(0, 1, 1, 1);
            }

            if (Script.Scale == null)
            {
                Script.Scale = AnimationCurve.Linear(0, 1, 1, 1);
            }

            if (Script.RotationZ == null)
            {
                Script.RotationZ = AnimationCurve.Linear(0, 1, 1, 1);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawRuntimeCommands();
            DrawDefaultInspector();
            DrawCallbackSection();
        }

        public void DrawRuntimeCommands()
        {
            if (Application.isPlaying && GUILayout.Button("Spawn Test"))//, GUILayout.Width(150)))
            {
                Script.Run(Script.StartGameObject);
            }
        }

        public void DrawCallbackSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);
        
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Callback List", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            DrawCallbackMenu();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        public void DrawCallbackMenu()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Callback Menu", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
