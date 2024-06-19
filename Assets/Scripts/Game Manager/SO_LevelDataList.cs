using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_LevelDataList", menuName = "Scriptable Objects/Level Data")]
public class SO_LevelDataList : ScriptableObject
{
    public List<LevelData> levelDataList;
}

[System.Serializable]
public struct LevelData
{
    public int maxOrderHeight;
    public int spawnSpecialRecipeAfterXCustomer;
    public int customerDelay;
    [Range(0, 1)] 
    public float doubleCustomerProbability;
    public int maxPoint;
    public int pointPerCustomer;
    public int timer;

    [Space(20)]
    public IngredientName[] baseIngredientCode;
    public IngredientName[] flavorIngredientCode;
    public IngredientName[] toppingIngredientCode;

    [Space(20)]
    public RecipeCodes[] recipeList;
}

[System.Serializable]
public struct RecipeCodes
{
    //then assign proper ID of desired ingredients to array's childs.
    //note that IDs index should be carefully selected from existing ingrediets.
    //we also can use duplicate indexs. meaning a recipe can consist of two or more of the same ingredient.
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
