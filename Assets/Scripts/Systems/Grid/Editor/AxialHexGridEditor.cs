#if UNITY_EDITOR
using Systems.Grid.AlterationPasses;
using UnityEditor;
using UnityEngine;

namespace Systems.Grid.Editor
{
    [CustomEditor(typeof(AxialHexGrid))]
    public class AxialHexGridEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            AxialHexGrid grid = (AxialHexGrid)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Live Data Debugging", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Tiles: {grid.Tiles.Count}");
            
            TileData origin = grid.GetTile(0, 0);
            if (origin != null)
            {
                // Check if Neighbors logic is linked
                int neighborCount = origin.Neighbours?.Count ?? 0;
                EditorGUILayout.LabelField($"Origin (0,0) Neighbors: {neighborCount}");
            }
        }
    }
}
#endif