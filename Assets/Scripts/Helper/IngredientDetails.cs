using UnityEngine;

[System.Serializable]
public class IngredientDetails
{
    public IngredientName ingredientCode;
    public string ingredientName;
    public IngredientType ingredientType;
    public ConeTypes coneTypes = ConeTypes.NotACone;
    public Sprite basketIngredientSprite;
    public Sprite dressIngredientSprite;
    public Sprite collectionIngredientSprite;
    public float nextIngredientPosY = 0.8162498f;
    // public Sprite plateIngredientSprite;
}
