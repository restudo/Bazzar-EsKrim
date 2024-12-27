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
    [SerializeField] private GameObject glowingPlate;
    [SerializeField] private IngredientPool ingredientPool;
    [SerializeField] private IngredientHolder ingredientHolder;
    [SerializeField] private MainGameController mainGameController;

    private Camera mainCamera;
    // private Transform parentIngredient;
    private RectTransform draggedIngredientRect;
    private Vector3 worldPosition;

    private const float flewDuration = 0.3f;
    private const string INGREDIENT_HOLDER = "Ingredient Holder";
    private const string RENDER_ON_TOP = "Render On Top";

    private void Start()
    {
        mainCamera = Camera.main;
        // parentIngredient = ingredientHolder.transform;

        glowingPlate.SetActive(false);
        draggedIngredient.SetActive(false);

        draggedIngredientRect = draggedIngredient.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredientDetails == null || !GameManager.Instance.isGameActive || GameManager.Instance.gameStates != GameStates.MainGame || inventorySlotImageBlocker.gameObject.activeSelf)
        {
            return;
        }

        ingredientHolder.canDeliverOrder = false;

        EventHandler.CallCloseTrashBinEvent();

        glowingPlate.SetActive(true);

        // Check if the input is from a touch device or mouse
        bool isSingleFingerTouch = Input.touchCount == 1 && eventData.pointerId >= 0;
        bool isLeftMouseClick = eventData.pointerId == -1; // -1 is the pointer ID for the left mouse button

        if (isSingleFingerTouch || isLeftMouseClick)
        {
            draggedIngredient.SetActive(true);

            Canvas draggedCanvas = draggedIngredient.GetComponent<Canvas>();
            if (draggedCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                draggedCanvas.worldCamera = mainCamera;
                draggedCanvas.sortingLayerName = RENDER_ON_TOP;
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
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggedIngredientRect, eventData.position, eventData.pressEventCamera, out worldPosition))
            {
                draggedIngredientRect.position = worldPosition;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIngredient != null)
        {
            HandleIngredientDrop(ingredientHolder.gameObject, mainGameController);

            glowingPlate.SetActive(false);

            ingredientHolder.canDeliverOrder = true;
        }
    }

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

    private void TryAddIngredientToPlate(GameObject ingredientHolderObj, MainGameController mainGameController)
    {
        if (ingredientDetails != null)
        {
            // Transform lastIngredient = GetLastIngredient(ingredientHolderObj);
            Transform lastIngredient = ingredientPool.GetLastIngredient();
            Ingredient ingredient = lastIngredient.GetComponent<Ingredient>();
            if (IsInvalidIngredientPlacement(ingredientHolderObj, lastIngredient, ingredient))
            {
                DraggedIngredientFlewAnim();

                return;
            }

            float nextPositionY = CalculateNextPositionY(ingredientHolderObj, lastIngredient);
            CreateAndPlaceIngredient(ingredientHolderObj, nextPositionY, mainGameController, lastIngredient, ingredient);
        }
    }

    private void DraggedIngredientFlewAnim()
    {
        draggedIngredient.transform.DOMove(transform.position, flewDuration).SetEase(Ease.OutExpo).OnComplete(() => draggedIngredient.SetActive(false));
    }

    private bool IsInvalidIngredientPlacement(GameObject ingredientHolderObj, Transform lastIngredient, Ingredient ingredient)
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

                if (ingredient.IngredientType == IngredientType.Base && ingredientDetails.ingredientType == IngredientType.Topping)
                {
                    return true;
                }

                if (ingredient.IngredientType == IngredientType.Topping)
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

    private void CreateAndPlaceIngredient(GameObject ingredientHolderObj, float nextPositionY, MainGameController mainGameController, Transform lastIngredient, Ingredient ingredient)
    {
        ingredientPool.SetParent(ingredientHolderObj.transform);

        Ingredient ingredientGameobject = ingredientPool.ingredientPool.Get();

        // set the nextpostransform from the last child transform from ingredient gameobject
        Transform nextPosTransform = ingredientGameobject.transform.GetChild(ingredientGameobject.transform.childCount - 1);
        nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

        ingredientGameobject.IngredientCode = (int)ingredientDetails.ingredientCode;
        ingredientGameobject.IngredientType = ingredientDetails.ingredientType;

        SpriteRenderer spriteRenderer = ingredientGameobject.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = ingredientDetails.dressIngredientSprite;

        spriteRenderer.sortingLayerName = INGREDIENT_HOLDER;
        spriteRenderer.sortingOrder = 1;

        // if this ingredient gameobject is the first one
        if (lastIngredient != null)
        {
            SpriteRenderer lastIngredientSprite = lastIngredient.GetComponentInChildren<SpriteRenderer>();

            if (lastIngredientSprite != null && ingredientGameobject.IngredientCode != (int)IngredientName.AstorTopping)
            {
                int temp = lastIngredientSprite.sortingOrder + 1;
                spriteRenderer.sortingOrder = temp;
            }
        }

        // set the transform of the ingredient gameobject to the next position of the previous ingredient gameobject
        ingredientGameobject.transform.position = new Vector3(ingredientHolderObj.transform.position.x, nextPositionY, ingredientHolderObj.transform.position.z);

        mainGameController.deliveryQueueIngredient++;
        mainGameController.deliveryQueueIngredientsContent.Add(ingredientGameobject.IngredientCode);

        ingredientHolder.availableIngredients.Add(ingredient);

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