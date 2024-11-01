using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UiInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image inventorySlotImage;
    public Image inventorySlotImageBlocker;
    public Image BlockerRef;
    [HideInInspector] public IngredientDetails ingredientDetails;

    [SerializeField] private GameObject draggedIngredient;
    // [SerializeField] private GameObject ingredientPrefab = null;
    [SerializeField] private IngredientPool ingredientPool;
    [SerializeField] private GameObject glowingPlate;
    // [SerializeField] private ScrollRect scrollRect;
    // [SerializeField] private ScrollController scrollController;

    private Camera mainCamera;
    private Transform parentIngredient;
    private IngredientHolder ingredientHolder;
    private MainGameController mainGameController;
    private Vector3 worldPosition;
    private const float flewDuration = 0.3f;
    // private GameObject draggedIngredient;
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

        mainGameController = FindObjectOfType<MainGameController>();

        glowingPlate.SetActive(false);
        draggedIngredient.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredientDetails == null || !GameManager.Instance.isGameActive || GameManager.Instance.gameStates != GameStates.MainGame || inventorySlotImageBlocker.gameObject.activeSelf)
        {
            return;
        }

        // isDragging = true; // Mark that a drag has started

        ingredientHolder.canDeliverOrder = false;

        EventHandler.CallCloseTrashBinEvent();

        glowingPlate.SetActive(true);

        // Check if the input is from a touch device or mouse
        bool isSingleFingerTouch = Input.touchCount == 1 && eventData.pointerId >= 0;
        bool isLeftMouseClick = eventData.pointerId == -1; // -1 is the pointer ID for the left mouse button

        if (isSingleFingerTouch || isLeftMouseClick)
        {
            // Instantiate the ingredient object
            // draggedIngredient = Instantiate(InventoryManager.Instance.inventoryDraggedIngredient, eventData.position, Quaternion.identity, transform.parent);
            draggedIngredient.SetActive(true);

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
            // Destroy(draggedIngredient);
            // draggedIngredient.SetActive(false);

            // levelManager.deliveryQueueIsFull = true;
            HandleIngredientDrop(parentIngredient.gameObject, mainGameController);

            glowingPlate.SetActive(false);

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

    private void HandleIngredientDrop(GameObject ingredientHolderObj, MainGameController mainGameController)
    {
        if (ingredientHolderObj != null && mainGameController != null)
        {
            BoxCollider2D plateCollider = ingredientHolderObj.GetComponent<BoxCollider2D>();
            if (plateCollider != null && IsMouseOverCollider(plateCollider))
            {
                TryAddIngredientToPlate(ingredientHolderObj, mainGameController);
            }
            else
            {
                DraggedIngredientFlewAnim();
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

    // private void AddIngredientDirectlyToHolder(GameObject ingredientHolderObj, MainGameController mainGameController)
    // {
    //     if (ingredientHolderObj != null && mainGameController != null)
    //     {
    //         TryAddIngredientToPlate(ingredientHolderObj, mainGameController);
    //     }
    // }

    private void TryAddIngredientToPlate(GameObject ingredientHolderObj, MainGameController mainGameController)
    {
        if (ingredientDetails != null)
        {
            // Transform lastIngredient = GetLastIngredient(ingredientHolderObj);
            Transform lastIngredient = ingredientPool.GetLastIngredient();
            if (IsInvalidIngredientPlacement(ingredientHolderObj, lastIngredient))
            {
                DraggedIngredientFlewAnim();

                return;
            }

            float nextPositionY = CalculateNextPositionY(ingredientHolderObj, lastIngredient);
            CreateAndPlaceIngredient(ingredientHolderObj, nextPositionY, mainGameController, lastIngredient);
        }
    }

    private void DraggedIngredientFlewAnim()
    {
        draggedIngredient.transform.DOMove(transform.position, flewDuration).SetEase(Ease.OutExpo).OnComplete(() => draggedIngredient.SetActive(false));
    }

    // private Transform GetLastIngredient(GameObject ingredientHolderObj)
    // {
    //     return ingredientHolderObj.transform.childCount > 0 ? ingredientHolderObj.transform.GetChild(ingredientHolderObj.transform.childCount - 1) : null;
    // }

    private bool IsInvalidIngredientPlacement(GameObject ingredientHolderObj, Transform lastIngredient)
    {
        if (lastIngredient == null && ingredientDetails.ingredientType != IngredientType.Base)
        {
            return true;
        }
        else if (lastIngredient != null && lastIngredient.gameObject.activeSelf)
        {
            if (lastIngredient != null && lastIngredient.childCount > 0)
            {
                if (mainGameController.deliveryQueueIngredient >= mainGameController.maxOrderHeight)
                {
                    return true;
                }

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

    private void CreateAndPlaceIngredient(GameObject ingredientHolderObj, float nextPositionY, MainGameController mainGameController, Transform lastIngredient)
    {
        // Vector3 platePosition = ingredientHolderObj.transform.position;
        // GameObject ingredientGameobject = Instantiate(ingredientPrefab, platePosition, Quaternion.identity, parentIngredient);

        ingredientPool.SetParent(ingredientHolderObj.transform);

        Ingredient ingredientGameobject = ingredientPool.ingredientPool.Get();

        // if(lastIngredient != null && lastIngredient.gameObject.activeSelf)
        // {
        //     lastIngredient.SetSiblingIndex(0);
        // }

        Transform nextPosTransform = ingredientGameobject.transform.GetChild(ingredientGameobject.transform.childCount - 1);
        nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

        // Ingredient ingredient = ingredientGameobject.GetComponent<Ingredient>();
        ingredientGameobject.IngredientCode = (int)ingredientDetails.ingredientCode;
        ingredientGameobject.IngredientType = ingredientDetails.ingredientType;

        SpriteRenderer spriteRenderer = ingredientGameobject.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = ingredientDetails.dressIngredientSprite;
        // if (ingredientDetails.ingredientType == IngredientType.Topping)
        // {
        //     spriteRenderer.sprite = ingredientDetails.plateIngredientSprite;
        // }
        // else
        // {
        // }

        spriteRenderer.sortingLayerName = "Ingredient Holder";
        spriteRenderer.sortingOrder = 1;

        if (lastIngredient != null)
        {
            SpriteRenderer lastIngredientSprite = lastIngredient.GetComponentInChildren<SpriteRenderer>();

            if (lastIngredientSprite != null && ingredientGameobject.IngredientCode != (int)IngredientName.AstorTopping)
            {
                int temp = lastIngredientSprite.sortingOrder + 1;
                spriteRenderer.sortingOrder = temp;
            }
        }

        ingredientGameobject.transform.position = new Vector3(ingredientHolderObj.transform.position.x, nextPositionY, ingredientHolderObj.transform.position.z);

        mainGameController.deliveryQueueIngredient++;
        mainGameController.deliveryQueueIngredientsContent.Add(ingredientGameobject.IngredientCode);

        SetButtonLogic(ingredientGameobject.IngredientType);

        draggedIngredient.SetActive(false);
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