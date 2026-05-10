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

        /// <summary>
        /// Renders the property drawer in the inspector, including live data access via reflection for non-serialized properties.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yOffset = EditorGUIUtility.singleLineHeight + 2;

                SerializedProperty xProp = property.FindPropertyRelative("x");
                SerializedProperty zProp = property.FindPropertyRelative("z");
                
                EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), $"Coordinates: ({xProp.intValue}, {zProp.intValue})");
                yOffset += EditorGUIUtility.singleLineHeight;

                TileData tileData = GetTargetObjectOfProperty(property) as TileData;

                if (tileData != null)
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), $"Type: {tileData.type} | Index: {tileData.VariationIndex}");
                    yOffset += EditorGUIUtility.singleLineHeight;

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

        /// <summary>
        /// Calculates the dynamic height of the property drawer based on whether foldouts are expanded.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            
            float height = EditorGUIUtility.singleLineHeight * 3 + 10;
            if (_showNeighbors) height += EditorGUIUtility.singleLineHeight * 7;
            return height;
        }

        /// <summary>
        /// Uses reflection to retrieve the actual object instance from a SerializedProperty path.
        /// </summary>
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

        /// <summary>
        /// Retrieves a field value from an object using reflection.
        /// </summary>
        private object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null) return null;
            return f.GetValue(source);
        }

        /// <summary>
        /// Retrieves an indexed value from an enumerable field using reflection.
        /// </summary>
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