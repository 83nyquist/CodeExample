#if UNITY_EDITOR
using System.Collections.Generic;
using Systems.Grid;
using Systems.Grid.AlterationPasses;
using UnityEditor;
using UnityEngine;

namespace Systems.Coordinators.Editor
{
    [CustomEditor(typeof(WorldGeneratorCoordinator))]
    public class WorldGeneratorCoordinatorEditor : UnityEditor.Editor
    {
        private readonly List<IGridAlterationPass> _availablePasses = new List<IGridAlterationPass>()
        {
            new PerlinNoiseAlterationPass(),
            new MountainSmoothingAlterationPass(),
            new RotationAlterationPass()
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WorldGeneratorCoordinator coord = (WorldGeneratorCoordinator)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Generation Pipeline Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Add passes here to define the sequence of world generation.", MessageType.Info);

            EditorGUILayout.LabelField("Available Pass Templates", EditorStyles.miniLabel);
            foreach (IGridAlterationPass pass in _availablePasses)
            {
                if (GUILayout.Button($"Add {pass.GetType().Name}"))
                {
                    Undo.RecordObject(coord, "Add Generator Pass");
                    coord.AddGeneratorPass(pass);
                    EditorUtility.SetDirty(coord);
                }
            }

            var currentPasses = coord.GetGeneratorPasses();
            if (currentPasses.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Active Pipeline Actions", EditorStyles.boldLabel);
                
                foreach (IGridAlterationPass pass in currentPasses)
                {
                    if (GUILayout.Button($"Remove {pass.GetType().Name}", EditorStyles.miniButton))
                    {
                        Undo.RecordObject(coord, "Remove Generator Pass");
                        coord.RemoveGeneratorPass(pass);
                        EditorUtility.SetDirty(coord);
                    }
                }
            }
        }
    }
}
#endif