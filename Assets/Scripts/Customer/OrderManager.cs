using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [HideInInspector] public int[] productIngredientsCodes;
    [SerializeField] private GameObject ingredientPrefab;

    [SerializeField] private GameObject BubblePos;
    [SerializeField] private GameObject specialRecipeTag;
    // [SerializeField] private SO_IngredientList ingredientList;
    // [SerializeField] private SO_IngredientObject allIngredients;
    // [SerializeField] private SO_IngredientObject baseIngredients;
    // [SerializeField] private SO_IngredientObject flavorIngredients;
    // [SerializeField] private SO_IngredientObject toppingIngredients;
    [SerializeField] private SO_LevelDataList levelDataList;

    private UiInventorySlot[] baseIngredientDetails;
    private UiInventorySlot[] flavorIngredientDetails;
    private UiInventorySlot[] toppingIngredientDetails;
    private GameObject[] baseInventorySlot;
    private GameObject[] flavorInventorySlot;
    private GameObject[] toppingInventorySlot;
    private IngredientPool ingredientPool;

    private List<Ingredient> ingredientsInPool;

    private void Awake()
    {
        ingredientsInPool = new List<Ingredient>();

        specialRecipeTag.SetActive(false);

        GetBaseIngredientDetails();
        GetFlavorIngredientDetails();
        GetToppingIngredientDetails();
    }

    private void Start()
    {
        ingredientPool = GetComponent<IngredientPool>();
    }

    private void GetBaseIngredientDetails()
    {
        baseInventorySlot = GameObject.FindGameObjectsWithTag("Base");
        baseIngredientDetails = new UiInventorySlot[baseInventorySlot.Length];

        // Use a foreach loop to populate the baseIngredientDetails array
        int index = 0;
        foreach (GameObject gameObject in baseInventorySlot)
        {
            baseIngredientDetails[index] = gameObject.GetComponent<UiInventorySlot>();
            index++;
        }
    }

    private void GetFlavorIngredientDetails()
    {
        flavorInventorySlot = GameObject.FindGameObjectsWithTag("Flavor");
        flavorIngredientDetails = new UiInventorySlot[flavorInventorySlot.Length];

        // Use a foreach loop to populate the flavorIngredientDetails array
        int index = 0;
        foreach (GameObject gameObject in flavorInventorySlot)
        {
            flavorIngredientDetails[index] = gameObject.GetComponent<UiInventorySlot>();
            index++;
        }
    }

    private void GetToppingIngredientDetails()
    {
        toppingInventorySlot = GameObject.FindGameObjectsWithTag("Topping");
        toppingIngredientDetails = new UiInventorySlot[toppingInventorySlot.Length];

        // Use a foreach loop to populate the toppingIngredientDetails array
        int index = 0;
        foreach (GameObject gameObject in toppingInventorySlot)
        {
            toppingIngredientDetails[index] = gameObject.GetComponent<UiInventorySlot>();
            index++;
        }
    }

    // Method to add a base ingredient to the order
    private void AddBaseIngredient()
    {
        productIngredientsCodes[0] = (int)baseIngredientDetails[GetRandomIngredientFromIngredientDetails(baseIngredientDetails)].ingredientDetails.ingredientCode;
    }
    // private void AddBaseIngredient()
    // {
    //     Ingredient baseIngredient = GetRandomIngredientFromIngredientDetails(baseIngredients.ingredientObjects, levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].baseIngredientCode.Length);
    //     productIngredientsCodes[0] = baseIngredient.IngredientCode;
    // }

    // Method to add a flavor ingredient to the order
    private void AddFlavorIngredient(int index)
    {
        productIngredientsCodes[index] = (int)flavorIngredientDetails[GetRandomIngredientFromIngredientDetails(flavorIngredientDetails)].ingredientDetails.ingredientCode;

        // Ingredient flavorIngredient = GetRandomIngredientFromIngredientDetails(flavorIngredients.ingredientObjects, levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].flavorIngredientCode.Length);
        // productIngredientsCodes[index] = flavorIngredient.IngredientCode;
    }

    // Method to add a topping ingredient to the order
    private void AddToppingIngredient(int index)
    {
        productIngredientsCodes[index] = (int)toppingIngredientDetails[GetRandomIngredientFromIngredientDetails(toppingIngredientDetails)].ingredientDetails.ingredientCode;

        // Ingredient toppingIngredient = GetRandomIngredientFromIngredientDetails(toppingIngredients.ingredientObjects, levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].toppingIngredientCode.Length);
        // productIngredientsCodes[index] = toppingIngredient.IngredientCode;
    }

    // Method to get a random ingredient from a specific array
    private int GetRandomIngredientFromIngredientDetails(UiInventorySlot[] ingredientDetails)
    {
        return Random.Range(0, ingredientDetails.Length);
    }

    // private Ingredient GetRandomIngredientFromIngredientDetails(GameObject[] ingredientArray, int unlockedCount)
    // {
    //     int randomIndex = Random.Range(0, Mathf.Min(unlockedCount, ingredientArray.Length));
    //     return ingredientArray[randomIndex].GetComponent<Ingredient>();
    // }

    private void DisplayOrder()
    {
        GameObject lastIngredient = null;

        float nextPositionY = BubblePos.transform.position.y;
        int sortingOrder = 1;

        foreach (int ingredientCode in productIngredientsCodes)
        {
            UiInventorySlot ingredientDetail = FindIngredientDetail(ingredientCode);

            if (ingredientDetail == null)
            {
                Debug.LogError($"Ingredient detail not found for code: {ingredientCode}");
                continue; // Skip this iteration if the ingredient detail is not found
            }

            // If there was a previous ingredient
            if (lastIngredient != null)
            {
                if (ingredientDetail.ingredientDetails.ingredientType != IngredientType.Topping)
                {
                    nextPositionY = lastIngredient.transform.GetChild(lastIngredient.transform.childCount - 1).position.y;
                }
                else
                {
                    nextPositionY = lastIngredient.transform.position.y;
                }

                sortingOrder++;
            }

            // Instantiate the new ingredient object
            // GameObject ingredientObject = Instantiate(ingredientPrefab, BubblePos.transform);
            ingredientPool.SetParent(BubblePos.transform);

            Ingredient ingredientObject = ingredientPool.ingredientPool.Get();

            lastIngredient = ingredientPool.GetLastIngredient().gameObject;

            ingredientsInPool.Add(ingredientObject);

            Transform nextPosTransform = ingredientObject.transform.GetChild(ingredientObject.transform.childCount - 1);
            nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetail.ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

            // if (lastIngredient != null)
            // {
            //     ingredientObject.transform.parent = lastIngredient.transform;
            // }

            // lastIngredient = ingredientObject.gameObject;

            // Ingredient ingredientScript = ingredientObject.GetComponent<Ingredient>();
            ingredientObject.IngredientCode = ingredientCode;
            ingredientObject.IngredientType = ingredientDetail.ingredientDetails.ingredientType;

            SpriteRenderer spriteRenderer = ingredientObject.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = ingredientDetail.ingredientDetails.dressIngredientSprite;
            spriteRenderer.sortingLayerName = "Customer HUD";
            spriteRenderer.sortingOrder = sortingOrder;

            if (ingredientCode == 1023) // Specific case for ingredient code 1023
            {
                spriteRenderer.sortingOrder -= 2;
            }

            ingredientObject.transform.position = new Vector3(BubblePos.transform.position.x, nextPositionY, BubblePos.transform.position.z);

            // Debug.Log($"Instantiated ingredient {ingredientCode} at position {ingredientObject.transform.position} with sorting order {spriteRenderer.sortingOrder}");
        }

        // // Create a dictionary for quick lookup of ingredients by code
        // Dictionary<int, GameObject> ingredientLookup = new Dictionary<int, GameObject>();
        // foreach (GameObject ingredientObject in allIngredients.ingredientObjects)
        // {
        //     Ingredient ingredient = ingredientObject.GetComponent<Ingredient>();
        //     ingredientLookup[ingredient.IngredientCode] = ingredientObject;
        // }

        // GameObject lastTmpIngredient = null; // Initialize to null to avoid uninitialized usage
        // int tempSortingOrder = 1;
        // foreach (int ingredientCode in productIngredientsCodes)
        // {
        //     if (!ingredientLookup.TryGetValue(ingredientCode, out GameObject tmpIngredient))
        //     {
        //         Debug.LogError($"Ingredient code {ingredientCode} not found.");
        //         continue;
        //     }

        //     Ingredient ingredient = tmpIngredient.GetComponent<Ingredient>();
        //     IngredientType ingredientType = ingredient.IngredientType;

        //     Vector3 spawnPosition = BubblePos.transform.position;
        //     Transform parentTransform = BubblePos.transform;

        //     if (lastTmpIngredient != null)
        //     {
        //         Transform pointTransform = lastTmpIngredient.transform.GetChild(lastTmpIngredient.transform.childCount - 1); // Assuming the point transform is the second child
        //         spawnPosition = pointTransform.position;

        //         if (ingredientType == IngredientType.Topping)
        //         {
        //             spawnPosition = lastTmpIngredient.transform.position;
        //         }

        //         tempSortingOrder++;
        //     }

        //     // Instantiate the ingredient
        //     lastTmpIngredient = Instantiate(tmpIngredient, spawnPosition, Quaternion.identity, parentTransform);

        //     // Set the sorting layer
        //     Transform childlastTmpIngredientTransform = lastTmpIngredient.transform.GetChild(0);
        //     SpriteRenderer childlastTmpIngredientSpriteRenderer = childlastTmpIngredientTransform.GetComponent<SpriteRenderer>();

        //     childlastTmpIngredientSpriteRenderer.sortingLayerName = "Customer HUD";

        //     if (lastTmpIngredient != null)
        //     {
        //         childlastTmpIngredientSpriteRenderer.sortingOrder = tempSortingOrder;
        //     }
        //     else
        //     {
        //         tempSortingOrder = childlastTmpIngredientSpriteRenderer.sortingOrder;
        //     }

        //     if (ingredientCode == 1023) // astor
        //     {
        //         childlastTmpIngredientSpriteRenderer.sortingOrder -= 2;
        //     }
        // }
    }

    private UiInventorySlot FindIngredientDetail(int ingredientCode)
    {
        foreach (UiInventorySlot detail in baseIngredientDetails)
        {
            if ((int)detail.ingredientDetails.ingredientCode == ingredientCode)
                return detail;
        }

        foreach (UiInventorySlot detail in flavorIngredientDetails)
        {
            if ((int)detail.ingredientDetails.ingredientCode == ingredientCode)
                return detail;
        }

        foreach (UiInventorySlot detail in toppingIngredientDetails)
        {
            if ((int)detail.ingredientDetails.ingredientCode == ingredientCode)
                return detail;
        }

        return null;
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
                int unlockedToppingsCount = levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].toppingIngredientCode.Length;
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

        Invoke("DisplayOrder", 0.5f);
        // DisplayOrder();
    }

    public void OrderByRecipe(int recipeUnlockIndex)
    {
        specialRecipeTag.SetActive(true);

        int randomRecipeIndex = Random.Range(0, recipeUnlockIndex);

        int maxOrderByRecipe = levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].recipeList[randomRecipeIndex].ingredientsCodes.Length;

        productIngredientsCodes = new int[maxOrderByRecipe];

        for (int i = 0; i < maxOrderByRecipe; i++)
        {
            productIngredientsCodes[i] = (int)levelDataList.levelDataList[GameManager.Instance.currentLevel - 1].recipeList[randomRecipeIndex].ingredientsCodes[i];
        }

        Invoke("DisplayOrder", 0.5f);
    }

    public void ReleaseAllIngredients()
    {
        // release ingredient pool
        // foreach (Transform child in BubblePos.transform)
        // {
        //     if (child.TryGetComponent<Ingredient>(out Ingredient ingredient))
        //     {
        //         if (!ingredient.GetReleaseFlag())
        //         {
        //             ingredient.GetIngredientPool().Release(ingredient);
        //             ingredient.SetReleaseFlag(true);
        //         }
        //     }
        // }

        foreach (Ingredient ingredient in ingredientsInPool)
        {
            if (!ingredient.GetReleaseFlag())
            {
                ingredient.GetIngredientPool().Release(ingredient);
                ingredient.SetReleaseFlag(true);
            }
        }
    }
}