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
    // [HideInInspector] public bool[] availableSeatForCustomers;
    public Dictionary<int, (CustomerController customer, bool isAvailable)> availableSeatForCustomers; // Use int for seat index, and tuple (CustomerController, bool) for storing customer and availability
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
        for (int i = 0; i < seatPositions.Length; i++)
        {
            availableSeatForCustomers.Add(i, (null, true));  // Seat is available with no customer assigned
        }

        progressSlider.maxValue = maxPoint;
        progressSlider.value = 0;

        ingredientToday.SetActive(true);
        gameOverWinUI.transform.parent.gameObject.SetActive(false);
        gameOverLoseUI.transform.parent.gameObject.SetActive(false);
        pauseMenuUI.SetActive(false);
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

        //TODO: change with countdown or else
        // yield return new WaitForSeconds(2);
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
        foreach (var seat in availableSeatForCustomers)
        {
            if (seat.Value.isAvailable)
            {
                return seat.Key;
            }
        }
        return -1;
    }

    private bool IsDoubleCustomer(float probability)
    {
        return UnityEngine.Random.value < probability;
    }

    private IEnumerator CreateCustomer(int availableSeat, bool isDoubleCustomer)
    {
        canCreateNewCustomer = false;

        if (isDoubleCustomer)
        {
            // Ensure you have two available seats
            var selectedSeats = GetTwoAvailableSeats();
            if (selectedSeats.Count == 0)
            {
                // Only one seat available, create a single customer
                CreateSingleCustomer(availableSeat, customerEntryPos[0].position);  // Default to first entry position
            }
            else
            {
                // Create two customers at two different entry positions
                CreateSingleCustomer(selectedSeats[0], customerEntryPos[0].position);  // First customer at first entry
                yield return new WaitForSeconds(0.1f);  // Small delay between customers

                CreateSingleCustomer(selectedSeats[1], customerEntryPos[1].position);  // Second customer at second entry
            }
        }
        else
        {
            // Single customer case
            CreateSingleCustomer(availableSeat, customerEntryPos[0].position);  // Default to first entry position
        }

        yield return new WaitForSeconds(customerDelay);
        canCreateNewCustomer = true;
    }


    private List<int> GetTwoAvailableSeats()
    {
        List<int> availableSeats = new List<int>();
        foreach (var seat in availableSeatForCustomers)
        {
            if (seat.Value.isAvailable)
            {
                availableSeats.Add(seat.Key);
            }
        }

        if (availableSeats.Count < 2) return new List<int>();
        return availableSeats.GetRange(0, 2);
    }

    private void CreateSingleCustomer(int seatIndex, Vector3 entryPosition)
    {
        CustomerController newCustomer = customerPool.customerPool.Get();
        Vector3 seatPosition = seatPositions[seatIndex].position;

        availableSeatForCustomers[seatIndex] = (newCustomer, false);

        // Set customer starting position from the entry point
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

        IncreaseSliderValue(pointPerCustomer);

        if (progressCount >= maxPoint)
        {
            GameManager.Instance.isGameActive = false;

            EventHandler.CallChaseCustomerEvent();

            StartCoroutine(WinAnim());
        }
    }

    private IEnumerator WinAnim()
    {
        yield return new WaitForSeconds(1f);

        gameOverWinUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);
        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverWinUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        yield return new WaitForSeconds(1f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true);

        Debug.Log("Game Over - Win");
    }

    private IEnumerator LoseAnim()
    {
        yield return new WaitForSeconds(1);

        gameOverLoseUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverLoseUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        Debug.Log("Game Over - Lose");
    }

    private void IncorrectOrderEvent()
    {
        // Handle incorrect order logic here
    }

    public void IncreaseSliderValue(float value)
    {
        // Update the slider value
        progressSlider.DOValue(progressCount, animationTime).OnUpdate(() => progressCount = (int)progressSlider.value);
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

            // TODO: add timer or countdown

            GameManager.Instance.isGameActive = true;
            GameManager.Instance.isGamePaused = false;
            canCreateNewCustomer = true;
        }
        else
        {
            // TODO pause menu set active false
            pauseMenuUI.SetActive(false);
        }
    }
}
