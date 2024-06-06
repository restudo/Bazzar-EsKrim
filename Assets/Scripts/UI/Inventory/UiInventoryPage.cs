using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiInventoryPage : MonoBehaviour
{
    [HideInInspector] public ScrollController scrollController;

    [SerializeField] private InventoryLocation _inventoryLocation;
    [SerializeField] private Sprite blankSprite;
    [SerializeField] private Image imageBlocker;
    [SerializeField] private UiInventorySlot[] inventorySlot = null;

    private ScrollRect scrollRect;

    private void Awake()
    {
        Transform grandparentTransform = GetGrandparentTransform(transform);
        if (grandparentTransform != null)
        {
            scrollRect = grandparentTransform.GetComponent<ScrollRect>();
            scrollController = grandparentTransform.GetComponent<ScrollController>();
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
            }
            
            if(scrollController != null)
            {
                scrollController.isScrollActive = false;
            }
        }

        if (_inventoryLocation == InventoryLocation.Base)
        {
            imageBlocker.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
        EventHandler.EnableTabButton += EnableTabButton;
        EventHandler.DisableTabButton += DisableTabButton;
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
        EventHandler.EnableTabButton -= EnableTabButton;
        EventHandler.DisableTabButton -= DisableTabButton;
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        InventoryUpdated(_inventoryLocation, InventoryManager.Instance.inventoryLists[(int)_inventoryLocation], CheckInventoryLocation());
    }

    private Transform GetGrandparentTransform(Transform child)
    {
        if (child.parent != null && child.parent.parent != null)
        {
            return child.parent.parent;
        }

        return null;
    }

    private int CheckInventoryLocation()
    {
        switch (_inventoryLocation)
        {
            case InventoryLocation.Base:
                return GameManager.Instance.GetBaseUnlock();
            case InventoryLocation.Flavor:
                return GameManager.Instance.GetFlavorUnlock();
            case InventoryLocation.Topping:
                return GameManager.Instance.GetToppingUnlock();
            default:
                break;
        }

        return -1;
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryIngredient> inventoryList, int ingredientUnlockedInLocation)
    {
        if (inventoryLocation == _inventoryLocation)
        {
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
            imageBlocker.gameObject.SetActive(false);

            if(_inventoryLocation == InventoryLocation.Topping)
            {
                scrollRect.enabled = true;
                scrollController.isScrollActive = true;
            }
        }
    }

    private void DisableTabButton(int index)
    {
        if (index == (int)_inventoryLocation)
        {
            imageBlocker.gameObject.SetActive(true);

            if(_inventoryLocation == InventoryLocation.Topping)
            {
                scrollRect.enabled = false;
                scrollController.isScrollActive = false;
            }
        }
    }
}
