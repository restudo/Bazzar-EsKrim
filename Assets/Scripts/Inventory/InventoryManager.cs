using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private SO_IngredientList ingredientList = null;
    private Dictionary<int, IngredientDetails> ingredientDetailsDictionary;
    public List<InventoryIngredient>[] inventoryLists;
    public GameObject inventoryDraggedIngredient;
    [HideInInspector] public int[] inventoryListCapacityIntArray;   // the index of the array is the inventory lis
                                                                    // (from the InventoryLocation enum), 
                                                                    // and the value is the capacity of that inventory list

    public static InventoryManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Create Inventory Lists
        CreateInventoryLists();

        CreateIngredientDetailsDictionary();

        AddIngredientToInventoryList();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     foreach (InventoryLocation location in InventoryLocation.GetValues(typeof(InventoryLocation)))
        //     {
        //         if (location != InventoryLocation.count)
        //         {
        //             foreach (InventoryIngredient inventoryIngredient in inventoryLists[(int)location])
        //             {
        //                 Debug.Log("Location: " + location.ToString() + "    --    Name: " + GetIngredientDetails(inventoryIngredient.ingredientCode).ingredientName);
        //             }
        //         }
        //     }
        // }
    }

    private void CreateInventoryLists()
    {
        // initialise inventory list capacity array
        inventoryLists = new List<InventoryIngredient>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryIngredient>();
        }

        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        inventoryListCapacityIntArray[(int)InventoryLocation.Base] = 10; // max inventory
    }

    private void AddIngredientToInventoryList()
    {
        foreach (IngredientDetails ingredientDetails in ingredientList.ingredientDetails)
        {
            switch (ingredientDetails.ingredientType)
            {
                case IngredientType.Base:
                    AddItem(InventoryLocation.Base, (int)ingredientDetails.ingredientCode);
                    break;
                case IngredientType.Flavor:
                    AddItem(InventoryLocation.Flavor, (int)ingredientDetails.ingredientCode);
                    break;
                case IngredientType.Topping:
                    AddItem(InventoryLocation.Topping, (int)ingredientDetails.ingredientCode);
                    break;
                default:
                    // Handle any other cases if needed
                    break;
            }
        }
    }

    /// <summary>
    /// Populate the ingredientDetailsDictionary from the SO_ingredientList
    /// </summary>
    private void CreateIngredientDetailsDictionary()
    {
        ingredientDetailsDictionary = new Dictionary<int, IngredientDetails>();

        foreach (IngredientDetails ingredientDetails in ingredientList.ingredientDetails)
        {
            ingredientDetailsDictionary.Add((int)ingredientDetails.ingredientCode, ingredientDetails);
            // Debug.Log(ingredientDetails.ingredientCode);
        }
    }

    /// <summary>
    /// Add an ingredient to the inventory list for the inventoryLocation
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="ingredient"></param>
    public void AddItem(InventoryLocation inventoryLocation, int ingredientCode)
    {
        List<InventoryIngredient> inventoryList = inventoryLists[(int)inventoryLocation];

        AddItemAtPosition(inventoryList, ingredientCode);
    }

    private void AddItemAtPosition(List<InventoryIngredient> inventoryList, int ingredientCode)
    {
        InventoryIngredient InventoryIngredient = new InventoryIngredient();

        InventoryIngredient.ingredientCode = ingredientCode;
        inventoryList.Add(InventoryIngredient);


    }

    /// <summary>
    /// Return the ingredientDetails from SO_IngredientList for the ingredient code, or null if item code doesn't exist
    /// </summary>
    /// <param name="ingredientCode"></param>
    /// <returns></returns>
    public IngredientDetails GetIngredientDetails(int ingredientCode)
    {
        IngredientDetails ingredientDetails;

        if (ingredientDetailsDictionary.TryGetValue(ingredientCode, out ingredientDetails))
        {
            return ingredientDetails;
        }
        else
        {
            return null;
        }

    }

    private void DebugPrintInventoryList(List<InventoryIngredient> inventoryList)
    {
        foreach (InventoryIngredient inventoryIngredient in inventoryList)
        {
            Debug.Log("Ingredient Name: " + GetIngredientDetails(inventoryIngredient.ingredientCode));
        }
    }
}
