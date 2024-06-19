using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image inventorySlotImage;
    [HideInInspector] public IngredientDetails ingredientDetails;

    [SerializeField] private GameObject ingredientPrefab = null;
    [SerializeField] private ScrollRect scrollRect;
    // [SerializeField] private ScrollController scrollController;
    
    private Camera mainCamera;
    private Transform parentIngredient;
    private IngredientHolder ingredientHolder;
    private LevelManager levelManager;
    private GameObject draggedIngredient;
    private Vector3 worldPosition;
    // private bool isPointerOverUI;
    // private bool isDragging;
    // private bool isScrolling;
    // private float pointerDownTime;
    // private const float dragThreshold = 0.2f; // Adjust this value as needed
    // private Vector2 dragStartPosition;
    // private Vector2 contentStartPosition;

    private void Start()
    {
        // isDragging = false;
        // isPointerOverUI = false;
        // isScrolling = false;

        mainCamera = Camera.main;
        parentIngredient = GameObject.FindGameObjectWithTag("Ingredient Holder").transform;

        ingredientHolder = parentIngredient.gameObject.GetComponent<IngredientHolder>();
        ingredientHolder.canDeliverOrder = false;

        levelManager = FindObjectOfType<LevelManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredientDetails == null || !GameManager.Instance.isGameActive)
        {
            return;
        }

        // isDragging = true; // Mark that a drag has started

        EventHandler.CallCloseTrashBinEvent();

        // Check if the input is from a touch device or mouse
        bool isSingleFingerTouch = Input.touchCount == 1 && eventData.pointerId >= 0;
        bool isLeftMouseClick = eventData.pointerId == -1; // -1 is the pointer ID for the left mouse button

        if (isSingleFingerTouch || isLeftMouseClick)
        {
            // Instantiate the ingredient object
            draggedIngredient = Instantiate(InventoryManager.Instance.inventoryDraggedIngredient, eventData.position, Quaternion.identity, transform.parent);

            Canvas draggedCanvas = draggedIngredient.GetComponent<Canvas>();
            if (draggedCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                draggedCanvas.worldCamera = mainCamera;
                draggedCanvas.sortingLayerName = "Render On Top";
            }

            // Get the ingredient image
            Image draggedIngredientImage = draggedIngredient.GetComponentInChildren<Image>();
            draggedIngredientImage.sprite = ingredientDetails.dressIngredientSprite;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move the ingredient
        if (draggedIngredient != null)
        {
            // Convert screen position to world position in the context of the RectTransform's parent
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggedIngredient.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out worldPosition))
            {
                draggedIngredient.GetComponent<RectTransform>().position = worldPosition;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // GameObject ingredientHolderObj = GameObject.FindGameObjectWithTag("Ingredient Holder");
        // IngredientHolder ingredientHolder = parentIngredient.gameObject.GetComponent<IngredientHolder>();

        if (draggedIngredient != null)
        {
            Destroy(draggedIngredient);

            if (levelManager.deliveryQueueIngredient < levelManager.maxOrderHeight)
            {
                // levelManager.deliveryQueueIsFull = true;
                HandleIngredientDrop(parentIngredient.gameObject, levelManager);
            }

            ingredientHolder.canDeliverOrder = true;

            // else
            // {
            //     levelManager.deliveryQueueIsFull = false;
            // }
        }

        // isDragging = false; // Reset the drag flag
    }

    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);
    //     isDragging = false;
    //     isScrolling = false;
    //     pointerDownTime = Time.time;
    //     dragStartPosition = eventData.position;
    //     if (scrollRect != null && scrollRect.enabled)
    //     {
    //         contentStartPosition = scrollRect.content.anchoredPosition;
    //     }
    // }

    // public void OnPointerUp(PointerEventData eventData)
    // {
    //     if (!isPointerOverUI || ingredientDetails == null || !GameManager.Instance.isGameActive || isDragging || isScrolling)
    //     {
    //         isPointerOverUI = false;
    //         return;
    //     }

    //     bool isSingleFingerTouch = Input.touchCount == 1 && eventData.pointerId >= 0;
    //     bool isLeftMouseClick = eventData.pointerId == -1;

    //     if (isSingleFingerTouch || isLeftMouseClick)
    //     {
    //         if (Time.time - pointerDownTime <= dragThreshold)
    //         {
    //             if (levelManager.deliveryQueueIngredient < levelManager.maxOrderHeight)
    //             {
    //                 AddIngredientDirectlyToHolder(parentIngredient.gameObject, levelManager);
    //             }

    //             ingredientHolder.canDeliverOrder = true;
    //         }
    //     }

    //     isPointerOverUI = false;
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     isPointerOverUI = false;
    // }

    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     isDragging = true;
    //     isScrolling = false;
    // }

    // public void OnDrag(PointerEventData eventData)
    // {
    //     if (isDragging && scrollRect != null && scrollRect.enabled)
    //     {
    //         Vector2 dragDelta = eventData.position - dragStartPosition;
    //         dragDelta.y = 0; // Lock vertical movement
    //         scrollRect.content.anchoredPosition = contentStartPosition + dragDelta;
    //         isScrolling = true;
    //     }
    // }

    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     isDragging = false;
    //     isScrolling = false;
    // }

    private void HandleIngredientDrop(GameObject ingredientHolderObj, LevelManager levelManager)
    {
        if (ingredientHolderObj != null && levelManager != null)
        {
            BoxCollider2D plateCollider = ingredientHolderObj.GetComponent<BoxCollider2D>();
            if (plateCollider != null && IsMouseOverCollider(plateCollider))
            {
                TryAddIngredientToPlate(ingredientHolderObj, levelManager);
            }
        }
        else
        {
            Debug.Log("Either plate gameobject or levelmanager script isn't available in this scene");
        }
    }

    private bool IsMouseOverCollider(BoxCollider2D collider)
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        return collider.bounds.Contains(mousePos);
    }

    private void AddIngredientDirectlyToHolder(GameObject ingredientHolderObj, LevelManager levelManager)
    {
        if (ingredientHolderObj != null && levelManager != null)
        {
            TryAddIngredientToPlate(ingredientHolderObj, levelManager);
        }
    }

    private void TryAddIngredientToPlate(GameObject ingredientHolderObj, LevelManager levelManager)
    {
        if (ingredientDetails != null)
        {
            Transform lastIngredient = GetLastIngredient(ingredientHolderObj);
            if (IsInvalidIngredientPlacement(ingredientHolderObj, lastIngredient))
            {
                return;
            }

            float nextPositionY = CalculateNextPositionY(ingredientHolderObj, lastIngredient);
            CreateAndPlaceIngredient(ingredientHolderObj, nextPositionY, levelManager, lastIngredient);
        }
    }

    private Transform GetLastIngredient(GameObject ingredientHolderObj)
    {
        return ingredientHolderObj.transform.childCount > 0 ? ingredientHolderObj.transform.GetChild(ingredientHolderObj.transform.childCount - 1) : null;
    }

    private bool IsInvalidIngredientPlacement(GameObject ingredientHolderObj, Transform lastIngredient)
    {
        if (lastIngredient == null && ingredientDetails.ingredientType != IngredientType.Base)
        {
            return true;
        }

        if (lastIngredient != null && lastIngredient.childCount > 0)
        {
            if (lastIngredient.GetComponent<Ingredient>().IngredientType == IngredientType.Base && ingredientDetails.ingredientType == IngredientType.Topping)
            {
                return true;
            }

            if (lastIngredient.GetComponent<Ingredient>().IngredientType == IngredientType.Topping)
            {
                return true;
            }

            foreach (Transform child in ingredientHolderObj.transform)
            {
                if (child.GetComponent<Ingredient>().IngredientType == IngredientType.Base && ingredientDetails.ingredientType == IngredientType.Base)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private float CalculateNextPositionY(GameObject ingredientHolderObj, Transform lastIngredient)
    {
        float nextPositionY = ingredientHolderObj.transform.position.y;
        if (lastIngredient != null)
        {
            if (ingredientDetails.ingredientType != IngredientType.Topping)
            {
                nextPositionY = lastIngredient.GetChild(lastIngredient.childCount - 1).position.y;
            }
            else
            {
                nextPositionY = lastIngredient.position.y;
            }
        }

        return nextPositionY;
    }

    private void CreateAndPlaceIngredient(GameObject ingredientHolderObj, float nextPositionY, LevelManager levelManager, Transform lastIngredient)
    {
        Vector3 platePosition = ingredientHolderObj.transform.position;
        GameObject ingredientGameobject = Instantiate(ingredientPrefab, platePosition, Quaternion.identity, parentIngredient);

        Ingredient ingredient = ingredientGameobject.GetComponent<Ingredient>();
        ingredient.IngredientCode = (int)ingredientDetails.ingredientCode;
        ingredient.IngredientType = ingredientDetails.ingredientType;

        SpriteRenderer spriteRenderer = ingredientGameobject.GetComponentInChildren<SpriteRenderer>();
        if (ingredientDetails.ingredientType == IngredientType.Topping)
        {
            spriteRenderer.sprite = ingredientDetails.plateIngredientSprite;
        }
        else
        {
            spriteRenderer.sprite = ingredientDetails.dressIngredientSprite;
        }

        spriteRenderer.sortingLayerName = "Ingredient Holder";
        spriteRenderer.sortingOrder = 1;

        if (lastIngredient != null)
        {
            SpriteRenderer lastIngredientSprite = lastIngredient.GetComponentInChildren<SpriteRenderer>();

            if (lastIngredientSprite != null)
            {
                int temp = lastIngredientSprite.sortingOrder + 1;
                spriteRenderer.sortingOrder = temp;
            }
        }

        ingredientGameobject.transform.position = new Vector3(ingredientHolderObj.transform.position.x, nextPositionY, ingredientHolderObj.transform.position.z);

        levelManager.deliveryQueueIngredient++;
        levelManager.deliveryQueueIngredientsContent.Add(ingredient.IngredientCode);

        SetButtonLogic(ingredient.IngredientType);
    }

    private void SetButtonLogic(IngredientType ingredientType)
    {
        if (ingredientType == IngredientType.Base)
        {
            EventHandler.CallDisableTabButtonEvent((int)IngredientType.Base);
            EventHandler.CallDisableTabButtonEvent((int)IngredientType.Topping);

            EventHandler.CallEnableTabButtonEvent((int)IngredientType.Flavor);
        }

        if (ingredientType == IngredientType.Flavor)
        {
            EventHandler.CallEnableTabButtonEvent((int)IngredientType.Topping);
        }
    }
}
