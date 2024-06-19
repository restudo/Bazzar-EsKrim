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
    public int[] baseIngredientCode;
    public int[] flavorIngredientCode;
    public int[] toppingIngredientCode;
}
