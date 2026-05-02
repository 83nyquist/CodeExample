#if UNITY_EDITOR
using System.Collections.Generic;
using Systems.Grid.AlterationPasses;
using UnityEditor;
using UnityEngine;

namespace Systems.Grid.Editor
{
    [CustomEditor(typeof(AxialHexGrid))]
    public class AxialHexGridEditor : UnityEditor.Editor
    {
        /*
         * Only add compatible alteration passes to this list and the OnInspectorGUI will display them
         */
        private readonly List<IGridAlterationPass> _generationPasses = new List<IGridAlterationPass>()
        {
            new PerlinNoiseAlterationPass(),
            new MountainSmoothingAlterationPass(),
            new RotationAlterationPass()
        };
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            AxialHexGrid grid = (AxialHexGrid)target;
        
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Tile Count: {grid.Tiles.Count}");
            TileData origin = grid.GetTile(0, 0);
            if (origin != null)
            {
                EditorGUILayout.LabelField($"Origin Neighbour Count: {origin.Neighbours.Count}");
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Add Generator Passes", EditorStyles.boldLabel);
            
            foreach (IGridAlterationPass pass in _generationPasses)
            {
                if (GUILayout.Button($"Add {pass.GetType().Name}"))
                {
                    grid.AddGeneratorPass(pass);
                    EditorUtility.SetDirty(grid);
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Remove Generator Passes", EditorStyles.boldLabel);
            
            foreach (IGridAlterationPass pass in grid.GetGeneratorPasses())
            {
                if (GUILayout.Button($"Remove {pass.GetType().Name}"))
                {
                    grid.RemoveGeneratorPass(pass);
                    EditorUtility.SetDirty(grid);
                }
            }
        }
    }
}
#endif