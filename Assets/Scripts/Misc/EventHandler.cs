using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventHandler
{
    public static event Action<InventoryLocation, List<InventoryIngredient>, int> InventoryUpdatedEvent;
    public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, List<InventoryIngredient> inventoryList, int ingredientUnlockedInLocation)
    {
        if (InventoryUpdatedEvent != null)
            InventoryUpdatedEvent(inventoryLocation, inventoryList, ingredientUnlockedInLocation);
    }

    public static event Action CorrectOrder;
    public static void CallCorrectOrderEvent()
    {
        if (CorrectOrder != null)
            CorrectOrder();
    }
    
    public static event Action IncorrectOrder;
    public static void CallIncorrectOrderEvent()
    {
        if (IncorrectOrder != null)
            IncorrectOrder();
    }
    
    public static event Action CloseTrashBin;
    public static void CallCloseTrashBinEvent()
    {
        if (CloseTrashBin != null)
            CloseTrashBin();
    }
    
    public static event Action ResetMainQueue;
    public static void CallResetMainQueueEvent()
    {
        if (ResetMainQueue != null)
            ResetMainQueue();
    }
    
    public static event Action ResetPlatePosition;
    public static void CallResetPlatePositionEvent()
    {
        if (ResetPlatePosition != null)
            ResetPlatePosition();
    }
    
    public static event Action<int> DisableTabButton;
    public static void CallDisableTabButtonEvent(int tabButtonindex)
    {
        if (DisableTabButton != null)
            DisableTabButton(tabButtonindex);
    }
    
    public static event Action<int> EnableTabButton;
    public static void CallEnableTabButtonEvent(int tabButtonindex)
    {
        if (EnableTabButton != null)
            EnableTabButton(tabButtonindex);
    }

    public static event Action ChaseCustomer;
    public static void CallChaseCustomerEvent()
    {
        if (ChaseCustomer != null)
            ChaseCustomer();
    }
}