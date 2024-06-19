using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RecipeList", menuName = "Scriptable Objects/Recipe List")]
public class SO_RecipeList : ScriptableObject
{
    public List<RecipeCodes> recipeList;
}

[System.Serializable]
public struct RecipeCodes
{
    //then assign proper ID of desired ingredients to array's childs.
    //note that IDs index should be carefully selected from existing ingrediets.
    //we also can use duplicate indexs. meaning a recipe can consist of two or more of the same ingredient.
    public int[] ingredientsCodes;
    
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
