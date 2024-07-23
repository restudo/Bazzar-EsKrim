using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_LevelDataList", menuName = "Scriptable Objects/Level Data")]
public class SO_LevelDataList : ScriptableObject
{
    public LevelDataMainGame mainGameLevelData;

    public LevelDataMiniGame miniGameLevelData;
}

[System.Serializable]
public struct LevelDataMainGame
{
    [Header("General Settings")]
    [Tooltip("Maximum height of the order.")]
    public int maxOrderHeight;

    [Tooltip("Spawn special recipe after every X customer.")]
    public int spawnSpecialRecipeAfterXCustomer;

    [Tooltip("Delay between customer spawns.")]
    public int customerDelay;

    [Range(0, 1)]
    [Tooltip("Probability of spawning double customers.")]
    public float doubleCustomerProbability;

    [Tooltip("Maximum points achievable in this level.")]
    public int maxPoint;

    [Tooltip("Points awarded per customer.")]
    public int pointPerCustomer;

    [Tooltip("Timer for the level.")]
    public int timer;

    [Space(20)]
    [Header("Ingredient Codes")]
    [Tooltip("Codes for base ingredients.")]
    public IngredientName[] baseIngredientCode;

    [Tooltip("Codes for flavor ingredients.")]
    public IngredientName[] flavorIngredientCode;

    [Tooltip("Codes for topping ingredients.")]
    public IngredientName[] toppingIngredientCode;

    [Space(20)]
    [Header("Special Recipes")]
    [Tooltip("List of special recipes for this level.")]
    public RecipeCodes[] recipeList;
}

[System.Serializable]
public struct RecipeCodes
{
    //then assign proper ID of desired ingredients to array's childs.
    //note that IDs index should be carefully selected from existing ingrediets.
    //we also can use duplicate indexs. meaning a recipe can consist of two or more of the same ingredient.
     [Tooltip("Array of ingredient codes for the recipe. Note that IDs index should be carefully selected from existing ingredients. Duplicate indices are allowed, meaning a recipe can consist of two or more of the same ingredient.")]
    public IngredientName[] ingredientsCodes;
    
    //for example
    // a custom recipe definition is like this:
    /*
	totalIngredients = 6;
	ingredientsCodes[0] = 0;
	ingredientsCodes[1] = 4;
	ingredientsCodes[2] = 7;
	ingredientsCodes[3] = 12;
	ingredientsCodes[4] = 13;
	ingredientsCodes[5] = 18;
	*/

    //Another example
    /*
	totalIngredients = 3;
	ingredientsCodes[0] = 1;
	ingredientsCodes[1] = 2;
	ingredientsCodes[2] = 15;
	*/
}

[System.Serializable]
public struct LevelDataMiniGame
{
    public float timer;
    public int totalObjectSpawner;
}
