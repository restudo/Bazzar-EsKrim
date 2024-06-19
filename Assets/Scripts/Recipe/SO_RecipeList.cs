using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RecipeList", menuName = "Scriptable Objects/Recipe List")]
public class SO_RecipeList : ScriptableObject
{
    public List<RecipeCodes> recipeList;
}
