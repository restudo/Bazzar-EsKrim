using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;

public class CustomerController : MonoBehaviour
{
    // Public properties
    public float customerPatience = 30.0f;  // seconds 

    [HideInInspector] public int mySeat;
    [HideInInspector] public Vector3 destination;
    [HideInInspector] public Vector3 leavePoint;
    [HideInInspector] public bool isCloseEnoughToDelivery;

    // Serialized fields for customization in the inspector
    [SerializeField] private SO_CustomerList customerList;
    [Range(0.1f, 1f)][SerializeField] private float decreasePatiencePercentage = 0.25f;
    [SerializeField] private float customerSpeed = 3.0f;
    [SerializeField] private GameObject HudPos;
    [SerializeField] private Sprite[] customerMoods;

    // Private variables
    private int maxOrderSize = 6;
    private int moodIndex;
    private bool isOnSeat;
    private bool isLeaving;
    private bool isFacingRight;
    private bool isRecipeOrder;
    private bool isDeliveryPlateColliding;
    private bool isMousePositionColliding;

    private Vector2 mousePosition;
    private GameObject deliveryPlate;
    private GameObject mainGameControllerObj;
    private Collider2D deliveryPlateCol;
    private MainGameController mainGameController;
    private OrderManager orderManager;
    private IngredientHolder ingredientHolder;
    private PatienceBarController patienceBarController;
    private MoneySpawner moneySpawner;
    private CustomerPool customerPool;

    private SpriteRenderer spriteRenderer;
    private Collider2D customerCol;

    private void Awake()
    {
        // Cache component references
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        customerCol = GetComponent<Collider2D>();

        // Find and cache references to other objects in the scene
        mainGameControllerObj = FindObjectOfType<MainGameController>().gameObject;
        mainGameController = mainGameControllerObj.GetComponent<MainGameController>();
        customerPool = mainGameController.GetComponent<CustomerPool>();
        orderManager = GetComponent<OrderManager>();
        deliveryPlate = FindObjectOfType<IngredientHolder>().gameObject;
        ingredientHolder = deliveryPlate.GetComponent<IngredientHolder>();
        deliveryPlateCol = deliveryPlate.GetComponent<Collider2D>();
        patienceBarController = GetComponent<PatienceBarController>();
        moneySpawner = FindObjectOfType<MoneySpawner>();

        isFacingRight = true;
    }

    private void OnEnable()
    {
        EventHandler.ChaseCustomer += ChaseCustomer;
    }

    private void OnDisable()
    {
        EventHandler.ChaseCustomer -= ChaseCustomer;
    }

    private void LateUpdate()
    {
        // Check if the customer is close enough to the delivery plate to receive the order
        if (ingredientHolder.canDeliverOrder)
        {
            CheckDistanceToDelivery();
        }
    }

    public void Init()
    {
        // Initialize customer with random details from the customer list
        int randomCustomer = Random.Range(0, customerList.customerDetails.Count);
        customerPatience = customerList.customerDetails[randomCustomer].customerPatience;
        customerMoods = customerList.customerDetails[randomCustomer].customerMoods;
        spriteRenderer.sprite = customerMoods[0];
        // spriteRenderer.color = customerList.customerDetails[randomCustomer].customerColor; // remove later

        // Reset flags and variables
        isCloseEnoughToDelivery = false;
        isOnSeat = false;
        isLeaving = false;
        isRecipeOrder = false;
        moodIndex = 0;
        maxOrderSize = mainGameController.maxOrderHeight;

        Order();

        // Hide HUD and highlight
        HudPos.SetActive(false);
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);

        // Start moving to the assigned seat
        StartCoroutine(GoToSeat());
    }

    private void Order()
    {
        if (mainGameController.customerCounter == mainGameController.spawnSpecialRecipeAfterXCustomer)
        {
            // Ensure to run order by recipe when customer counter is equal to the threshold
            orderManager.OrderByRecipe(mainGameController.maxSpecialRecipeInThisLevel);
            isRecipeOrder = true;
            mainGameController.wasLastOrderByRecipe = true;

            mainGameController.customerCounter = 0;
        }
        else
        {
            // Randomly decide between OrderByRecipe and OrderRandomProduct
            if (Random.value > 0.5f && mainGameController.maxSpecialRecipeInThisLevel > 0 && !mainGameController.wasLastOrderByRecipe)
            {
                // Generate order by recipe
                orderManager.OrderByRecipe(mainGameController.maxSpecialRecipeInThisLevel);
                isRecipeOrder = true;
                mainGameController.wasLastOrderByRecipe = true;

                mainGameController.customerCounter = 0;
            }
            else
            {
                // Generate a random order
                int randomMaxOrder = Random.Range(2, maxOrderSize + 1);
                orderManager.OrderRandomProduct(randomMaxOrder);
                isRecipeOrder = false;
                mainGameController.wasLastOrderByRecipe = false;
            }
        }
    }

    private IEnumerator GoToSeat()
    {
        // Flip sprite if necessary based on the destination
        FlipCheck(destination);

        // Create a yoyo tween, but don't start it immediately
        Tween yoyoTween = null;

        while ((transform.position - new Vector3(destination.x, transform.position.y, destination.z)).sqrMagnitude > 0.01f)
        {
            if (GameManager.Instance.isGameActive)
            {
                // Check if the tween has been killed by DOTween.KillAll() and recreate it if needed
                if (yoyoTween == null || !yoyoTween.IsActive())
                {
                    yoyoTween = transform.DOLocalMoveY(destination.y + 0.2f, 0.35f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.Linear);
                }
                else if (!yoyoTween.IsPlaying())
                {
                    yoyoTween.Play();
                }

                // Move towards the destination
                Vector3 targetPosition = new Vector3(destination.x, transform.position.y, destination.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);
            }
            else
            {
                // Pause the yoyo tween when the game is inactive
                if (yoyoTween != null && yoyoTween.IsPlaying())
                {
                    yoyoTween.Pause();
                }
            }

            yield return null;
        }

        // Stop the yoyo movement and finalize position
        if (yoyoTween != null && yoyoTween.IsActive())
        {
            yoyoTween.Kill();
            yoyoTween = null;  // Set to null after killing to avoid further use
        }

        yield return new WaitForSeconds(0.05f);

        transform.DOMove(destination, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (GameManager.Instance.isGameActive)
            {
                isOnSeat = true;
                HudPos.SetActive(true);
                patienceBarController.StartDecreasingPatience();
            }
        });
    }

    private void FlipCheck(Vector3 target)
    {
        // Flip the sprite to face the correct direction
        if ((transform.position.x > target.x && isFacingRight) || (transform.position.x < target.x && !isFacingRight))
        {
            Flip();
        }
    }

    private void Flip()
    {
        // Rotate the sprite to flip it
        isFacingRight = !isFacingRight;
        spriteRenderer.transform.Rotate(0, 180, 0);
    }

    private void CheckDistanceToDelivery()
    {
        if (customerCol == null || deliveryPlateCol == null || !isOnSeat || isLeaving)
        {
            isCloseEnoughToDelivery = false;
            return;
        }

        // Get the mouse position in world coordinates
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Check if the delivery plate bounds intersect with the customer bounds
        isDeliveryPlateColliding = customerCol.bounds.Intersects(deliveryPlateCol.bounds);

        // Check if the mouse position is within the bounds of the customer collider
        isMousePositionColliding = customerCol.bounds.Contains(mousePosition);

        // Set the flag if both conditions are true
        isCloseEnoughToDelivery = isDeliveryPlateColliding && isMousePositionColliding;
    }

    public void UpdateCustomerMood(int moodIndex)
    {
        // Update the customer's mood sprite (currently commented out)
        // spriteRenderer.sprite = customerMoods[moodIndex];
    }

    private void OrderIsCorrect()
    {
        // Trigger progression events and spawn money
        EventHandler.CallCorrectOrderEvent(isRecipeOrder);
        EventHandler.CallSetMoneyPosToCustomerPosEvent(transform.position, isRecipeOrder);
        moneySpawner.moneyPool.Get();

        moodIndex = 2;  // Make the customer happy

        // Stop decreasing patience and start leaving
        patienceBarController.StopDecreasingPatience();
        StartCoroutine(Leave());
    }

    private void OrderIsIncorrect()
    {
        EventHandler.CallIncorrectOrderEvent();

        moodIndex = 3; // Make the customer angry

        // Decrease patience based on a percentage
        float decreaseValue = customerPatience * decreasePatiencePercentage;
        patienceBarController.DecreaseWithValue(decreaseValue);
    }

    public IEnumerator Leave()
    {
        if (isLeaving) yield break;

        isLeaving = true;
        isOnSeat = false;

        // Mark the seat as available and hide HUD and highlight
        mainGameController.availableSeatForCustomers[mySeat] = true;
        HudPos.SetActive(false);
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);

        // Flip the sprite if necessary and set leaving flag
        FlipCheck(leavePoint);

        yield return new WaitForSeconds(0.15f);

        // Tween for yoyo movement (up and down) while moving horizontally
        Tween yoyoTween = transform.DOLocalMoveY(destination.y + 0.35f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

        // Move towards the leave point
        while ((transform.position - new Vector3(leavePoint.x, transform.position.y, leavePoint.z)).sqrMagnitude > 0.01f)
        {
            Vector3 targetPosition = new Vector3(leavePoint.x, transform.position.y, leavePoint.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);
            yield return null;
        }

        // Stop the yoyo movement and finalize position
        yoyoTween.Kill();
        transform.DOMove(leavePoint, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            orderManager.ReleaseAllIngredients();
            customerPool.customerPool.Release(this);  // Return customer to the pool
        });
    }

    public bool ReceiveOrder(List<int> myReceivedOrder)
    {
        int[] myOriginalOrder = orderManager.productIngredientsCodes;

        // Check if the received order matches the original order
        if (myOriginalOrder.Length != myReceivedOrder.Count) return OrderIsIncorrectAndReturnFalse();

        for (int i = 0; i < myOriginalOrder.Length; i++)
        {
            if (myOriginalOrder[i] != myReceivedOrder[i]) return OrderIsIncorrectAndReturnFalse();
        }

        OrderIsCorrect();

        return true;
    }

    private bool OrderIsIncorrectAndReturnFalse()
    {
        OrderIsIncorrect();
        return false;
    }

    private void ChaseCustomer()
    {
        StartCoroutine(Leave());
    }

    public void BaseOnlyServed()
    {
        OrderIsIncorrect();
    }

    public bool IsOnSeat()
    {
        return isOnSeat;
    }
}
