using UnityEngine;
using UnityEditor;

namespace BazarEsKrim
{
    [CustomPropertyDrawer(typeof(AspectRatioData))]
    public class AspectRatioDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the aspect ratio enum from the serialized property
            SerializedProperty aspectRatioProp = property.FindPropertyRelative("aspectRatio");

            // Get the current AspectRatio enum value
            AspectRatio aspectRatio = (AspectRatio)aspectRatioProp.enumValueIndex;

            // Convert enum to string in the format "16:9" by parsing the enum name
            string aspectString = ConvertAspectRatioEnumToString(aspectRatio);

            // Change the label to reflect the parsed AspectRatio (e.g., "16:9")
            label = new GUIContent(aspectString);

            // Start property drawing
            EditorGUI.BeginProperty(position, label, property);

            // Initialize Rect for content drawing
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Initialize content position for child properties
                Rect contentPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

                // Draw AspectRatio field
                EditorGUI.PropertyField(contentPosition, aspectRatioProp);
                contentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Draw AdjustScale field
                SerializedProperty adjustScaleProp = property.FindPropertyRelative("adjustScale");
                EditorGUI.PropertyField(contentPosition, adjustScaleProp);
                contentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Draw AdjustPosition field
                SerializedProperty adjustPositionProp = property.FindPropertyRelative("adjustPosition");
                EditorGUI.PropertyField(contentPosition, adjustPositionProp);
                contentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Conditionally display the scale field if adjustScale is true
                if (adjustScaleProp.boolValue)
                {
                    SerializedProperty scaleProp = property.FindPropertyRelative("scale");
                    EditorGUI.PropertyField(contentPosition, scaleProp);
                    contentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                // Conditionally display the position field if adjustPosition is true
                if (adjustPositionProp.boolValue)
                {
                    SerializedProperty positionProp = property.FindPropertyRelative("position");
                    EditorGUI.PropertyField(contentPosition, positionProp);
                    contentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            // End property drawing
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Base height for foldout
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                // Add height for aspectRatio, adjustScale, and adjustPosition fields
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;

                // Add extra height for scale and position only if they are enabled
                SerializedProperty adjustScaleProp = property.FindPropertyRelative("adjustScale");
                if (adjustScaleProp.boolValue)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                SerializedProperty adjustPositionProp = property.FindPropertyRelative("adjustPosition");
                if (adjustPositionProp.boolValue)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        // Convert the AspectRatio enum to a "width:height" string format
        private string ConvertAspectRatioEnumToString(AspectRatio aspectRatio)
        {
            // Get the enum name (e.g., "Aspect16_9")
            string enumName = aspectRatio.ToString();

            // Remove the "Aspect" prefix and replace "_" with ":"
            string formattedString = enumName.Replace("Aspect", "").Replace("_", ":");

            return formattedString;
        }
    }
}
