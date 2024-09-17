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
    [HideInInspector] public bool[] availableSeatForCustomers;
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

        availableSeatForCustomers = new bool[seatPositions.Length];
        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            availableSeatForCustomers[i] = true;
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
                int seatIndex = UnityEngine.Random.Range(0, availableSeatForCustomers.Length);

                if (availableSeatForCustomers[seatIndex])
                {
                    bool isDoubleCustomer = IsDoubleCustomer(doubleCustomerProbability);

                    StartCoroutine(CreateCustomer(seatIndex, isDoubleCustomer));
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

    private bool IsDoubleCustomer(float trueProbability)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        return randomValue < trueProbability;
    }

    private IEnumerator CreateCustomer(int seatIndex, bool isDoubleCustomer)
    {
        canCreateNewCustomer = false;

        if (isDoubleCustomer)
        {
            int[] selectedSeats = GetTwoAvailableSeats(seatIndex);
            if (selectedSeats == null)
            {
                CreateSingleCustomer(seatIndex, 0);
                customerCounter++;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    CreateSingleCustomer(selectedSeats[i], i);
                    customerCounter++;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        else
        {
            CreateSingleCustomer(seatIndex, 0);
            customerCounter++;
        }

        yield return new WaitForSeconds(customerDelay);
        canCreateNewCustomer = true;
    }

    private int[] GetTwoAvailableSeats(int firstSeatIndex)
    {
        if (!availableSeatForCustomers[firstSeatIndex]) return null;

        List<int> availableSeats = new List<int>();
        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            if (i != firstSeatIndex && availableSeatForCustomers[i])
            {
                availableSeats.Add(i);
            }
        }

        if (availableSeats.Count == 0) return null;

        int secondSeatIndex = availableSeats[UnityEngine.Random.Range(0, availableSeats.Count)];
        return new int[] { firstSeatIndex, secondSeatIndex };
    }

    private void CreateSingleCustomer(int seatIndex, int entryIndex)
    {
        CustomerController newCustomer = customerPool.customerPool.Get();

        Vector3 seat = seatPositions[seatIndex].position;
        availableSeatForCustomers[seatIndex] = false;

        int entryPosIndex = entryIndex < customerEntryPos.Length ? entryIndex : UnityEngine.Random.Range(0, customerEntryPos.Length);
        newCustomer.transform.position = new Vector2(customerEntryPos[entryPosIndex].position.x, seat.y);

        // CustomerController customerController = newCustomer.GetComponent<CustomerController>();
        newCustomer.mySeat = seatIndex;
        newCustomer.destination = seat;

        int leavePosIndex = UnityEngine.Random.Range(0, customerLeavePoint.Length);
        newCustomer.leavePoint = new Vector3(customerLeavePoint[leavePosIndex].position.x, seat.y, customerLeavePoint[leavePosIndex].position.z);

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
