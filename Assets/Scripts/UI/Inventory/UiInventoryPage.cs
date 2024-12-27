using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiInventoryPage : MonoBehaviour
{
    [SerializeField] private Sprite blankSprite;
    [SerializeField] private UiInventorySlot[] inventorySlot = null;
    [SerializeField] private InventoryLocation _inventoryLocation;
    [SerializeField] private Image glowVisual;
    private SO_LevelDataList[] levelDataIngredientCodes;
    private int[] ingredientCodeRecipe;

    private void Awake()
    {
        levelDataIngredientCodes = GameManager.Instance.levelDataLists;
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
            glowVisual.gameObject.SetActive(true);

            foreach (var item in inventorySlot)
            {
                item.inventorySlotImageBlocker.gameObject.SetActive(false);
            }
        }

        yield return new WaitForEndOfFrame();

        InventoryUpdated(_inventoryLocation);
    }

    private int[] CheckInventoryLocation()
    {
        List<IngredientName> ingredientNames = new List<IngredientName>();

        switch (_inventoryLocation)
        {
            case InventoryLocation.Base:
                ingredientNames = levelDataIngredientCodes[GameManager.Instance.currentLevel - 1].mainGameLevelData.baseIngredientCode;
                break;
            case InventoryLocation.Flavor:
                ingredientNames = levelDataIngredientCodes[GameManager.Instance.currentLevel - 1].mainGameLevelData.flavorIngredientCode;
                break;
            case InventoryLocation.Topping:
                ingredientNames = levelDataIngredientCodes[GameManager.Instance.currentLevel - 1].mainGameLevelData.toppingIngredientCode;
                break;
            default:
                ingredientNames = levelDataIngredientCodes[GameManager.Instance.currentLevel - 1].mainGameLevelData.baseIngredientCode;
                break;
        }

        // Convert IngredientName[] to int[]
        int[] ingredientCodes = new int[ingredientNames.Count];
        for (int i = 0; i < ingredientNames.Count; i++)
        {
            ingredientCodes[i] = (int)ingredientNames[i];
        }

        return ingredientCodes;
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

                        inventorySlot[i].inventorySlotImageBlocker.sprite = inventorySlot[i].BlockerRef.sprite;
                        inventorySlot[i].inventorySlotImageBlocker.transform.localScale = inventorySlot[i].BlockerRef.transform.localScale;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Inventory Slot is empty!");
            }
        }
    }

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
            glowVisual.gameObject.SetActive(true);

            // loop through inventory slots and deactivate blocker
            foreach (var item in inventorySlot)
            {
                item.inventorySlotImageBlocker.gameObject.SetActive(false);
            }
        }
    }

    private void DisableTabButton(int index)
    {
        if (index == (int)_inventoryLocation)
        {
            glowVisual.gameObject.SetActive(false);

            // loop through inventory slots and activate blocker
            foreach (var item in inventorySlot)
            {
                item.inventorySlotImageBlocker.gameObject.SetActive(true);
            }
        }
    }
}
