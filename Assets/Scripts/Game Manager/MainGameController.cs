using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainGameController : MonoBehaviour
{
    [HideInInspector] public int maxOrderHeight;
    [HideInInspector] public int spawnSpecialRecipeAfterXCustomer;
    [HideInInspector] public int maxSpecialRecipeInThisLevel;
    [HideInInspector] public int customerCounter;
    [HideInInspector] public int deliveryQueueIngredient;
    [HideInInspector] public bool wasLastOrderByRecipe;

    public Dictionary<int, (CustomerController customer, bool isAvailable)> availableSeatForCustomers;
    private List<int> availableSeats = new List<int>(); // Cache available seats to avoid dictionary lookups
    private List<int> cachedAvailableSeats = new List<int>(); // Cached list for available seats

    [HideInInspector] public List<int> deliveryQueueIngredientsContent = new List<int>();

    [Header("Level Manager")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject[] characters;

    [Header("Customer Params")]
    [SerializeField] private Transform[] customerEntryPos;
    [SerializeField] private Transform[] customerLeavePoint;
    [SerializeField] private Transform[] seatPositions;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float timer;

    [Header("Progress")]
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private Slider progressSlider;
    private int pointPerCustomer;
    private int specialRecipePoint;
    private int maxPoint;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private GameObject gameOverLoseUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject ingredientToday;

    private int progressCount;
    private int customerDelay;
    private TimeSpan time;
    private float currentTime;
    private float doubleCustomerProbability;
    private bool canCreateNewCustomer;
    private CustomerPool customerPool; // Reference to the CustomerPool
    private const float animationTime = .15f;

    // Cache WaitForSeconds to avoid memory allocation spikes
    private WaitForSeconds customerDelayWait;

    private void Awake()
    {
        customerPool = GetComponent<CustomerPool>();

        foreach (GameObject character in characters)
        {
            character.SetActive(false);
        }

        LevelDataMainGame levelData = GameManager.Instance.levelDataLists[GameManager.Instance.currentLevel - 1].mainGameLevelData;
        maxOrderHeight = levelData.maxOrderHeight;
        spawnSpecialRecipeAfterXCustomer = levelData.spawnSpecialRecipeAfterXCustomer;
        maxSpecialRecipeInThisLevel = levelData.sO_RecipeList.Length;
        customerDelay = levelData.customerDelay;
        customerDelayWait = new WaitForSeconds(customerDelay); // Cache WaitForSeconds object
        doubleCustomerProbability = levelData.doubleCustomerProbability;
        pointPerCustomer = levelData.pointPerCustomer;
        specialRecipePoint = levelData.specialRecipePoint;
        maxPoint = levelData.maxPoint;
        timer = levelData.timer;
        characters[(int)levelData.character].SetActive(true);

        customerCounter = 0;
        deliveryQueueIngredient = 0;
        deliveryQueueIngredientsContent.Clear();
        canCreateNewCustomer = false;
        wasLastOrderByRecipe = false;

        availableSeatForCustomers = new Dictionary<int, (CustomerController, bool)>();
        InitializeAvailableSeats();

        progressSlider.maxValue = maxPoint;
        progressSlider.value = 0;

        ingredientToday.SetActive(true);
        gameOverWinUI.transform.parent.gameObject.SetActive(false);
        gameOverLoseUI.transform.parent.gameObject.SetActive(false);
        pauseMenuUI.SetActive(false);
    }

    private void InitializeAvailableSeats()
    {
        for (int i = 0; i < seatPositions.Length; i++)
        {
            availableSeatForCustomers.Add(i, (null, true));  // Seat is available with no customer assigned
            availableSeats.Add(i); // Add all seats to available seats list
        }
    }

    private void OnEnable()
    {
        EventHandler.CorrectOrder += CorrectOrderEvent;
        EventHandler.IncorrectOrder += IncorrectOrderEvent;
    }

    private void OnDisable()
    {
        EventHandler.CorrectOrder -= CorrectOrderEvent;
        EventHandler.IncorrectOrder -= IncorrectOrderEvent;
    }

    private void Start()
    {
        currentTime = timer;
        progressCount = 0;
        pointText.text = progressCount.ToString();
        targetText.text = maxPoint.ToString();

        time = TimeSpan.FromSeconds(currentTime);
        timerText.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);

        canCreateNewCustomer = true; // Start customer creation
    }

    private void Update()
    {
        if (GameManager.Instance.isGameActive && !GameManager.Instance.isGamePaused && GameManager.Instance.gameStates == GameStates.MainGame)
        {
            ManageTimer();

            if (canCreateNewCustomer)
            {
                int availableSeatIndex = GetAvailableSeatIndex();
                if (availableSeatIndex != -1)
                {
                    bool isDoubleCustomer = IsDoubleCustomer(doubleCustomerProbability);
                    StartCoroutine(CreateCustomer(availableSeatIndex, isDoubleCustomer));
                }
            }
        }
    }

    private void ManageTimer()
    {
        currentTime -= Time.deltaTime;
        time = TimeSpan.FromSeconds(currentTime);

        if (currentTime <= 0f)
        {
            GameManager.Instance.isGameActive = false;
            EventHandler.CallChaseCustomerEvent();
            StartCoroutine(LoseAnim());
            Debug.Log("Countdown time is up!");
        }
        else
        {
            timerText.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);
        }
    }

    private int GetAvailableSeatIndex()
    {
        // Clear the list instead of creating a new one
        cachedAvailableSeats.Clear();

        // Collect all available seats
        foreach (var seat in availableSeatForCustomers)
        {
            if (seat.Value.isAvailable)
            {
                cachedAvailableSeats.Add(seat.Key);
            }
        }

        // If there are no available seats, return -1
        if (cachedAvailableSeats.Count == 0)
        {
            return -1;
        }

        // Return a random seat from the available ones
        int randomIndex = UnityEngine.Random.Range(0, cachedAvailableSeats.Count);
        return cachedAvailableSeats[randomIndex];
    }

    private bool IsDoubleCustomer(float probability)
    {
        return UnityEngine.Random.value < probability;
    }

    private IEnumerator CreateCustomer(int availableSeat, bool isDoubleCustomer)
    {
        canCreateNewCustomer = false;

        if (!GameManager.Instance.isGameActive) // Early exit if game is not active
            yield break;

        if (isDoubleCustomer)
        {
            var (seat1, seat2) = GetTwoAvailableSeats();
            if (seat1 != -1 && seat2 != -1) // Two seats available
            {
                Vector3 entry1 = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
                Vector3 entry2 = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;

                CreateSingleCustomer(seat1, entry1);
                yield return new WaitForSeconds(0.1f);
                CreateSingleCustomer(seat2, entry2);
            }
            else if (seat1 != -1)
            {
                Vector3 entry = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
                CreateSingleCustomer(seat1, entry);
            }
        }
        else
        {
            Vector3 entry = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
            CreateSingleCustomer(availableSeat, entry);
        }

        yield return customerDelayWait; // Reuse cached WaitForSeconds object
        canCreateNewCustomer = true;
    }

    private (int firstSeat, int secondSeat) GetTwoAvailableSeats()
    {
        int firstSeat = -1, secondSeat = -1;
        foreach (var seat in availableSeatForCustomers)
        {
            if (seat.Value.isAvailable)
            {
                if (firstSeat == -1)
                {
                    firstSeat = seat.Key;
                }
                else
                {
                    secondSeat = seat.Key;
                    break; // Found two available seats
                }
            }
        }
        return (firstSeat, secondSeat);
    }

    private void CreateSingleCustomer(int seatIndex, Vector3 entryPosition)
    {
        CustomerController newCustomer = customerPool.customerPool.Get();
        Vector3 seatPosition = seatPositions[seatIndex].position;

        availableSeatForCustomers[seatIndex] = (newCustomer, false); // Mark seat as occupied
        availableSeats.Remove(seatIndex); // Update available seats

        newCustomer.transform.position = new Vector2(entryPosition.x, seatPosition.y);
        newCustomer.mySeat = seatIndex;
        newCustomer.destination = seatPosition;
        newCustomer.leavePoint = customerLeavePoint[UnityEngine.Random.Range(0, customerLeavePoint.Length)].position;

        newCustomer.Init();
    }

    private void CorrectOrderEvent(bool isRecipeOrder)
    {
        progressCount += isRecipeOrder ? specialRecipePoint : pointPerCustomer;
        pointText.text = progressCount.ToString();
        IncreaseSliderValue(progressCount);

        if (progressCount >= maxPoint)
        {
            GameManager.Instance.isGameActive = false;
            EventHandler.CallChaseCustomerEvent();
            StartCoroutine(WinAnim());
        }
    }

    private void IncorrectOrderEvent()
    {
        // Handle incorrect order logic here
    }

    private IEnumerator WinAnim()
    {
        yield return new WaitForSeconds(1f);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);
        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        yield return new WaitForSeconds(1f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true);
    }

    private IEnumerator LoseAnim()
    {
        yield return new WaitForSeconds(1);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
    }

    public void IncreaseSliderValue(float value)
    {
        if (Mathf.Abs(progressSlider.value - value) > 0.01f) // Update only when there's a noticeable change
        {
            progressSlider.DOValue(value, animationTime);
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenuUI.SetActive(true);
    }

    public void ClosePauseMenu()
    {
        if (ingredientToday.activeSelf)
        {
            ingredientToday.SetActive(false);
            GameManager.Instance.isGameActive = true;
            GameManager.Instance.isGamePaused = false;
            canCreateNewCustomer = true;
        }
        else
        {
            pauseMenuUI.SetActive(false);
        }
    }
}
