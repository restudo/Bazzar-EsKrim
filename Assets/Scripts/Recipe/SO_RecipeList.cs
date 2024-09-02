using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RecipeList", menuName = "Scriptable Objects/Recipe List")]
public class SO_RecipeList : ScriptableObject
{
    [Tooltip("Array of ingredient codes for the recipe. Note that IDs index should be carefully selected from existing ingredients. Duplicate indices are allowed, meaning a recipe can consist of two or more of the same ingredient.")]
    [HideInInspector]
    public List<IngredientName> ingredientsCodes = new List<IngredientName>();

    [Header("Ingredient Codes")]
    [Tooltip("Codes for base ingredients.")]
    public IngredientName[] baseIngredientCode;

    [Tooltip("Codes for flavor ingredients.")]
    public IngredientName[] flavorIngredientCode;

    [Tooltip("Codes for topping ingredients.")]
    public IngredientName[] toppingIngredientCode;

    public string recipeName;
    public Sprite recipeSprite;

    public void Set()
    {
        ingredientsCodes.Clear();

        foreach (IngredientName ingredientCode in baseIngredientCode)
        {
            ingredientsCodes.Add(ingredientCode);
        }

        foreach (IngredientName ingredientCode in flavorIngredientCode)
        {
            ingredientsCodes.Add(ingredientCode);
        }

        foreach (IngredientName ingredientCode in toppingIngredientCode)
        {
            ingredientsCodes.Add(ingredientCode);
        }
    }
}
