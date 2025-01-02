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

    public static event Action<bool> CorrectOrder;
    public static void CallCorrectOrderEvent(bool isRecipeOrder)
    {
        if (CorrectOrder != null)
            CorrectOrder(isRecipeOrder);
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
    
    public static event Action PlaySfxTrashBin;
    public static void CallPlaySfxTrashBinEvent()
    {
        if (PlaySfxTrashBin != null)
            PlaySfxTrashBin();
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
    
    public static event Action<Vector3, bool> SetMoneyPosToCustomerPos;
    public static void CallSetMoneyPosToCustomerPosEvent(Vector3 customerPos, bool isRecipeOrder)
    {
        if (SetMoneyPosToCustomerPos != null)
            SetMoneyPosToCustomerPos(customerPos, isRecipeOrder);
    }
    
    public static event Action AddMiniGameScore;
    public static void CallAddMiniGameScoreEvent()
    {
        if (AddMiniGameScore != null)
            AddMiniGameScore();
    }
    
    public static event Action<float> BasketFlashEffect;
    public static void CallBasketFlashEffectEvent(float xPos)
    {
        if (BasketFlashEffect != null)
            BasketFlashEffect(xPos);
    }
    
    public static event Action<Vector3, Vector3> SetPointCounterPos;
    public static void CallSetPointCounterPosEvent(Vector3 fallingObjPos, Vector3 basketPos)
    {
        if (SetPointCounterPos != null)
            SetPointCounterPos(fallingObjPos, basketPos);
    }
    
    public static event Action<float> CameraMove;
    public static void CallCameraMoveEvent(float delta)
    {
        if (CameraMove != null)
            CameraMove(delta);
    }
    
    public static event Action<int> MiniGameScoreTier;
    public static void CallMiniGameScoreTierEvent(int scoreTier)
    {
        if (MiniGameScoreTier != null)
            MiniGameScoreTier(scoreTier);
    }
    
    public static event Action MiniGameWin;
    public static void CallminiGameWinEvent()
    {
        if (MiniGameWin != null)
            MiniGameWin();
    }
    
    public static event Action<int> LoadToLevel;
    public static void CallLoadToLevelEvent(int lvIndex)
    {
        if (LoadToLevel != null)
            LoadToLevel(lvIndex);
    }
    
    public static event Action TogglePause;
    public static void CallTogglePauseEvent()
    {
        if (TogglePause != null)
            TogglePause();
    }
}