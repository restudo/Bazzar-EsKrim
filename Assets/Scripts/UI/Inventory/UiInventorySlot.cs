using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera mainCamera;
    private Transform parentIngredient;
    private IngredientHolder ingredientHolder;
    private LevelManager levelManager;
    private GameObject draggedIngredient;

    [SerializeField] private GameObject ingredientPrefab = null;
    public Image inventorySlotImage;
    [HideInInspector] public IngredientDetails ingredientDetails;

    Vector3 worldPosition;

    private void Start()
    {
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
                draggedCanvas.sortingLayerName = "RenderOnTop";
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

            if (levelManager.deliveryQueueIngredient < ingredientHolder.maxSlotIngredient)
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
    }

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
        ingredient.IngredientCode = ingredientDetails.ingredientCode;
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

        spriteRenderer.sortingLayerName = ingredientDetails.ingredientType.ToString();

        if (lastIngredient != null)
        {
            SpriteRenderer lastIngredientSprite = lastIngredient.GetComponentInChildren<SpriteRenderer>();

            if (lastIngredientSprite != null)
            {
                int temp = lastIngredientSprite.sortingOrder + 1;
                spriteRenderer.sortingOrder = temp;
                Debug.LogWarning(lastIngredientSprite.sortingOrder + " YESSS " + spriteRenderer.sortingOrder);
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
