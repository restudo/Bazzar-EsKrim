using System.Collections.Generic;
using UnityEngine;

public class UiInventoryPage : MonoBehaviour
{
    [SerializeField] private Sprite blankSprite;
    [SerializeField] private UiInventorySlot[] inventorySlot = null;

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryIngredient> inventoryList, int ingredientUnlockedInLocation)
    {
        // if (inventoryLocation == InventoryLocation.Base)
        // {
            ClearInventorySlots();

            if (inventorySlot.Length > 0 && inventoryList.Count > 0)
            {
                // loop through inventory slots and update with corresponding inventory list item
                for (int i = 0; i < inventorySlot.Length; i++)
                {
                    if (ingredientUnlockedInLocation > 0 && i < inventoryList.Count)
                    {
                        int ingredientCode = inventoryList[i].ingredientCode;

                        // ItemDetails itemDetails = InventoryManager.Instance.itemList.itemDetails.Find(x => x.itemCode == itemCode);
                        IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCode);

                        if (ingredientDetails != null)
                        {
                            // add images and details to inventory item slot
                            inventorySlot[i].inventorySlotImage.sprite = ingredientDetails.basketIngredientSprite;
                            inventorySlot[i].ingredientDetails = ingredientDetails;

                            ingredientUnlockedInLocation--;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        // }
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
}
