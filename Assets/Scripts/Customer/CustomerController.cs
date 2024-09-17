using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;
using Spine.Unity;

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

    [Space(20)]
    [SpineAnimation][SerializeField] private string idleAnimationName;
    [SpineAnimation][SerializeField] private string walkAnimationName;
    [SpineAnimation][SerializeField] private string positiveAnimationName;
    [SpineAnimation][SerializeField] private string negativeAnimationName;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private MeshRenderer meshRenderer;

    // Private variables
    private float positiveAnimationDuration;
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

    // private SpriteRenderer spriteRenderer;
    private Collider2D customerCol;

    private void Awake()
    {
        // Cache component references
        // spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        EventHandler.TogglePause += TogglePauseAnim;
    }

    private void OnDisable()
    {
        EventHandler.ChaseCustomer -= ChaseCustomer;
        EventHandler.TogglePause -= TogglePauseAnim;
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
        // Initialize customer with random details
        InitializeCustomerDetails();

        // Reset flags and variables
        ResetFlagsAndVariables();

        // Hide the HUD and highlight
        HideHudAndHighlight();

        // Start the customer movement coroutine
        StartCoroutine(GoToSeat());
    }

    private void InitializeCustomerDetails()
    {
        // Pick a random customer from the list
        int randomCustomer = Random.Range(0, customerList.customerDetails.Count);

        // Set customer attributes (patience, speed, etc.)
        customerPatience = customerList.customerDetails[randomCustomer].customerPatience;
        customerSpeed = customerList.customerDetails[randomCustomer].customerSpeed;

        // handle customer moods and sprite changes here if necessary
        // customerMoods = customerList.customerDetails[randomCustomer].customerMoods;
        // spriteRenderer.sprite = customerMoods[0];
        // spriteRenderer.color = customerList.customerDetails[randomCustomer].customerColor; // remove later if unneeded

        // Set the SkeletonDataAsset
        skeletonAnimation.skeletonDataAsset = customerList.customerDetails[randomCustomer].skeletonDataAsset;

        // Reinitialize the skeleton to apply the new SkeletonDataAsset
        skeletonAnimation.Initialize(true);

        // Set an animation for the newly assigned skeleton
        skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);

        // Set the material if applicable
        if (meshRenderer != null)
        {
            meshRenderer.material = customerList.customerDetails[randomCustomer].material;
        }

        positiveAnimationDuration = customerList.customerDetails[randomCustomer].positiveAnimationDuration;
    }

    private void ResetFlagsAndVariables()
    {
        // Reset all relevant flags
        isCloseEnoughToDelivery = false;
        isOnSeat = false;
        isLeaving = false;
        isRecipeOrder = false;

        // Reset mood and max order size
        moodIndex = 0;
        maxOrderSize = mainGameController.maxOrderHeight;

        // Call the order function (assumed this handles order-related initialization)
        Order();
    }

    private void HideHudAndHighlight()
    {
        // Hide the HUD position and any highlight on the customer
        HudPos.SetActive(false);

        // Hide the last child, which seems to be the highlight or indicator
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
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

        while ((transform.position - new Vector3(destination.x, transform.position.y, destination.z)).sqrMagnitude > 0.01f)
        {
            if (!GameManager.Instance.isGamePaused)
            {
                // Move towards the destination
                Vector3 targetPosition = new Vector3(destination.x, transform.position.y, destination.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);

                skeletonAnimation.timeScale = 1;
            }
            else
            {
                skeletonAnimation.timeScale = 0;
            }

            yield return null;
        }

        isOnSeat = true;
        HudPos.SetActive(true);

        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);

        patienceBarController.StartDecreasingPatience();
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
        // spriteRenderer.transform.Rotate(0, 180, 0);
        meshRenderer.transform.Rotate(0, 180, 0);
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

        // set the positive animation and then walk
        if (!isLeaving)
        {
            HudPos.SetActive(false);
            skeletonAnimation.AnimationState.SetAnimation(0, positiveAnimationName, false);
        }

        // Stop decreasing patience and start leaving
        patienceBarController.StopDecreasingPatience();
        StartCoroutine(Leave());
    }

    private void OrderIsIncorrect()
    {
        EventHandler.CallIncorrectOrderEvent();

        moodIndex = 3; // Make the customer angry

        // set the negative animation and idle again
        if (!isLeaving)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, negativeAnimationName, false);
            skeletonAnimation.AnimationState.AddAnimation(0, idleAnimationName, true, 0);
        }

        // Decrease patience based on a percentage
        float decreaseValue = customerPatience * decreasePatiencePercentage;
        patienceBarController.DecreaseWithValue(decreaseValue);
    }

    public IEnumerator Leave()
    {
        if (isLeaving) yield break;

        isLeaving = true;
        isOnSeat = false;

        // Check if the positive or negative animation is playing before switching to walk animation
        yield return StartCoroutine(CheckAndSetWalkAnimation(positiveAnimationDuration));

        // Mark the seat as available and hide HUD and highlight
        mainGameController.availableSeatForCustomers[mySeat] = true;
        HudPos.SetActive(false);
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);

        // Flip the sprite if necessary and set leaving flag
        FlipCheck(leavePoint);

        yield return new WaitForSeconds(0.15f);

        // Move towards the leave point
        while ((transform.position - new Vector3(leavePoint.x, transform.position.y, leavePoint.z)).sqrMagnitude > 0.01f)
        {
            if (!GameManager.Instance.isGamePaused)
            {
                Vector3 targetPosition = new Vector3(leavePoint.x, transform.position.y, leavePoint.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);

                skeletonAnimation.timeScale = 1;
            }
            else
            {
                skeletonAnimation.timeScale = 0;
            }

            yield return null;
        }

        orderManager.ReleaseAllIngredients();
        customerPool.customerPool.Release(this);  // Return customer to the pool
    }

    private IEnumerator CheckAndSetWalkAnimation(float positiveAnimationDuration)
    {
        // Get the current animation on track 0
        Spine.TrackEntry currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(0);

        // Check if the positive or negative animation is playing
        if (currentTrackEntry != null)
        {
            // Check for positive animation
            if (currentTrackEntry.Animation.Name == positiveAnimationName)
            {
                // Play the positive animation for only the specified duration
                while (currentTrackEntry.TrackTime < positiveAnimationDuration)
                {
                    yield return null;
                }

                // After positive animation, transition to the normal walk animation
                skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);
                Debug.Log("Leaving Happy");
            }
            // Check for negative animation
            else if (currentTrackEntry.Animation.Name == negativeAnimationName)
            {
                // Wait until the negative animation is complete
                yield return new WaitForSpineAnimationComplete(currentTrackEntry);

                // After negative animation, transition to the angry walk animation
                // TODO: change it to leaving angry
                skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);
                Debug.Log("Leaving Angry");
            }
            else
            {
                // If no specific animation is playing, just set the walk animation
                skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);
                Debug.Log("Leaving Normally");
            }
        }
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

    private void TogglePauseAnim()
    {
        if (skeletonAnimation.timeScale == 0)
        {
            skeletonAnimation.timeScale = 1;
        }
        else
        {
            skeletonAnimation.timeScale = 0;
        }
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
