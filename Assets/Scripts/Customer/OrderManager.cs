using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [HideInInspector] public int[] productIngredientsCodes;

    [SerializeField] private GameObject BubblePos;
    [SerializeField] private SO_IngredientObject allIngredients;
    [SerializeField] private SO_IngredientObject baseIngredients;
    [SerializeField] private SO_IngredientObject flavorIngredients;
    [SerializeField] private SO_IngredientObject toppingIngredients;
    [SerializeField] private SO_RecipeList recipeList;
    [SerializeField] private SO_LevelDataList levelDataIngredientCodes;

    // Method to add a base ingredient to the order
    private void AddBaseIngredient()
    {
        Ingredient baseIngredient = GetRandomIngredientFromArray(baseIngredients.ingredientObjects, levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].baseIngredientCode.Length);
        productIngredientsCodes[0] = baseIngredient.IngredientCode;
    }

    // Method to add a flavor ingredient to the order
    private void AddFlavorIngredient(int index)
    {
        Ingredient flavorIngredient = GetRandomIngredientFromArray(flavorIngredients.ingredientObjects, levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].flavorIngredientCode.Length);
        productIngredientsCodes[index] = flavorIngredient.IngredientCode;
    }

    // Method to add a topping ingredient to the order
    private void AddToppingIngredient(int index)
    {
        Ingredient toppingIngredient = GetRandomIngredientFromArray(toppingIngredients.ingredientObjects, levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].toppingIngredientCode.Length);
        productIngredientsCodes[index] = toppingIngredient.IngredientCode;
    }

    // Method to get a random ingredient from a specific array
    private Ingredient GetRandomIngredientFromArray(GameObject[] ingredientArray, int unlockedCount)
    {
        int randomIndex = Random.Range(0, Mathf.Min(unlockedCount, ingredientArray.Length));
        return ingredientArray[randomIndex].GetComponent<Ingredient>();
    }

    private void DisplayOrder()
    {
        // Create a dictionary for quick lookup of ingredients by code
        Dictionary<int, GameObject> ingredientLookup = new Dictionary<int, GameObject>();
        foreach (GameObject ingredientObject in allIngredients.ingredientObjects)
        {
            Ingredient ingredient = ingredientObject.GetComponent<Ingredient>();
            ingredientLookup[ingredient.IngredientCode] = ingredientObject;
        }

        GameObject lastTmpIngredient = null; // Initialize to null to avoid uninitialized usage
        int tempSortingOrder = 1;
        foreach (int ingredientCode in productIngredientsCodes)
        {
            if (!ingredientLookup.TryGetValue(ingredientCode, out GameObject tmpIngredient))
            {
                Debug.LogError($"Ingredient code {ingredientCode} not found.");
                continue;
            }

            Ingredient ingredient = tmpIngredient.GetComponent<Ingredient>();
            IngredientType ingredientType = ingredient.IngredientType;

            Vector3 spawnPosition = BubblePos.transform.position;
            Transform parentTransform = BubblePos.transform;

            if (lastTmpIngredient != null)
            {
                Transform pointTransform = lastTmpIngredient.transform.GetChild(lastTmpIngredient.transform.childCount - 1); // Assuming the point transform is the second child
                spawnPosition = pointTransform.position;

                if (ingredientType == IngredientType.Topping)
                {
                    spawnPosition = lastTmpIngredient.transform.position;
                }

                tempSortingOrder++;
            }

            // Instantiate the ingredient
            lastTmpIngredient = Instantiate(tmpIngredient, spawnPosition, Quaternion.identity, parentTransform);

            // Set the sorting layer
            Transform childlastTmpIngredientTransform = lastTmpIngredient.transform.GetChild(0);
            SpriteRenderer childlastTmpIngredientSpriteRenderer = childlastTmpIngredientTransform.GetComponent<SpriteRenderer>();

            childlastTmpIngredientSpriteRenderer.sortingLayerName = "Customer HUD";

            if (lastTmpIngredient != null)
            {
                childlastTmpIngredientSpriteRenderer.sortingOrder = tempSortingOrder;
            }
            else
            {
                tempSortingOrder = childlastTmpIngredientSpriteRenderer.sortingOrder;
            }

            if (ingredientCode == 1023) // astor
            {
                childlastTmpIngredientSpriteRenderer.sortingOrder -= 2;
            }
        }
    }

    // Main method to add ingredients to the order
    public void OrderRandomProduct(int randomMaxOrder)
    {
        productIngredientsCodes = new int[randomMaxOrder];

        // Add base ingredient
        AddBaseIngredient();

        // Add flavor and topping ingredients based on order size
        for (int i = 1; i < randomMaxOrder; i++)
        {
            if (randomMaxOrder == 2 && i == 1) // Only 2 elements, add flavor
            {
                AddFlavorIngredient(i);
            }
            else if (randomMaxOrder > 2 && i < randomMaxOrder - 1) // More than 2 elements, add flavors
            {
                AddFlavorIngredient(i);
            }
            else if (i == randomMaxOrder - 1) // Last element
            {
                int unlockedToppingsCount = levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].toppingIngredientCode.Length;
                if (unlockedToppingsCount > 0)
                {
                    AddToppingIngredient(i);
                }
                else
                {
                    AddFlavorIngredient(i);
                }
            }
        }

        DisplayOrder();
    }

    public void OrderByRecipe(int recipeUnlockIndex)
    {
        int randomRecipeIndex = Random.Range(0, recipeUnlockIndex - 1);
        if(GameManager.Instance.currentLevel == recipeUnlockIndex + 1)
        {
            randomRecipeIndex = recipeUnlockIndex - 1;
        }

        int maxOrderByRecipe = recipeList.recipeList[randomRecipeIndex].ingredientsCodes.Length;

        productIngredientsCodes = new int[maxOrderByRecipe];

        for (int i = 0; i < maxOrderByRecipe; i++)
        {
            productIngredientsCodes[i] = recipeList.recipeList[randomRecipeIndex].ingredientsCodes[i];
        }

        DisplayOrder();
    }
}