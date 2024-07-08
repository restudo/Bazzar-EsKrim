using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [HideInInspector] public int maxOrderHeight = 6;
    [HideInInspector] public int spawnSpecialRecipeAfterXCustomer;
    [HideInInspector] public int maxSpecialRecipeInThisLevel;
    [HideInInspector] public int customerCounter;	
    [HideInInspector] public int deliveryQueueIngredient;	
    [HideInInspector] public bool[] availableSeatForCustomers;
    [HideInInspector] public List<int> deliveryQueueIngredientsContent = new List<int>();

    [SerializeField] private Transform[] customerEntryPos;
    [SerializeField] private Transform[] customerLeavePoint;
    [SerializeField] private Transform[] seatPositions;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float timer;

    [Header("Progress")]
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private TextMeshProUGUI targetText;
    private int pointPerCustomer;
    private int maxPoint;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private GameObject gameOverLoseUI;

    private int progressCount;
    private int customerDelay;
    private TimeSpan time;
    private float currentTime;
    private float doubleCustomerProbability;
    private bool canCreateNewCustomer;
    private CustomerPool customerPool; // Reference to the CustomerPool

    private void Awake()
    {
        customerPool = GetComponent<CustomerPool>();

        LevelData levelData = GameManager.Instance.levelDataList.levelDataList[GameManager.Instance.currentLevel - 1];
        maxOrderHeight = levelData.maxOrderHeight;
        spawnSpecialRecipeAfterXCustomer = levelData.spawnSpecialRecipeAfterXCustomer;
        maxSpecialRecipeInThisLevel = levelData.recipeList.Length;
        customerDelay = levelData.customerDelay;
        doubleCustomerProbability = levelData.doubleCustomerProbability;
        pointPerCustomer = levelData.pointPerCustomer;
        maxPoint = levelData.maxPoint;
        timer = levelData.timer;

        customerCounter = 0;
        deliveryQueueIngredient = 0;
        deliveryQueueIngredientsContent.Clear();
        canCreateNewCustomer = false;

        availableSeatForCustomers = new bool[seatPositions.Length];
        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            availableSeatForCustomers[i] = true;
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

    private IEnumerator Start()
    {
        currentTime = timer;
        progressCount = 0;
        pointText.text = progressCount.ToString();
        targetText.text = maxPoint.ToString();

        time = TimeSpan.FromSeconds(currentTime);
        timerText.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);

        yield return new WaitForSeconds(2);

        GameManager.Instance.isGameActive = true;
        canCreateNewCustomer = true;
    }

    private void Update()
    {
        if (GameManager.Instance.isGameActive)
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
            StartCoroutine(Lose());
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

        if (customerCounter > spawnSpecialRecipeAfterXCustomer)
        {
            customerCounter = 0;
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

    private void CorrectOrderEvent()
    {
        progressCount += pointPerCustomer;
        pointText.text = progressCount.ToString();

        if (progressCount >= maxPoint)
        {
            GameManager.Instance.isGameActive = false;
            StartCoroutine(Win());
        }
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(0.7f);

        gameOverWinUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);
        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverWinUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        yield return new WaitForSeconds(1.5f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Win");
    }

    private void IncorrectOrderEvent()
    {
        // Handle incorrect order logic here
    }

    private IEnumerator Lose()
    {
        yield return new WaitForSeconds(1);

        gameOverLoseUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverLoseUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Lose");
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadToLevelSelection()
    {
        SceneController.Instance.FadeAndLoadScene(Scenes.LevelSelection);
    }

    // public void LoadToNextLevel()
    // {

    // }
}
