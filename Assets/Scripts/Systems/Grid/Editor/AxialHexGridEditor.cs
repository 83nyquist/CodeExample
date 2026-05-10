#if UNITY_EDITOR
using Systems.Grid.Components;
using UnityEditor;

namespace Systems.Grid.Editor
{
    [CustomEditor(typeof(AxialHexGrid))]
    public class AxialHexGridEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the custom inspector for the AxialHexGrid, displaying live tile counts and neighbor debug data.
        /// </summary>
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
                int neighborCount = origin.Neighbours?.Length ?? 0;
                EditorGUILayout.LabelField($"Origin (0,0) Neighbors: {neighborCount}");
            }
        }
    }
}
#endif