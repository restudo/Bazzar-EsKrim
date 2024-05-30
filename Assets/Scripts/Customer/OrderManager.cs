using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private GameObject[] baseIngredients;
    [SerializeField] private GameObject[] flavorIngredients;
    [SerializeField] private GameObject[] toppingIngredients;
    [SerializeField] private int maxOrderSize = 6;

    [HideInInspector] public int[] productIngredientsCodes;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OrderRandomProduct();
        }
    }

    // Main method to add ingredients to the order
    public void OrderRandomProduct()
    {
        int randomMaxOrder = Random.Range(2, maxOrderSize);
        productIngredientsCodes = new int[randomMaxOrder];

        Debug.Log(randomMaxOrder);

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
                int unlockedToppingsCount = GameManager.Instance.GetToppingUnlock();
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
    }

    // Method to add a base ingredient to the order
    private void AddBaseIngredient()
    {
        Ingredient baseIngredient = GetRandomIngredientFromArray(baseIngredients, GameManager.Instance.GetBaseUnlock());
        productIngredientsCodes[0] = baseIngredient.IngredientCode;
        Debug.Log(productIngredientsCodes[0]);
    }

    // Method to add a flavor ingredient to the order
    private void AddFlavorIngredient(int index)
    {
        Ingredient flavorIngredient = GetRandomIngredientFromArray(flavorIngredients, GameManager.Instance.GetFlavorUnlock());
        productIngredientsCodes[index] = flavorIngredient.IngredientCode;
        Debug.Log(productIngredientsCodes[index]);
    }

    // Method to add a topping ingredient to the order
    private void AddToppingIngredient(int index)
    {
        Ingredient toppingIngredient = GetRandomIngredientFromArray(toppingIngredients, GameManager.Instance.GetToppingUnlock());
        productIngredientsCodes[index] = toppingIngredient.IngredientCode;
        Debug.Log(productIngredientsCodes[index]);
    }

    // Method to get a random ingredient from a specific array
    private Ingredient GetRandomIngredientFromArray(GameObject[] ingredientArray, int unlockedCount)
    {
        int randomIndex = Random.Range(0, Mathf.Min(unlockedCount, ingredientArray.Length));
        return ingredientArray[randomIndex].GetComponent<Ingredient>();
    }
}