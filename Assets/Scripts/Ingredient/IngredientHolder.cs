using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class IngredientHolder : MonoBehaviour
{
    [HideInInspector] public bool canDeliverOrder;
    [HideInInspector] public List<CustomerController> availableCustomers = new();
    [HideInInspector] public List<Ingredient> availableIngredients = new();

    [SerializeField] MainGameController mainGameController;
    [SerializeField] TrashBinController trashBin;
    [SerializeField] ParticleSystem trashBinHighlightVfx;

    private Vector3 initialPosition;
    private Collider2D plateCollider;

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
        while (canDeliverOrder && mainGameController.deliveryQueueIngredient > 0)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            foreach (CustomerController customer in availableCustomers)
            {
                if (customer.IsOnSeat())
                {
                    int lastChildIndex = customer.transform.childCount - 1;
                    ParticleSystem customerVfx = customer.transform.GetChild(lastChildIndex).GetComponent<ParticleSystem>();

                    if (!customerVfx.gameObject.activeSelf)
                    {
                        customerVfx.gameObject.SetActive(true);
                        customerVfx.Play();
                    }
                }
            }

            if (!trashBinHighlightVfx.gameObject.activeSelf)
            {
                trashBinHighlightVfx.gameObject.SetActive(true);
                trashBinHighlightVfx.Play();
                trashBin.OpenTrashBin();
            }

            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                HandleDelivery(availableCustomers);
                yield break;
            }

            yield return null;
        }
    }

    private void HandleDelivery(List<CustomerController> availableCustomers)
    {
        if (availableCustomers.Count < 1)
        {
            ResetPosition();
            return;
        }

        foreach (CustomerController customer in availableCustomers)
        {
            if (customer.isCloseEnoughToDelivery)
            {
                if (mainGameController.deliveryQueueIngredient > 1)
                {
                    // DebugDelivery();
                    bool isOrderCorrect = customer.ReceiveOrder(mainGameController.deliveryQueueIngredientsContent);

                    if (isOrderCorrect)
                    {
                        ResetMainQueue();
                    }
                }
                else
                {
                    customer.BaseOnlyServed();
                }

                break;
            }
        }

        ResetPosition();
        DeactivateCustomers(availableCustomers);
        trashBinHighlightVfx.gameObject.SetActive(false);
        trashBin.CloseTrashBin();
    }

    private void DeactivateCustomers(List<CustomerController> availableCustomers)
    {
        foreach (CustomerController customer in availableCustomers)
        {
            customer.transform.GetChild(customer.transform.childCount - 1).gameObject.SetActive(false);
        }
    }

    private void ResetMainQueue()
    {
        // release ingredient pool
        foreach (Ingredient ingredient in availableIngredients)
        {
            if (ingredient != null && !ingredient.GetReleaseFlag())
            {
                ingredient.GetIngredientPool().Release(ingredient);
                ingredient.SetReleaseFlag(true);
            }
        }

        mainGameController.deliveryQueueIngredient = 0;
        mainGameController.deliveryQueueIngredientsContent.Clear();

        availableIngredients.Clear();

        EventHandler.CallEnableTabButtonEvent((int)IngredientType.Base);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Flavor);
        EventHandler.CallDisableTabButtonEvent((int)IngredientType.Topping); 
    }

    private void ResetPosition()
    {
        if (trashBin.isCloseEnoughToTrashbin)
        {
            EventHandler.CallCloseTrashBinEvent();
            EventHandler.CallPlaySfxTrashBinEvent();
            // EventHandler.CallSquishTrashBinEvent();
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