using UnityEngine;
using System;
using System.Collections.Generic;

namespace BazarEsKrim
{
    [CreateAssetMenu(fileName = "SO_IngredientHolderPos", menuName = "Scriptable Objects/Ingredient Holder Pos")]
    public class SO_IngredientHolderPos : ScriptableObject
    {
        public List<IngredientHolderPosInfos> ingredientHolderPosInfos; // 0 is recipe 1, 1 is recipe 2
    }

    [Serializable]
    public struct IngredientHolderPosInfos
    {
        public ConeTypes coneTypes;
        public float[] holderYPosByHeight;
    }
}
