#if UNITY_EDITOR
using System.Collections.Generic;
using Systems.Grid.Passes.Abstraction;
using Systems.Grid.Passes.Alteration;
using Systems.Grid.Passes.Generation;
using UnityEditor;
using UnityEngine;

namespace Coordinators.Editor
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

        /// <summary>
        /// Renders the custom inspector UI for adding and removing passes.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            WorldGeneratorCoordinator coordinator = (WorldGeneratorCoordinator)target;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Pipeline Templates", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use these buttons to quickly add new passes to the generation or alteration lists.", MessageType.Info);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Available Generation Passes", EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("Remove All", GUILayout.Width(80), GUILayout.Height(18)))
            {
                ShowRemoveAllConfirmation(coordinator, isGeneration: true);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            foreach (var pass in _generationTemplates)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pass.PassName, GUILayout.ExpandWidth(true));
                
                bool exists = coordinator.HasGenerationPass(pass.GetType());

                using (new EditorGUI.DisabledScope(exists))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coordinator, "Add Generation Pass");
                        coordinator.AddGenerationPass(pass);
                        EditorUtility.SetDirty(coordinator);
                    }
                }

                using (new EditorGUI.DisabledScope(!exists))
                {
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coordinator, "Remove Generation Pass");
                        coordinator.RemoveGenerationPass(pass.GetType());
                        EditorUtility.SetDirty(coordinator);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Available Alteration Passes", EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("Remove All", GUILayout.Width(80), GUILayout.Height(18)))
            {
                ShowRemoveAllConfirmation(coordinator, isGeneration: false);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            foreach (var pass in _alterationTemplates)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pass.PassName, GUILayout.ExpandWidth(true));

                bool exists = coordinator.HasAlterationPass(pass.GetType());

                using (new EditorGUI.DisabledScope(exists))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coordinator, "Add Alteration Pass");
                        coordinator.AddAlterationPass(pass);
                        EditorUtility.SetDirty(coordinator);
                    }
                }

                using (new EditorGUI.DisabledScope(!exists))
                {
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(coordinator, "Remove Alteration Pass");
                        coordinator.RemoveAlterationPass(pass.GetType());
                        EditorUtility.SetDirty(coordinator);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// Displays a confirmation dialog for bulk removal.
        /// </summary>
        private void ShowRemoveAllConfirmation(WorldGeneratorCoordinator coord, bool isGeneration)
        {
            string passType = isGeneration ? "generation" : "alteration";
            
            if (EditorUtility.DisplayDialog(
                $"Remove All {passType} Passes",
                $"WARNING: This will remove ALL {passType} passes from the pipeline.\n\n" +
                "This action cannot be undone through Undo.\n\n" +
                $"Are you sure you want to remove all {passType} passes?",
                $"Yes, Remove All {passType} Passes",
                "Cancel"))
            {
                ExecuteRemoveAll(coord, isGeneration);
            }
        }

        /// <summary>
        /// Executes the removal of all passes from the selected list.
        /// </summary>
        private void ExecuteRemoveAll(WorldGeneratorCoordinator coord, bool isGeneration)
        {
            Undo.RecordObject(coord, $"Remove All {(isGeneration ? "Generation" : "Alteration")} Passes");
            
            if (isGeneration)
            {
                coord.ClearGenerationPasses();
            }
            else
            {
                coord.ClearAlterationPasses();
            }
            
            EditorUtility.SetDirty(coord);
            Debug.Log($"[WorldGeneratorCoordinatorEditor] All {(isGeneration ? "generation" : "alteration")} passes removed.");
        }
    }
}
#endif