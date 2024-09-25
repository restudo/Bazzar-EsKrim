using System.Collections;
using UnityEngine;
using DG.Tweening;

public class IngredientHolder : MonoBehaviour
{
    [HideInInspector] public bool canDeliverOrder;

    [SerializeField] MainGameController mainGameController;
    [SerializeField] TrashBinController trashBin;
    [SerializeField] GameObject trashBinHighlight;

    private Vector3 initialPosition;
    private Collider2D plateCollider;
    private GameObject[] cachedCustomers; // Cache the customers to avoid calling FindGameObjectsWithTag repeatedly
    private Vector3 platePosition; // Cache the mouse position

    void Awake()
    {
        plateCollider = GetComponent<Collider2D>();
        canDeliverOrder = false;
    }

    private void OnEnable()
    {
        EventHandler.ResetMainQueue += ResetMainQueue;
        EventHandler.ResetPlatePosition += ResetPosition;
    }

    private void OnDisable()
    {
        EventHandler.ResetMainQueue -= ResetMainQueue;
        EventHandler.ResetPlatePosition -= ResetPosition;
    }

    private void Start()
    {
        initialPosition = transform.position;
        CacheCustomers(); // Cache customers at the start
    }

    private void Update()
    {
        if (GameManager.Instance.isGameActive && canDeliverOrder && GameManager.Instance.gameStates == GameStates.MainGame)
        {
            ManageDeliveryDrag();
        }
    }

    private void ManageDeliveryDrag()
    {
        // Handle input only if the plate is clicked or moved
        if (!IsDragging()) return;

        // Update plate position and check if it's within bounds
        platePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

        if (plateCollider.bounds.Contains(platePosition))
        {
            StartCoroutine(CreateDeliveryPackage());
        }
    }

    // Simplified input handling
    private bool IsDragging()
    {
        return (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved) || Input.GetMouseButtonDown(0);
    }

    private IEnumerator CreateDeliveryPackage()
    {
        while (canDeliverOrder && mainGameController.deliveryQueueIngredient > 0)
        {
            // Move plate position to mouse
            transform.position = platePosition;

            // Show HUD for all customers
            ShowCustomerHud(true);

            trashBinHighlight.SetActive(true);
            trashBin.OpenTrashBin();

            // Check if delivery is done (touches end or mouse released)
            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                HandleDelivery();
                yield break; // Exit loop after delivery
            }

            yield return null; // Yield to prevent infinite looping
        }
    }

    private void HandleDelivery()
    {
        bool delivered = false;
        CustomerController targetCustomer = null;

        foreach (GameObject customerObj in cachedCustomers) // Use cached customers
        {
            CustomerController customer = customerObj.GetComponent<CustomerController>();
            if (customer.isCloseEnoughToDelivery)
            {
                targetCustomer = customer;
                delivered = mainGameController.deliveryQueueIngredient > 1;
                if (!delivered)
                {
                    customer.BaseOnlyServed();
                }
                break; // Exit once we deliver to one customer
            }
        }

        if (delivered && targetCustomer != null)
        {
            bool isOrderCorrect = targetCustomer.ReceiveOrder(mainGameController.deliveryQueueIngredientsContent);

            if (isOrderCorrect)
            {
                ResetMainQueue();
            }
        }

        ResetPosition();
        ShowCustomerHud(false); // Deactivate customer HUD
        trashBinHighlight.SetActive(false);
        trashBin.CloseTrashBin();
    }

    // Caching customers to avoid repetitive calls to FindGameObjectsWithTag
    private void CacheCustomers()
    {
        cachedCustomers = GameObject.FindGameObjectsWithTag("Customer");
    }

    // Show or hide HUD for customers
    private void ShowCustomerHud(bool show)
    {
        foreach (GameObject customer in cachedCustomers)
        {
            CustomerController customerController = customer.GetComponent<CustomerController>();
            if (customerController.IsOnSeat())
            {
                int lastChildIndex = customer.transform.childCount - 1;
                customer.transform.GetChild(lastChildIndex).gameObject.SetActive(show);
            }
        }
    }

    private void ResetMainQueue()
    {
        mainGameController.deliveryQueueIngredient = 0;
        mainGameController.deliveryQueueIngredientsContent.Clear();

        // Release ingredients from pool
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<Ingredient>(out Ingredient ingredient))
            {
                if (!ingredient.GetReleaseFlag())
                {
                    ingredient.GetIngredientPool().Release(ingredient);
                    ingredient.SetReleaseFlag(true);
                }
            }
        }

        EventHandler.CallEnableTabButtonEvent((int)IngredientType.Base);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Flavor);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Topping);
    }

    private void ResetPosition()
    {
        if (trashBin.isCloseEnoughToTrashbin)
        {
            EventHandler.CallCloseTrashBinEvent();
            EventHandler.CallSquishTrashBinEvent();
            ResetMainQueue();
        }

        transform.DOMove(initialPosition, 0.15f).SetEase(Ease.OutExpo);
        canDeliverOrder = false;
        StartCoroutine(Reactivate());
    }

    private IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(0.25f);
        canDeliverOrder = true;
    }

    private void OnMouseDown()
    {
        EventHandler.CallCloseTrashBinEvent();
    }
}
