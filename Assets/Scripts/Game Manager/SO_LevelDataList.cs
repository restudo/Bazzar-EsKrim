using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "SO_LevelDataList", menuName = "Scriptable Objects/Level Data")]
public class SO_LevelDataList : ScriptableObject
{
    public LevelDataMainGame mainGameLevelData;

    public LevelDataMiniGame miniGameLevelData;
}

[System.Serializable]
public class LevelDataMainGame
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

    [Tooltip("Points awarded per special Recipe.")]
    public int specialRecipePoint;

    [Tooltip("Timer for the level.")]
    public int timer;

    [Space(20)]
    [Header("Ingredient Codes")]
    [Tooltip("Codes for base ingredients.")]
    [HideInInspector]
    public List<IngredientName> baseIngredientCode = new List<IngredientName>();

    [Tooltip("Codes for flavor ingredients.")]
    [HideInInspector]
    public List<IngredientName> flavorIngredientCode = new List<IngredientName>();

    [Tooltip("Codes for topping ingredients.")]
    [HideInInspector]
    public List<IngredientName> toppingIngredientCode = new List<IngredientName>();

    [Space(20)]
    [Header("Special Recipes")]
    [Tooltip("List of special recipes for this level.")]
    public SO_RecipeList[] sO_RecipeList;

    private const int maxQtyIngredientsPerType = 5;

    private const int minBaseCode = 1001;
    private const int maxBaseCode = 1006;

    private const int minFlavorCode = 1007;
    private const int maxFlavorCode = 1016;

    private const int minToppingCode = 1017;
    private const int maxToppingCode = 1026;

    private void AddRange(List<IngredientName> list, IngredientName[] range)
    {
        for (int i = 0; i < range.Length; i++)
        {
            list.Add(range[i]);
        }
    }

    private void FillToMaxQty(List<IngredientName> list, int minCode, int maxCode)
    {
        while (list.Count < maxQtyIngredientsPerType)
        {
            IngredientName random = (IngredientName)Random.Range(minCode, maxCode + 1);

            bool exists = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == random)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                list.Add(random);
            }
        }
    }

    public void Set()
    {
        baseIngredientCode.Clear();
        flavorIngredientCode.Clear();
        toppingIngredientCode.Clear();

        foreach (var recipe in sO_RecipeList)
        {
            AddRange(baseIngredientCode, recipe.baseIngredientCode);
            AddRange(flavorIngredientCode, recipe.flavorIngredientCode);
            AddRange(toppingIngredientCode, recipe.toppingIngredientCode);
        }

        FillToMaxQty(baseIngredientCode, minBaseCode, maxBaseCode);
        FillToMaxQty(flavorIngredientCode, minFlavorCode, maxFlavorCode);
        FillToMaxQty(toppingIngredientCode, minToppingCode, maxToppingCode);
    }
}

// [System.Serializable]
// public struct RecipeCodes
// {
//     //then assign proper ID of desired ingredients to array's childs.
//     //note that IDs index should be carefully selected from existing ingrediets.
//     //we also can use duplicate indexs. meaning a recipe can consist of two or more of the same ingredient.
//      [Tooltip("Array of ingredient codes for the recipe. Note that IDs index should be carefully selected from existing ingredients. Duplicate indices are allowed, meaning a recipe can consist of two or more of the same ingredient.")]
//     public IngredientName[] ingredientsCodes;

//     //for example
//     // a custom recipe definition is like this:
//     /*
// 	totalIngredients = 6;
// 	ingredientsCodes[0] = 0;
// 	ingredientsCodes[1] = 4;
// 	ingredientsCodes[2] = 7;
// 	ingredientsCodes[3] = 12;
// 	ingredientsCodes[4] = 13;
// 	ingredientsCodes[5] = 18;
// 	*/

//     //Another example
//     /*
// 	totalIngredients = 3;
// 	ingredientsCodes[0] = 1;
// 	ingredientsCodes[1] = 2;
// 	ingredientsCodes[2] = 15;
// 	*/
// }

[System.Serializable]
public struct LevelDataMiniGame
{
    public float timer;
    public int totalObjectSpawner;
}
