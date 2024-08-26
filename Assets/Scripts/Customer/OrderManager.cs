using UnityEngine;
using System.Collections.Generic;
using BazarEsKrim;

public class OrderManager : MonoBehaviour
{
    [HideInInspector] public int[] productIngredientsCodes;
    [SerializeField] private GameObject ingredientHolderPos;
    [SerializeField] private GameObject specialRecipeTag;
    // [SerializeField] private float[] holderYPos;
    [SerializeField] private SO_IngredientHolderPos holderYPos;

    private SO_LevelDataList[] levelDataLists;
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
        levelDataLists = GameManager.Instance.levelDataLists;

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

    // Method to add a flavor ingredient to the order
    private void AddFlavorIngredient(int index)
    {
        productIngredientsCodes[index] = (int)flavorIngredientDetails[GetRandomIngredientFromIngredientDetails(flavorIngredientDetails)].ingredientDetails.ingredientCode;
    }

    // Method to add a topping ingredient to the order
    private void AddToppingIngredient(int index)
    {
        productIngredientsCodes[index] = (int)toppingIngredientDetails[GetRandomIngredientFromIngredientDetails(toppingIngredientDetails)].ingredientDetails.ingredientCode;
    }

    // Method to get a random ingredient from a specific array
    private int GetRandomIngredientFromIngredientDetails(UiInventorySlot[] ingredientDetails)
    {
        return Random.Range(0, ingredientDetails.Length);
    }

    private void DisplayOrder()
    {
        GameObject lastIngredient = null;

        float nextPositionY = ingredientHolderPos.transform.position.y;
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
            ingredientPool.SetParent(ingredientHolderPos.transform);

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

            ingredientObject.transform.position = new Vector3(ingredientHolderPos.transform.position.x, nextPositionY, ingredientHolderPos.transform.position.z);

            // Debug.Log($"Instantiated ingredient {ingredientCode} at position {ingredientObject.transform.position} with sorting order {spriteRenderer.sortingOrder}");
        }

        // get base ingredient detail
        UiInventorySlot baseIngredientDetail = FindIngredientDetail(productIngredientsCodes[0]);

        // change the ingredient holder pos in customer bubble
        foreach (IngredientHolderPosInfos holderPosInfos in holderYPos.ingredientHolderPosInfos)
        {
            if (holderPosInfos.coneTypes == baseIngredientDetail.ingredientDetails.coneTypes)
            {
                ingredientHolderPos.transform.localPosition = new Vector3(ingredientHolderPos.transform.localPosition.x, holderPosInfos.holderYPosByHeight[productIngredientsCodes.Length - 1], ingredientHolderPos.transform.localPosition.z);
            }
        }

        // ingredientHolderPos.transform.localPosition = new Vector3(ingredientHolderPos.transform.localPosition.x, holderYPos[productIngredientsCodes.Length - 1], ingredientHolderPos.transform.localPosition.z);
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
        specialRecipeTag.SetActive(false);

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
                int unlockedToppingsCount = levelDataLists[GameManager.Instance.currentLevel - 1].mainGameLevelData.toppingIngredientCode.Count;
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

        Debug.Log("========== START RECIPE ==========");

        foreach (int code in productIngredientsCodes)
        {
            Debug.Log(code);
        }

        Debug.Log("=========== END RECIPE ===========");

        Invoke("DisplayOrder", 0.5f);
        // DisplayOrder();
    }

    public void OrderByRecipe(int recipeUnlockIndex)
    {
        specialRecipeTag.SetActive(true);

        int randomRecipeIndex = Random.Range(0, recipeUnlockIndex);

        int maxOrderByRecipe = levelDataLists[GameManager.Instance.currentLevel - 1].mainGameLevelData.sO_RecipeList[randomRecipeIndex].ingredientsCodes.Count;

        productIngredientsCodes = new int[maxOrderByRecipe];

        Debug.Log("========== START RECIPE ==========");

        for (int i = 0; i < maxOrderByRecipe; i++)
        {
            productIngredientsCodes[i] = (int)levelDataLists[GameManager.Instance.currentLevel - 1].mainGameLevelData.sO_RecipeList[randomRecipeIndex].ingredientsCodes[i];

            Debug.Log(productIngredientsCodes[i]);
        }

        Debug.Log("=========== END RECIPE ===========");

        Invoke("DisplayOrder", 0.5f);
    }

    public void ReleaseAllIngredients()
    {
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
