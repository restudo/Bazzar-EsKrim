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
    [SerializeField] private IngredientPool ingredientPool;
    [SerializeField] private GameObject glowingPlate;

    private Camera mainCamera;
    private Transform parentIngredient;
    private IngredientHolder ingredientHolder;
    private MainGameController mainGameController;
    private const float flewDuration = 0.3f;

    private RectTransform draggedIngredientRect;
    private Canvas draggedCanvas;

    private void Awake()
    {
        // Cache components to avoid repeated lookups
        mainCamera = Camera.main;
        parentIngredient = GameObject.FindGameObjectWithTag("Ingredient Holder").transform;
        ingredientHolder = parentIngredient.GetComponent<IngredientHolder>();
        mainGameController = FindObjectOfType<MainGameController>();

        draggedIngredientRect = draggedIngredient.GetComponent<RectTransform>();
        draggedCanvas = draggedIngredient.GetComponent<Canvas>();

        glowingPlate.SetActive(false);
        draggedIngredient.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsValidForDrag()) return;

        ingredientHolder.canDeliverOrder = false;
        EventHandler.CallCloseTrashBinEvent();

        glowingPlate.SetActive(true);
        InitializeDraggedIngredient(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIngredient.activeSelf)
        {
            // Move dragged ingredient according to the pointer position
            UpdateDraggedIngredientPosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggedIngredient.activeSelf) return;

        HandleIngredientDrop();
        glowingPlate.SetActive(false);
        ingredientHolder.canDeliverOrder = true;
    }

    // Helper method to check if drag can be initiated
    private bool IsValidForDrag()
    {
        return ingredientDetails != null && GameManager.Instance.isGameActive && GameManager.Instance.gameStates == GameStates.MainGame && !inventorySlotImageBlocker.gameObject.activeSelf;
    }

    // Initialize dragged ingredient visuals and position
    private void InitializeDraggedIngredient(PointerEventData eventData)
    {
        draggedIngredient.SetActive(true);
        if (draggedCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            draggedCanvas.worldCamera = mainCamera;
            draggedCanvas.sortingLayerName = "Render On Top";
        }

        // Set sprite for the dragged ingredient
        Image draggedIngredientImage = draggedIngredient.GetComponentInChildren<Image>();
        draggedIngredientImage.sprite = ingredientDetails.dressIngredientSprite;

        UpdateDraggedIngredientPosition(eventData);
    }

    // Update the dragged ingredient's position based on the pointer
    private void UpdateDraggedIngredientPosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggedIngredientRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPosition))
        {
            draggedIngredientRect.position = worldPosition;
        }
    }

    // Handle the logic when ingredient is dropped
    private void HandleIngredientDrop()
    {
        BoxCollider2D plateCollider = parentIngredient.GetComponent<BoxCollider2D>();

        if (plateCollider != null && IsMouseOverCollider(plateCollider))
        {
            TryAddIngredientToPlate();
        }
        else
        {
            // Animate the ingredient flying back to its original position
            DraggedIngredientFlewAnim();
        }
    }

    private bool IsMouseOverCollider(BoxCollider2D collider)
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        return collider.bounds.Contains(mousePos);
    }

    // Animate ingredient flying back to its original position
    private void DraggedIngredientFlewAnim()
    {
        draggedIngredient.transform.DOMove(transform.position, flewDuration).SetEase(Ease.OutExpo).OnComplete(() => draggedIngredient.SetActive(false));
    }

    // Add ingredient to the plate
    private void TryAddIngredientToPlate()
    {
        if (ingredientDetails == null) return;

        Transform lastIngredient = ingredientPool.GetLastIngredient();
        if (IsInvalidIngredientPlacement(lastIngredient)) // Validate placement
        {
            DraggedIngredientFlewAnim();
            return;
        }

        float nextPositionY = CalculateNextPositionY(lastIngredient);
        PlaceIngredientOnPlate(nextPositionY, lastIngredient);

        draggedIngredient.SetActive(false);
    }

    private bool IsInvalidIngredientPlacement(Transform lastIngredient)
    {
        // Prevent placement of non-Base ingredient if the plate is empty
        if (lastIngredient == null && ingredientDetails.ingredientType != IngredientType.Base) return true;

        // Prevent multiple Base ingredients or incorrect order of ingredients
        if (lastIngredient != null && lastIngredient.gameObject.activeSelf)
        {
            Ingredient lastIngredientComponent = lastIngredient.GetComponent<Ingredient>();

            if (mainGameController.deliveryQueueIngredient >= mainGameController.maxOrderHeight) return true;
            if (lastIngredientComponent.IngredientType == IngredientType.Topping) return true;
            if (ingredientDetails.ingredientType == IngredientType.Base && lastIngredientComponent.IngredientType != IngredientType.Topping) return true;

            foreach (Transform child in parentIngredient)
            {
                if (child.GetComponent<Ingredient>().IngredientType == IngredientType.Base && ingredientDetails.ingredientType == IngredientType.Base)
                {
                    return true; // Prevent adding multiple base ingredients
                }
            }
        }
        return false;
    }

    private float CalculateNextPositionY(Transform lastIngredient)
    {
        // Calculate Y position based on the last ingredient's position
        return lastIngredient != null ? lastIngredient.GetChild(lastIngredient.childCount - 1).position.y : parentIngredient.position.y;
    }

    private void PlaceIngredientOnPlate(float nextPositionY, Transform lastIngredient)
    {
        Ingredient newIngredient = ingredientPool.ingredientPool.Get();
        Transform nextPosTransform = newIngredient.transform.GetChild(newIngredient.transform.childCount - 1);
        nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

        newIngredient.IngredientCode = (int)ingredientDetails.ingredientCode;
        newIngredient.IngredientType = ingredientDetails.ingredientType;

        SpriteRenderer spriteRenderer = newIngredient.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = ingredientDetails.dressIngredientSprite;
        spriteRenderer.sortingLayerName = "Ingredient Holder";

        if (lastIngredient != null)
        {
            SpriteRenderer lastSpriteRenderer = lastIngredient.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sortingOrder = lastSpriteRenderer != null ? lastSpriteRenderer.sortingOrder + 1 : 1;
        }

        newIngredient.transform.position = new Vector3(parentIngredient.position.x, nextPositionY, parentIngredient.position.z);

        mainGameController.deliveryQueueIngredient++;
        mainGameController.deliveryQueueIngredientsContent.Add(newIngredient.IngredientCode);

        UpdateIngredientButtons(newIngredient.IngredientType);
    }

    private void UpdateIngredientButtons(IngredientType ingredientType)
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
