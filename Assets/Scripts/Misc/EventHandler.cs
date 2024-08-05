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
    
    public static event Action SquishTrashBin;
    public static void CallSquishTrashBinEvent()
    {
        if (SquishTrashBin != null)
            SquishTrashBin();
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
    
    public static event Action<Vector3> SetMoneyPosToCustomerPos;
    public static void CallSetMoneyPosToCustomerPosEvent(Vector3 customerPos)
    {
        if (SetMoneyPosToCustomerPos != null)
            SetMoneyPosToCustomerPos(customerPos);
    }
    
    public static event Action AddMiniGameScore;
    public static void CallAddMiniGameScoreEvent()
    {
        if (AddMiniGameScore != null)
            AddMiniGameScore();
    }
    
    public static event Action BasketFlashEffect;
    public static void CallBasketFlashEffectEvent()
    {
        if (BasketFlashEffect != null)
            BasketFlashEffect();
    }
}