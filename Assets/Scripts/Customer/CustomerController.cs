using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System.Linq;
using BazarEsKrim;

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
    [SerializeField] private SkeletonAnimation ladyCustomer;
    [SerializeField] private SkeletonAnimation teenCustomer;
    [SerializeField] private SkeletonAnimation manCustomer;

    [Header("Audio")]
    [SerializeField] private AudioClip bubbleOrderPopUpSfx;

    // Private variables
    private float positiveAnimationDuration;
    private int maxOrderSize = 6;
    private int moodIndex;
    private bool isOnSeat;
    private bool isLeaving;
    private bool isFacingRight;
    private bool isRecipeOrder;
    private bool isOrderCompleted;
    private bool isDeliveryPlateColliding;
    private bool isMousePositionColliding;

    private Vector2 mousePosition;
    private MainGameController mainGameController;
    private Collider2D deliveryPlateCol;
    private OrderManager orderManager;
    private IngredientHolder ingredientHolder;
    private PatienceBarController patienceBarController;
    private MoneySpawner moneySpawner;
    private CustomerPool customerPool;
    private SkeletonAnimation skeletonAnimation;
    private MeshRenderer meshRenderer;
    private Collider2D customerCol;

    private const int minOrderSize = 2;

    private void Awake()
    {
        // Ensure all customer objects are initially disabled
        ladyCustomer.gameObject.SetActive(false);
        teenCustomer.gameObject.SetActive(false);
        manCustomer.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventHandler.ChaseCustomer += StartLeaving;
        EventHandler.TogglePause += TogglePauseAnim;
    }

    private void OnDisable()
    {
        EventHandler.ChaseCustomer -= StartLeaving;
        EventHandler.TogglePause -= TogglePauseAnim;
    }

    private void LateUpdate()
    {
        // Check distance to the delivery only if the customer is on the seat and not leaving
        if (isOnSeat && !isLeaving && ingredientHolder.canDeliverOrder)
        {
            CheckDistanceToDelivery();
        }
    }

    public void Init()
    {
        InitializeCustomerDetails();
        ResetFlagsAndVariables();
        HideHudAndHighlight();
        StartCoroutine(GoToSeat());
    }

    private void InitializeCustomerDetails()
    {
        isFacingRight = true;
        isOrderCompleted = false;
        skeletonAnimation = null;
        meshRenderer = null;

        // Pick a random customer from the list
        var randomCustomer = customerList.customerDetails[Random.Range(0, customerList.customerDetails.Count)];
        customerPatience = randomCustomer.customerPatience;
        customerSpeed = randomCustomer.customerSpeed;

        SetCustomerAppearance(randomCustomer.customers);
        InitializeSkeletonWithNewData(randomCustomer);
        EnsureUniqueSkin(randomCustomer);
        SetAnimation(randomCustomer);
    }

    private void SetCustomerAppearance(Customers customerType)
    {
        // Disable all customers initially
        ladyCustomer.gameObject.SetActive(false);
        teenCustomer.gameObject.SetActive(false);
        manCustomer.gameObject.SetActive(false);

        // Enable the selected customer and cache references
        switch (customerType)
        {
            case Customers.Nyonya:
                ladyCustomer.gameObject.SetActive(true);
                skeletonAnimation = ladyCustomer;
                meshRenderer = ladyCustomer.GetComponent<MeshRenderer>();
                break;
            case Customers.Remaja:
                teenCustomer.gameObject.SetActive(true);
                skeletonAnimation = teenCustomer;
                meshRenderer = teenCustomer.GetComponent<MeshRenderer>();
                break;
            case Customers.Bapak:
                manCustomer.gameObject.SetActive(true);
                skeletonAnimation = manCustomer;
                meshRenderer = manCustomer.GetComponent<MeshRenderer>();
                break;
        }
    }

    private void InitializeSkeletonWithNewData(CustomerDetails customerDetail)
    {
        skeletonAnimation.skeletonDataAsset.Clear();
        skeletonAnimation.skeletonDataAsset = customerDetail.skeletonDataAsset;
        skeletonAnimation.Initialize(true);
        meshRenderer.material = customerDetail.material;
    }

    private void EnsureUniqueSkin(CustomerDetails customerDetail)
    {
        var skeleton = skeletonAnimation.Skeleton;
        var availableSkins = customerDetail.availableSkins;

        // Filter out already used skins
        var unusedSkins = availableSkins.Where(skin => !IsSkinAlreadyUsed(skin)).ToList();

        // Check if there are any unused skins available
        if (unusedSkins.Count == 0)
        {
            Debug.LogWarning("No unused skins available. Resetting the skin pool.");
            return;
        }

        // Select a random skin from the unused ones
        string selectedSkin = unusedSkins[Random.Range(0, unusedSkins.Count)];

        skeleton.SetSkin(selectedSkin);
        skeleton.SetSlotsToSetupPose();
    }

    private bool IsSkinAlreadyUsed(string skin)
    {
        foreach (var seatEntry in mainGameController.availableSeatForCustomers)
        {
            if (seatEntry.Value.customer != null &&
                seatEntry.Value.customer.skeletonAnimation.Skeleton.Skin.Name == skin)
            {
                return true;
            }
        }
        return false;
    }

    private void SetAnimation(CustomerDetails customerDetail)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);
        positiveAnimationDuration = customerDetail.positiveAnimationDuration;
    }

    private void ResetFlagsAndVariables()
    {
        customerCol.enabled = false;
        isCloseEnoughToDelivery = false;
        isOnSeat = false;
        isLeaving = false;
        isRecipeOrder = false;
        moodIndex = 0;
        maxOrderSize = mainGameController.maxOrderHeight;
        Order();
    }

    private void HideHudAndHighlight()
    {
        HudPos.SetActive(false);
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
    }

    private void Order()
    {
        if (mainGameController.customerCounter == mainGameController.spawnSpecialRecipeAfterXCustomer)
        {
            orderManager.OrderByRecipe(mainGameController.maxSpecialRecipeInThisLevel);
            isRecipeOrder = true;
            mainGameController.wasLastOrderByRecipe = true;
            mainGameController.customerCounter = 0;
        }
        else
        {
            if (Random.value > 0.5f && mainGameController.maxSpecialRecipeInThisLevel > 0 && !mainGameController.wasLastOrderByRecipe)
            {
                orderManager.OrderByRecipe(mainGameController.maxSpecialRecipeInThisLevel);
                isRecipeOrder = true;
                mainGameController.wasLastOrderByRecipe = true;
                mainGameController.customerCounter = 0;
            }
            else
            {
                orderManager.OrderRandomProduct(Random.Range(minOrderSize, maxOrderSize + 1));
                isRecipeOrder = false;
                mainGameController.wasLastOrderByRecipe = false;
            }
        }
    }

    private IEnumerator GoToSeat()
    {
        FlipCheck(destination);

        Vector3 targetPosition = new Vector3(destination.x, transform.position.y, destination.z);
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
        {
            if (!GameManager.Instance.isGamePaused)
            {
                if (!isLeaving)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);
                    skeletonAnimation.timeScale = 1;
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                skeletonAnimation.timeScale = 0;
            }

            yield return null;
        }

        if (!isLeaving)
        {
            customerCol.enabled = true;
            isOnSeat = true;
            HudPos.SetActive(true);
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
            patienceBarController.StartDecreasingPatience();

            // play sfx pop up bubble
            AudioManager.Instance.PlaySFX(bubbleOrderPopUpSfx);
        }
    }

    private void FlipCheck(Vector3 target)
    {
        if ((meshRenderer.transform.position.x > target.x && isFacingRight) || (meshRenderer.transform.position.x < target.x && !isFacingRight))
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        meshRenderer.transform.Rotate(0, 180, 0);

        // skeleton flip
        // skeletonAnimation.skeleton.ScaleX *= -1; 
    }

    private void CheckDistanceToDelivery()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDeliveryPlateColliding = customerCol.bounds.Intersects(deliveryPlateCol.bounds);
        isMousePositionColliding = customerCol.bounds.Contains(mousePosition);
        isCloseEnoughToDelivery = isDeliveryPlateColliding && isMousePositionColliding;
    }

    public void StartLeaving()
    {
        if (!isLeaving)
        {
            StartCoroutine(Leave());
        }
    }

    public IEnumerator Leave()
    {
        isLeaving = true;
        isOnSeat = false;
        customerCol.enabled = false;
        yield return StartCoroutine(CheckAndSetWalkAnimation(positiveAnimationDuration));
        mainGameController.availableSeatForCustomers[mySeat] = (null, true);
        HudPos.SetActive(false);
        transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
        FlipCheck(leavePoint);

        ingredientHolder.availableCustomers.Remove(this);

        Vector3 targetPosition = new Vector3(leavePoint.x, transform.position.y, leavePoint.z);
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
        {
            if (!GameManager.Instance.isGamePaused)
            {
                if (isLeaving)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);
                    skeletonAnimation.timeScale = 1;
                }
            }
            else
            {
                skeletonAnimation.timeScale = 0;
            }

            yield return null;
        }

        meshRenderer.transform.eulerAngles = Vector3.zero;
        orderManager.ReleaseAllIngredients();
        customerPool.customerPool.Release(this);
        isLeaving = false;
    }

    private IEnumerator CheckAndSetWalkAnimation(float positiveAnimationDuration)
    {
        Spine.TrackEntry currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
        if (currentTrackEntry != null)
        {
            if (currentTrackEntry.Animation.Name == positiveAnimationName)
            {
                while (currentTrackEntry.TrackTime < positiveAnimationDuration)
                {
                    yield return null;
                }
            }
            else if (currentTrackEntry.Animation.Name == negativeAnimationName)
            {
                yield return new WaitForSpineAnimationComplete(currentTrackEntry);
            }
            
            skeletonAnimation.AnimationState.SetAnimation(0, walkAnimationName, true);
        }
    }

    public bool ReceiveOrder(List<int> myReceivedOrder)
    {
        int[] myOriginalOrder = orderManager.productIngredientsCodes;
        if (myOriginalOrder.Length != myReceivedOrder.Count)
        {
            OrderIsIncorrect();
            return false;
        }

        for (int i = 0; i < myOriginalOrder.Length; i++)
        {
            if (myOriginalOrder[i] != myReceivedOrder[i])
            {
                OrderIsIncorrect();
                return false;
            }
        }

        OrderIsCorrect();
        return true;
    }

    private void OrderIsCorrect()
    {
        if (!isOrderCompleted)
        {
            if (!isLeaving)
            {
                HudPos.SetActive(false);
                skeletonAnimation.AnimationState.SetAnimation(0, positiveAnimationName, false);
            }

            patienceBarController.StopDecreasingPatience();
            StartLeaving();
            EventHandler.CallCorrectOrderEvent(isRecipeOrder);
            EventHandler.CallSetMoneyPosToCustomerPosEvent(transform.position, isRecipeOrder);
            moneySpawner.moneyPool.Get();

            isOrderCompleted = true;
        }
    }

    private void OrderIsIncorrect()
    {
        if (!isOrderCompleted)
        {
            if (!isLeaving)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, negativeAnimationName, false);
                skeletonAnimation.AnimationState.AddAnimation(0, idleAnimationName, true, 0);
            }

            float decreaseValue = customerPatience * decreasePatiencePercentage;
            patienceBarController.DecreaseWithValue(decreaseValue);
            EventHandler.CallIncorrectOrderEvent();
        }
    }

    private void TogglePauseAnim()
    {
        skeletonAnimation.timeScale = skeletonAnimation.timeScale == 0 ? 1 : 0;
    }

    public bool IsOnSeat()
    {
        return isOnSeat;
    }

    public void BaseOnlyServed()
    {
        OrderIsIncorrect();
    }

    public void SetReferences(MainGameController mainGameController, IngredientHolder ingredientHolder, MoneySpawner moneySpawner)
    {
        // Cache component references
        this.mainGameController = mainGameController;
        this.ingredientHolder = ingredientHolder;
        this.moneySpawner = moneySpawner;

        customerCol = GetComponent<Collider2D>();
        customerPool = this.mainGameController.GetComponent<CustomerPool>();
        orderManager = GetComponent<OrderManager>();
        deliveryPlateCol = this.ingredientHolder.GetComponent<Collider2D>();
        patienceBarController = GetComponent<PatienceBarController>();
    }
}
