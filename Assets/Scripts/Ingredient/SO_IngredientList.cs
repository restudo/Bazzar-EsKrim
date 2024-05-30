using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_IngredientList", menuName = "Scriptable Objects/Ingredient List")]
public class SO_IngredientList : ScriptableObject
{
    public List<IngredientDetails> ingredientDetails;
}
