using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;

public class CustomerController : MonoBehaviour
{
    public float customerPatience = 30.0f;  //seconds 

    [HideInInspector] public int mySeat;
    [HideInInspector] public Vector3 destination;
    [HideInInspector] public Vector3 leavePoint;
    [HideInInspector] public bool isCloseEnoughToDelivery;

    [Range(0.1f, 1f)][SerializeField] private float decreasePatiencePercentage = 0.25f;
    [SerializeField] private float customerSpeed = 3.0f;
    [SerializeField] private GameObject HudPos;
    [SerializeField] private Sprite[] customerMoods;
    // [SerializeField] private GameObject[] allIngredients;
    // [SerializeField] private GameObject[] baseIngredients;
    // [SerializeField] private GameObject[] flavorIngredients;
    // [SerializeField] private GameObject[] toppingIngredients;

    private string customerName; //random name
    private int maxOrderSize = 6;
    private int moodIndex;
    private bool isOnSeat;
    private bool isLeaving;
    private bool isFacingRight;
    private bool isDeliveryPlateColliding;
    private bool isMousePositionColliding;

    private Vector2 mousePosition;
    private GameObject deliveryPlate;
    private GameObject levelManagerObj;
    private Collider2D deliveryPlateCol;
    private LevelManager levelManager;
    private OrderManager orderManager;
    private IngredientHolder ingredientHolder;
    private PatienceBarController patienceBarController;
    private MoneySpawner moneySpawner;

    private SpriteRenderer spriteRenderer;
    private Collider2D customerCol;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        customerCol = GetComponent<Collider2D>();

        levelManagerObj = FindObjectOfType<LevelManager>().gameObject;
        levelManager = levelManagerObj.GetComponent<LevelManager>();

        orderManager = GetComponent<OrderManager>();

        deliveryPlate = FindObjectOfType<IngredientHolder>().gameObject;
        ingredientHolder = deliveryPlate.GetComponent<IngredientHolder>();
        deliveryPlateCol = deliveryPlate.GetComponent<Collider2D>();

        patienceBarController = GetComponent<PatienceBarController>();

        moneySpawner = FindObjectOfType<MoneySpawner>();

        isCloseEnoughToDelivery = false;
        isOnSeat = false;
        isLeaving = false;
        isFacingRight = true;

        moodIndex = 0;
        maxOrderSize = levelManager.maxOrderHeight;
    }

    private void OnEnable()
    {
        EventHandler.ChaseCustomer += ChaseCustomer;
    }

    private void OnDisable()
    {
        EventHandler.ChaseCustomer -= ChaseCustomer;
    }

    private void Start()
    {
        Init();
    }

    private void LateUpdate()
    {
        //check if this customer is close enough to delivery, in order to receive it.
        if (ingredientHolder.canDeliverOrder)
        {
            CheckDistanceToDelivery();
        }
    }

    private void Init()
    {
        // give name
        customerName = "Customer_" + Random.Range(100, 10000);
        gameObject.name = customerName;

        // order
        int recipeUnlock = GameManager.Instance.GetRecipeUnlock();
        if (recipeUnlock > 0 && levelManager.customerCounter == levelManager.spawnSpecialRecipeAfterXCustomer)
        {
            orderManager.OrderByRecipe(recipeUnlock);
            Debug.Log("By recipe");
        }
        else
        {
            int randomMaxOrder = Random.Range(2, maxOrderSize + 1);
            orderManager.OrderRandomProduct(randomMaxOrder);
            Debug.Log("Random");
        }

        HudPos.SetActive(false);

        StartCoroutine(GoToSeat());
    }

    private IEnumerator GoToSeat()
    {
        if (transform.position.x > destination.x && isFacingRight)
        {
            Flip();
        }
        else if (transform.position.x < destination.x && !isFacingRight)
        {
            Flip();
        }

        Vector3 targetPosition;
        Tween yoyoTween = transform.DOLocalMoveY(destination.y + 0.35f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

        while ((transform.position - new Vector3(destination.x, transform.position.y, destination.z)).sqrMagnitude > 0.01f)
        {
            // Create a target position with the current Y value to keep Y-axis unchanged
            targetPosition = new Vector3(destination.x, transform.position.y, destination.z);

            // Move smoothly towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);

            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // Kill the specific Y-axis movement tween
        yoyoTween.Kill();

        yield return new WaitForEndOfFrame();

        // Ensure the final position is exactly the target position, with the final Y value
        transform.DOMove(destination, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            isOnSeat = true;
            HudPos.SetActive(true);

            patienceBarController.StartDecreasingPatience(); // Start the patience bar
        });
    }

    private void Flip()
    {
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

        // Check if the delivery plate bounds intersect with the trash bin bounds
        isDeliveryPlateColliding = customerCol.bounds.Intersects(deliveryPlateCol.bounds);

        // Check if the mouse position is within the bounds of the trash bin collider
        isMousePositionColliding = customerCol.bounds.Contains(mousePosition);

        // Set the flag if both conditions are 
        if (isDeliveryPlateColliding && isMousePositionColliding)
        {
            isCloseEnoughToDelivery = true;

            // add other method like change the tint of customer
        }
        else
        {
            isCloseEnoughToDelivery = false;
        }
    }

    public void UpdateCustomerMood(int moodIndex)
    {
        // spriteRenderer.sprite = customerMoods[moodIndex];
    }

    private void OrderIsCorrect()
    {
        Debug.Log("Order is correct.");

        moodIndex = 2;  //make him/her happy :>

        // TODO: trigger progression (number of successful order, etc)

        EventHandler.CallSetMoneyPosToCustomerPosEvent(transform.position);
        moneySpawner.moneyPool.Get();

        patienceBarController.StopDecreasingPatience();
        StartCoroutine(Leave());
    }

    private void OrderIsIncorrect()
    {
        Debug.Log("Order is not correct.");

        moodIndex = 3; //make him/her angry :<

        // patienceBarController.StopDecreasingPatience();
        // StartCoroutine(Leave());

        float decreaseValue = customerPatience * decreasePatiencePercentage;
        patienceBarController.DecreaseWithValue(decreaseValue);
    }

    public IEnumerator Leave()
    {
        //prevent double animation
        if (isLeaving)
        {
            yield break;
        }

        //set the leave flag to prevent multiple calls to this function
        isLeaving = true;

        levelManager.availableSeatForCustomers[mySeat] = true;

        //animate (close) patienceBar
        //animate (close) request bubble
        HudPos.SetActive(false);

        if (transform.position.x > leavePoint.x && isFacingRight)
        {
            Flip();
        }
        else if (transform.position.x < leavePoint.x && !isFacingRight)
        {
            Flip();
        }

        //wait for seconds
        yield return new WaitForSeconds(0.15f);

        Vector3 targetPosition;
        Tween yoyoTween = transform.DOLocalMoveY(destination.y + 0.35f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

        while ((transform.position - new Vector3(leavePoint.x, transform.position.y, leavePoint.z)).sqrMagnitude > 0.01f)
        {
            // Create a target position with the current Y value to keep Y-axis unchanged
            targetPosition = new Vector3(leavePoint.x, transform.position.y, leavePoint.z);

            // Move smoothly towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);

            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // Kill the specific Y-axis movement tween
        yoyoTween.Kill();

        // Ensure the final position is exactly the target position, with the final Y value
        transform.DOMove(leavePoint, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public bool ReceiveOrder(List<int> myReceivedOrder)
    {
        //check the received order with the original one (customer's wish).
        int[] myOriginalOrder = orderManager.productIngredientsCodes;

        //check if the two array are the same, meaning that we received what we were looking for.
        //print(myOriginalOrder + " - " + myReceivedOrder);

        //1.check the length of two arrays
        if (myOriginalOrder.Length == myReceivedOrder.Count)
        {
            //2.compare two arrays
            bool detectInequality = false;
            for (int i = 0; i < myOriginalOrder.Length; i++)
            {
                Debug.Log("Original Order: " + myOriginalOrder[i] + " vs Received Order: " + myReceivedOrder[i]);
                if (myOriginalOrder[i] != myReceivedOrder[i])
                {
                    detectInequality = true;
                }
            }

            if (!detectInequality)
            {
                OrderIsCorrect();

                EventHandler.CallCorrectOrderEvent();

                return true;
            }
            else
            {
                OrderIsIncorrect(); //different array items

                EventHandler.CallIncorrectOrderEvent();

                return false;
            }
        }
        else
        {
            OrderIsIncorrect(); //different array length

            EventHandler.CallIncorrectOrderEvent();

            return false;
        }
    }

    private void ChaseCustomer()
    {
        StartCoroutine(Leave());
    }

    public void BaseOnlyServed()
    {
        OrderIsIncorrect();
    }
}
