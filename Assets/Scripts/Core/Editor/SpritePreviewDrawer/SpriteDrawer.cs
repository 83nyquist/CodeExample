using System;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.SpritePreviewDrawer
{
    public class SpriteDrawer : PropertyDrawer
    {
        const float ImageHeight = 100;

        /// <summary>
        /// Calculates the height of the property based on whether a sprite preview is displayed.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                (property.objectReferenceValue as Sprite) != null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true) + ImageHeight + 10;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary>
        /// Retrieves the path of the property for internal use.
        /// </summary>
        static string GetPath(SerializedProperty property)
        {
            string path = property.propertyPath;
            int index = path.LastIndexOf(".", StringComparison.Ordinal);
            return path.Substring(0, index + 1);
        }

        /// <summary>
        /// Renders the property field and an optional sprite preview in the Inspector.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var sprite = property.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                    position.height = ImageHeight;
                    DrawTexturePreview(position, sprite);
                }
            }
        }

        /// <summary>
        /// Draws a preview of the sprite correctly mapped from its atlas texture.
        /// </summary>
        private void DrawTexturePreview(Rect position, Sprite sprite)
        {
            Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);

            Rect coords = sprite.textureRect;
            coords.x /= fullSize.x;
            coords.width /= fullSize.x;
            coords.y /= fullSize.y;
            coords.height /= fullSize.y;

            Vector2 ratio;
            ratio.x = position.width / size.x;
            ratio.y = position.height / size.y;
            float minRatio = Mathf.Min(ratio.x, ratio.y);

            Vector2 center = position.center;
            position.width = size.x * minRatio;
            position.height = size.y * minRatio;
            position.center = center;

            GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
        }
    }
}