using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RecipeList", menuName = "Scriptable Objects/Recipe List")]
public class SO_RecipeList : ScriptableObject
{
    [Tooltip("Array of ingredient codes for the recipe. Note that IDs index should be carefully selected from existing ingredients. Duplicate indices are allowed, meaning a recipe can consist of two or more of the same ingredient.")]
    public IngredientName[] ingredientsCodes;
    public string recipeName;
    public Sprite recipeSprite;
}
