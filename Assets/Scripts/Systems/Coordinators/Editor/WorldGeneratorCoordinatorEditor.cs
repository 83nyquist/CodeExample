#if UNITY_EDITOR
using System.Collections.Generic;
using Systems.Grid.Passes.Alteration;
using Systems.Grid.Passes.Generation;
using UnityEditor;
using UnityEngine;

namespace Systems.Coordinators.Editor
{
    [CustomEditor(typeof(WorldGeneratorCoordinator))]
    public class WorldGeneratorCoordinatorEditor : UnityEditor.Editor
    {
        private readonly List<IGridGenerationPass> _generationTemplates = new List<IGridGenerationPass>()
        {
            new PerlinNoiseGenerationPass(),
            new GeographyGenerationPass(),
            new StandardBiomeGenerationPass(),
        };

        private readonly List<IGridAlterationPass> _alterationTemplates = new List<IGridAlterationPass>()
        {
            new DefaultVariationAlterationPass(),
            new RotationAlterationPass(),
            new MountainSmoothingAlterationPass(),
            new MassiveMountainAlterationPass(),
            new ForestAlterationPass(),
            new WaterDepthAlterationPass(),
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WorldGeneratorCoordinator coord = (WorldGeneratorCoordinator)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Pipeline Templates", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use these buttons to quickly add new passes to the generation or alteration lists.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Available Generation Passes", EditorStyles.miniBoldLabel);
            foreach (var pass in _generationTemplates)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pass.PassName, GUILayout.ExpandWidth(true));
                
                bool exists = coord.HasGenerationPass(pass.GetType());

                using (new EditorGUI.DisabledScope(exists))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coord, "Add Generation Pass");
                        coord.AddGenerationPass(pass);
                        EditorUtility.SetDirty(coord);
                    }
                }

                using (new EditorGUI.DisabledScope(!exists))
                {
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coord, "Remove Generation Pass");
                        coord.RemoveGenerationPass(pass.GetType());
                        EditorUtility.SetDirty(coord);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Available Alteration Passes", EditorStyles.miniBoldLabel);
            foreach (var pass in _alterationTemplates)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pass.PassName, GUILayout.ExpandWidth(true));

                bool exists = coord.HasAlterationPass(pass.GetType());

                using (new EditorGUI.DisabledScope(exists))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coord, "Add Alteration Pass");
                        coord.AddAlterationPass(pass);
                        EditorUtility.SetDirty(coord);
                    }
                }

                using (new EditorGUI.DisabledScope(!exists))
                {
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coord, "Remove Alteration Pass");
                        coord.RemoveAlterationPass(pass.GetType());
                        EditorUtility.SetDirty(coord);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(10);
        }
    }
}
#endif