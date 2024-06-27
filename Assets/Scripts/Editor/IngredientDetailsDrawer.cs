using UnityEngine;
using UnityEditor;

// [CustomPropertyDrawer(typeof(IngredientDetails))]
public class IngredientDetailsDrawer : PropertyDrawer
{
    // private const float spacing = 25f; // Adjust this value to set the desired space between elements

    // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    // {
    //     EditorGUI.BeginProperty(position, label, property);

    //     var ingredientCode = property.FindPropertyRelative("ingredientCode");
    //     var ingredientName = property.FindPropertyRelative("ingredientName");
    //     var ingredientType = property.FindPropertyRelative("ingredientType");
    //     var basketIngredientSprite = property.FindPropertyRelative("basketIngredientSprite");
    //     var dressIngredientSprite = property.FindPropertyRelative("dressIngredientSprite");
    //     var plateIngredientSprite = property.FindPropertyRelative("plateIngredientSprite");

    //     float lineHeight = EditorGUIUtility.singleLineHeight;
    //     float lineSpacing = EditorGUIUtility.standardVerticalSpacing;
    //     Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

    //     EditorGUI.PropertyField(currentRect, ingredientCode);
    //     currentRect.y += lineHeight + lineSpacing;
    //     EditorGUI.PropertyField(currentRect, ingredientName);
    //     currentRect.y += lineHeight + lineSpacing;
    //     EditorGUI.PropertyField(currentRect, ingredientType);
    //     currentRect.y += lineHeight + lineSpacing;
    //     EditorGUI.PropertyField(currentRect, basketIngredientSprite);
    //     currentRect.y += lineHeight + lineSpacing;
    //     EditorGUI.PropertyField(currentRect, dressIngredientSprite);
    //     currentRect.y += lineHeight + lineSpacing;

    //     if ((IngredientType)ingredientType.enumValueIndex == IngredientType.Topping)
    //     {
    //         EditorGUI.PropertyField(currentRect, plateIngredientSprite);
    //         currentRect.y += lineHeight + lineSpacing;
    //     }

    //     EditorGUI.EndProperty();

    //     // Adding spacing at the end of each element
    //     currentRect.y += spacing;
    // }

    // public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    // {
    //     int lines = 5; // base, name, type, basket, dress
    //     var ingredientType = property.FindPropertyRelative("ingredientType");
    //     if ((IngredientType)ingredientType.enumValueIndex == IngredientType.Topping)
    //     {
    //         lines++;
    //     }
    //     // Adding the space height
    //     return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lines - EditorGUIUtility.standardVerticalSpacing + spacing;
    // }
}