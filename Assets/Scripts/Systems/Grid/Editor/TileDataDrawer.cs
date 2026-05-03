#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Systems.Grid.Components;
using System.Reflection;

namespace Systems.Grid.Editor
{
    [CustomPropertyDrawer(typeof(TileData))]
    public class TileDataDrawer : PropertyDrawer
    {
        private bool _showNeighbors = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the foldout for the TileData itself
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yOffset = EditorGUIUtility.singleLineHeight + 2;

                // 1. Draw standard Serialized Fields
                SerializedProperty xProp = property.FindPropertyRelative("x");
                SerializedProperty zProp = property.FindPropertyRelative("z");
                
                EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), $"Coordinates: ({xProp.intValue}, {zProp.intValue})");
                yOffset += EditorGUIUtility.singleLineHeight;

                // 2. Access the actual class instance to show NonSerialized data
                TileData tileData = GetTargetObjectOfProperty(property) as TileData;

                if (tileData != null)
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), $"Type: {tileData.type} | Index: {tileData.VariationIndex}");
                    yOffset += EditorGUIUtility.singleLineHeight;

                    // 3. Draw Neighbors (The non-serialized part)
                    _showNeighbors = EditorGUI.Foldout(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), _showNeighbors, "Neighbors (Live Data)");
                    yOffset += EditorGUIUtility.singleLineHeight;

                    if (_showNeighbors && tileData.Neighbours != null)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < tileData.Neighbours.Length; i++)
                        {
                            TileData n = tileData.Neighbours[i];
                            string nText = n != null ? $"[{i}] ({n.X}, {n.Z}) - {n.type}" : $"[{i}] Empty";
                            EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), nText);
                            yOffset += EditorGUIUtility.singleLineHeight;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            
            float height = EditorGUIUtility.singleLineHeight * 3 + 10; // Basic info
            if (_showNeighbors) height += EditorGUIUtility.singleLineHeight * 7; // Neighbor list
            return height;
        }

        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null) return null;
            return f.GetValue(source);
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) if (!enm.MoveNext()) return null;
            return enm.Current;
        }
    }
}
#endif