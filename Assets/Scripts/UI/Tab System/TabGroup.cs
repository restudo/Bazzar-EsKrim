using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private List<TabButton> tabButtons;
    private TabButton selectedTab;

    private void OnEnable()
    {
        EventHandler.EnableTabButton += EnableTabButton;
        EventHandler.DisableTabButton += DisableTabButton;
    }

    private void OnDisable()
    {
        EventHandler.EnableTabButton -= EnableTabButton;
        EventHandler.DisableTabButton -= DisableTabButton;
    }

    private void Start()
    {
        for (int i = 1; i <= 2; i++)
        {
            DisableTabButton(i);
        }

        OnTabSelected(tabButtons[0]);
    }

    // public void Subscribe(TabButton button)
    // {
    //     if (tabButtons == null)
    //     {
    //         tabButtons = new List<TabButton>();
    //     }

    //     tabButtons.Add(button);
    // }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;

        ResetTab();
        EventHandler.CallCloseTrashBinEvent();

        // do something
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < pages.Count; i++)
        {
            if (i == index)
            {
                pages[i].SetActive(true);
            }
            else
            {
                pages[i].SetActive(false);
            }
        }

        InventoryLocation inventoryLocation = InventoryLocation.Base;
        int ingredientUnlockedInLocation = GameManager.Instance.GetBaseUnlock();
        if (button.transform.CompareTag(InventoryLocation.Flavor.ToString()))
        {
            inventoryLocation = InventoryLocation.Flavor;
            ingredientUnlockedInLocation = GameManager.Instance.GetFlavorUnlock();
        }
        else if (button.transform.CompareTag(InventoryLocation.Topping.ToString()))
        {
            inventoryLocation = InventoryLocation.Topping;
            ingredientUnlockedInLocation = GameManager.Instance.GetToppingUnlock();
        }

        List<InventoryIngredient> inventoryList = InventoryManager.Instance.inventoryLists[(int)inventoryLocation];
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList, ingredientUnlockedInLocation);
        // Debug.Log(inventoryLocation);
    }

    // reset every selected tab
    private void ResetTab()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }

            // do something
        }

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
    }

    private void EnableTabButton(int index)
    {
        tabButtons[index].GetComponent<Button>().interactable = true;

        if (index == 0)
        {
            OnTabSelected(tabButtons[index]);
        }

        // select flavor
        if (index == 1)
        {
            OnTabSelected(tabButtons[index]);
        }
    }

    private void DisableTabButton(int index)
    {
        tabButtons[index].GetComponent<Button>().interactable = false;
    }
}
