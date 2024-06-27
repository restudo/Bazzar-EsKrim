using System.Collections;
using UnityEngine;
using DG.Tweening;

public class IngredientHolder : MonoBehaviour
{
    [HideInInspector] public bool canDeliverOrder;

    [SerializeField] LevelManager levelManager;
    [SerializeField] TrashBinController trashBin;
    [SerializeField] GameObject trashBinHighlight;

    private Vector3 initialPosition;
    private Collider2D plateCollider;

    void Awake()
    {
        plateCollider = GetComponent<Collider2D>();
        canDeliverOrder = false;
        initialPosition = transform.position;
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

    private void Update()
    {
        if (GameManager.Instance.isGameActive && canDeliverOrder)
        {
            ManageDeliveryDrag();
        }
    }

    private void ManageDeliveryDrag()
    {
        if (!(Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved) && !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (plateCollider != null)
        {
            Vector3 platePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            if (plateCollider.bounds.Contains(platePos))
            {
                StartCoroutine(CreateDeliveryPackage());
            }
        }
    }

    private IEnumerator CreateDeliveryPackage()
    {
        while (canDeliverOrder && levelManager.deliveryQueueIngredient > 0)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("Customer");

            foreach (GameObject customer in availableCustomers)
            {
                customer.transform.GetChild(customer.transform.childCount - 1).gameObject.SetActive(true);
            }

            trashBinHighlight.SetActive(true);
            trashBin.OpenTrashBin();

            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                HandleDelivery(availableCustomers);
                yield break;
            }

            yield return null;
        }
    }

    private void HandleDelivery(GameObject[] availableCustomers)
    {
        if (availableCustomers.Length < 1)
        {
            ResetPosition();
            return;
        }

        bool delivered = false;
        CustomerController theCustomer = null;

        foreach (GameObject customerObj in availableCustomers)
        {
            CustomerController customer = customerObj.GetComponent<CustomerController>();
            if (customer.isCloseEnoughToDelivery)
            {
                theCustomer = customer;
                delivered = levelManager.deliveryQueueIngredient > 1;
                if (!delivered)
                {
                    customer.BaseOnlyServed();
                    Debug.Log("Customer Angry");
                }
                break;
            }
        }

        if (delivered)
        {
            DebugDelivery();
            bool isOrderCorrect = theCustomer.ReceiveOrder(levelManager.deliveryQueueIngredientsContent);

            if (isOrderCorrect)
            {
                ResetMainQueue();
            }
        }

        ResetPosition();
        DeactivateCustomers(availableCustomers);
        trashBinHighlight.SetActive(false);
        trashBin.CloseTrashBin();
    }

    private void DebugDelivery()
    {
        for (int i = 0; i < levelManager.deliveryQueueIngredientsContent.Count; i++)
        {
            print($"Delivery Items ID {i} = {levelManager.deliveryQueueIngredientsContent[i]}");
        }
    }

    private void DeactivateCustomers(GameObject[] availableCustomers)
    {
        foreach (GameObject customer in availableCustomers)
        {
            customer.transform.GetChild(customer.transform.childCount - 1).gameObject.SetActive(false);
        }
    }

    private void ResetMainQueue()
    {
        levelManager.deliveryQueueIngredient = 0;
        levelManager.deliveryQueueIngredientsContent.Clear();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        EventHandler.CallEnableTabButtonEvent((int)IngredientType.Base);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Flavor);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Topping);

        Debug.Log("Reset Queue");
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