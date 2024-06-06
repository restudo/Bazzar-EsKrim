using System.Collections;
using UnityEngine;
using DG.Tweening;

public class IngredientHolder : MonoBehaviour
{
    public int maxSlotIngredient = 6; //maximum available slots in delivery queue (set in init)
    [HideInInspector] public bool canDeliverOrder;

    [SerializeField] LevelManager levelManager;
    [SerializeField] TrashBinController trashbin;


    //Private flags
    private Vector3 initialPosition;
    private Collider2D plateCollider;
    private Vector3 platePos;

    void Awake()
    {
        transform.position = GameObject.FindGameObjectWithTag("Plate").transform.position;

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

    void Update()
    {
        if (GameManager.Instance.isGameActive && canDeliverOrder)
        {
            ManageDeliveryDrag();
        }
    }

    void ManageDeliveryDrag()
    {
        if (!((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved) || Input.GetMouseButtonDown(0)))
        {
            return;
        }

        if (plateCollider != null)
        {
            // Convert mouse position to world space
            platePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            if (plateCollider.bounds.Contains(platePos)) // && Ingredient not in hand
            {
                StartCoroutine(CreateDeliveryPackage());
            }
        }
    }

    IEnumerator CreateDeliveryPackage()
    {
        while (canDeliverOrder && levelManager.deliveryQueueIngredient > 0)
        {
            //follow mouse or touch
            // platePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // platePos = new Vector3(platePos.x, platePos.y, -0.5f);

            //follow player's finger
            // transform.position = platePos;
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            //deliver (dragging the plate) is not possible when user is not touching screen
            //so we must decide what we are going to do after dragging and releasing the plate
            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                //if we are giving the order to a customer (plate is close enough)
                GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("Customer");

                //if there is no customer in shop, take the plate back.
                if (availableCustomers.Length < 1)
                {
                    //take the plate back to it's initial position
                    ResetPosition();
                    yield break;
                }

                bool delivered = false;
                GameObject theCustomer = null;
                for (int i = 0; i < availableCustomers.Length; i++)
                {
                    if (availableCustomers[i].GetComponent<CustomerController>().isCloseEnoughToDelivery)
                    {
                        //we know that just 1 customer is always nearest to the delivery. so "theCustomer" is unique.
                        theCustomer = availableCustomers[i];

                        if (levelManager.deliveryQueueIngredient > 1)
                        {
                            delivered = true;
                        }
                        else
                        {
                            delivered = false;

                            //TODO: make customer angry
                            Debug.Log("Customer Angry");
                        }
                    }
                }

                //if customer got the delivery
                if (delivered)
                {
                    //debug delivery
                    for (int i = 0; i < levelManager.deliveryQueueIngredientsContent.Count; i++)
                    {
                        print("Delivery Items ID " + i.ToString() + " = " + levelManager.deliveryQueueIngredientsContent[i]);
                    }

                    //let the customers know what he got.
                    bool isOrderCorrect = theCustomer.GetComponent<CustomerController>().ReceiveOrder(levelManager.deliveryQueueIngredientsContent);

                    if (isOrderCorrect)
                    {
                        // reset and destroy serving plate contents
                        ResetMainQueue();
                    }

                    //take the plate back to it's initial position
                    ResetPosition();
                }
                else
                {
                    ResetPosition();
                }
            }

            yield return 0;
        }
    }

    private void ResetMainQueue()
    {
        //reset main queue
        levelManager.deliveryQueueIngredient = 0;
        // levelManager.deliveryQueueIsFull = false;
        levelManager.deliveryQueueIngredientsContent.Clear();

        //destroy the contents of the serving plate.
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
        //just incase user wants to move this to trashbin, check it here first
        if (trashbin.isCloseEnoughToTrashbin)
        {
            EventHandler.CallCloseTrashBinEvent();
            EventHandler.CallSquishTrashBinEvent();
            ResetMainQueue();
        }

        //jump to initial trasnform
        transform.DOMove(initialPosition, 0.15f).SetEase(Ease.OutExpo);
        canDeliverOrder = false;
        StartCoroutine(Reactivate());
    }

    IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(0.25f);
        canDeliverOrder = true;
    }

    private void OnMouseDown()
    {
        EventHandler.CallCloseTrashBinEvent();
    }
}
