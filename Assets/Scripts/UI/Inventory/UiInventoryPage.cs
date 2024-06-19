using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiInventoryPage : MonoBehaviour
{
    // [HideInInspector] public ScrollController scrollController;

    [SerializeField] private Sprite blankSprite;
    [SerializeField] private UiInventorySlot[] inventorySlot = null;
    [SerializeField] private InventoryLocation _inventoryLocation;
    [SerializeField] private Image imageBlocker;
    [SerializeField] private SO_LevelDataList levelDataIngredientCodes;
    private int[] ingredientCodeRecipe;

    // private ScrollRect scrollRect;

    private void Awake()
    {
        // Transform grandparentTransform = GetGrandparentTransform(transform);
        // if (grandparentTransform != null)
        // {
        //     scrollRect = grandparentTransform.GetComponent<ScrollRect>();
        //     scrollController = grandparentTransform.GetComponent<ScrollController>();
        //     if (scrollRect != null)
        //     {
        //         scrollRect.enabled = false;
        //     }

        //     if(scrollController != null)
        //     {
        //         scrollController.isScrollActive = false;
        //     }
        // }
    }

    private void OnEnable()
    {
        // EventHandler.InventoryUpdatedEvent += InventoryUpdated;
        EventHandler.EnableTabButton += EnableTabButton;
        EventHandler.DisableTabButton += DisableTabButton;
    }

    private void OnDisable()
    {
        // EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
        EventHandler.EnableTabButton -= EnableTabButton;
        EventHandler.DisableTabButton -= DisableTabButton;
    }

    private IEnumerator Start()
    {
        ingredientCodeRecipe = CheckInventoryLocation();

        if (_inventoryLocation == InventoryLocation.Base)
        {
            imageBlocker.gameObject.SetActive(false);
        }

        yield return new WaitForEndOfFrame();

        InventoryUpdated(_inventoryLocation);
        // InventoryUpdated(_inventoryLocation, InventoryManager.Instance.inventoryLists[(int)_inventoryLocation], CheckInventoryLocation());
    }

    // private Transform GetGrandparentTransform(Transform child)
    // {
    //     if (child.parent != null && child.parent.parent != null)
    //     {
    //         return child.parent.parent;
    //     }

    //     return null;
    // }

    private int[] CheckInventoryLocation()
    {
        switch (_inventoryLocation)
        {
            case InventoryLocation.Base:
                return levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].baseIngredientCode;
            case InventoryLocation.Flavor:
                return levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].flavorIngredientCode;
            case InventoryLocation.Topping:
                return levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].toppingIngredientCode;
            default:
                break;
        }

        return levelDataIngredientCodes.levelDataList[GameManager.Instance.currentLevel - 1].baseIngredientCode;
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation)
    {
        if (inventoryLocation == _inventoryLocation)
        {
            ClearInventorySlots();

            if (inventorySlot.Length > 0)
            {
                // loop through inventory slots and update with corresponding inventory list item
                for (int i = 0; i < inventorySlot.Length; i++)
                {
                    int ingredientCode = ingredientCodeRecipe[i];

                    IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCode);

                    if (ingredientDetails != null)
                    {
                        // add images and details to inventory item slot
                        inventorySlot[i].inventorySlotImage.sprite = ingredientDetails.basketIngredientSprite;
                        inventorySlot[i].ingredientDetails = ingredientDetails;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Inventory Slot is empty!");
            }
        }
    }

    // private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryIngredient> inventoryList, int ingredientUnlockedInLocation)
    // {
    //     if (inventoryLocation == _inventoryLocation)
    //     {
    //         ClearInventorySlots();

    //         if (inventorySlot.Length > 0 && inventoryList.Count > 0)
    //         {
    //             // loop through inventory slots and update with corresponding inventory list item
    //             for (int i = 0; i < inventorySlot.Length; i++)
    //             {
    //                 if (ingredientUnlockedInLocation > 0 && i < inventoryList.Count)
    //                 {
    //                     int ingredientCode = inventoryList[i].ingredientCode;

    //                     // ItemDetails itemDetails = InventoryManager.Instance.itemList.itemDetails.Find(x => x.itemCode == itemCode);
    //                     IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCode);

    //                     if (ingredientDetails != null)
    //                     {
    //                         // add images and details to inventory item slot
    //                         inventorySlot[i].inventorySlotImage.sprite = ingredientDetails.basketIngredientSprite;
    //                         inventorySlot[i].ingredientDetails = ingredientDetails;

    //                         ingredientUnlockedInLocation--;
    //                     }
    //                 }
    //                 else
    //                 {
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }

    private void ClearInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            // loop through inventory slots and update with blank sprite
            for (int i = 0; i < inventorySlot.Length; i++)

            {
                inventorySlot[i].inventorySlotImage.sprite = blankSprite;
                inventorySlot[i].ingredientDetails = null;
            }
        }
    }

    private void EnableTabButton(int index)
    {
        if (index == (int)_inventoryLocation)
        {
            imageBlocker.gameObject.SetActive(false);

            // if(index == (int)_inventoryLocation)
            // {
            // scrollRect.enabled = true;
            // scrollController.isScrollActive = true;
            // }
        }
    }

    private void DisableTabButton(int index)
    {
        if (index == (int)_inventoryLocation)
        {
            imageBlocker.gameObject.SetActive(true);

            // if(index == (int)_inventoryLocation)
            // {
            // scrollRect.enabled = false;
            // scrollController.isScrollActive = false;
            // }
        }
    }
}
