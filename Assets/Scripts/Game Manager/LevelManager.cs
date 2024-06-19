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
    [HideInInspector] public int maxOrderHeight = 6; //maximum available slots in delivery queue (set in init)
    [HideInInspector] public int spawnSpecialRecipeAfterXCustomer;
    [HideInInspector] public int customerCounter;	
    [HideInInspector] public int deliveryQueueIngredient;	//number of items in delivery queue
    [HideInInspector] public bool[] availableSeatForCustomers;
    [HideInInspector] public List<int> deliveryQueueIngredientsContent = new List<int>();   //contents of delivery queue

    [SerializeField] private GameObject[] customers; //list of all available customers (different patience)
    [SerializeField] private Transform[] customerEntryPos;
    [SerializeField] private Transform[] customerLeavePoint;
    [SerializeField] private Transform[] seatPositions;

    // [Header("Health")]
    // [SerializeField] private GameObject healthContainer;
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float timer;

    [Header("Progress")]
    // [SerializeField] private GameObject progressContainer;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private TextMeshProUGUI targetText;
    private int pointPerCustomer;
    private int maxPoint;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private GameObject gameOverLoseUI;

    // private GameObject[] hearts;
    // private int healthCount;
    // private GameObject[] progresses;
    private int progressCount;
    private int customerDelay; //delay between creating a new customer (smaller number leads to faster customer creation). Optimal value should be between 5 (Always crowded) and 15 (Not so crowded) seconds. 
    private TimeSpan time;
    private float currentTime;
    private float doubleCustomerProbability;
    private bool canCreateNewCustomer;	//flag to prevent double calls to functions

    private void Awake()
    {
        LevelData levelData = GameManager.Instance.levelDataList.levelDataList[GameManager.Instance.currentLevel - 1];
        maxOrderHeight = levelData.maxOrderHeight;
        spawnSpecialRecipeAfterXCustomer = levelData.spawnSpecialRecipeAfterXCustomer;
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
        //set all seats as available at the start of the game. No seat is taken yet.        
        for (int i = 0; i < availableSeatForCustomers.Length; i++)
        {
            availableSeatForCustomers[i] = true;
        }

        // int heartContainerChildCount = healthContainer.transform.childCount;
        // hearts = new GameObject[heartContainerChildCount];
        // for (int i = 0; i < heartContainerChildCount; i++)
        // {
        //     hearts[i] = healthContainer.transform.GetChild(i).gameObject;
        //     hearts[i].transform.GetChild(0).gameObject.SetActive(true);
        //     healthCount++;
        // }

        // int progressContainerChildCount = progressContainer.transform.childCount;
        // progresses = new GameObject[progressContainerChildCount];
        // for (int i = 0; i < progressContainerChildCount; i++)
        // {
        //     progresses[i] = progressContainer.transform.GetChild(i).gameObject;
        //     progresses[i].transform.GetChild(0).gameObject.SetActive(false);
        //     progressCount++;
        // }
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

        yield return new WaitForSeconds(2); // first delay until customer spawn

        GameManager.Instance.isGameActive = true;

        canCreateNewCustomer = true;
    }

    private void Update()
    {
        // Create a new customer if there is a free seat and the game is not finished yet
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
        // Update countdown time
        currentTime -= Time.deltaTime;
        time = TimeSpan.FromSeconds(currentTime);

        // If countdown time is up, stop the timer and do something
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
        // Generate a random float between 0.0 and 1.0
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        // Return true if the random value is less than the specified probability
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
                // Not enough available seats for double customers, create a single customer instead
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

        if(customerCounter > spawnSpecialRecipeAfterXCustomer)
        {
            customerCounter = 0;
        }

        yield return new WaitForSeconds(customerDelay);
        canCreateNewCustomer = true;
    }


    private int[] GetTwoAvailableSeats(int firstSeatIndex)
    {
        // Ensure the first seat is available
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
        // Select a random customer
        GameObject tmpCustomer = customers[UnityEngine.Random.Range(0, customers.Length)];

        // Select the seat position
        Vector3 seat = seatPositions[seatIndex].position;
        availableSeatForCustomers[seatIndex] = false;

        // Select the entry position
        int entryPosIndex = entryIndex < customerEntryPos.Length ? entryIndex : UnityEngine.Random.Range(0, customerEntryPos.Length);
        GameObject newCustomer = Instantiate(tmpCustomer, new Vector2(customerEntryPos[entryPosIndex].position.x, seat.y), Quaternion.identity);

        // Configure the customer controller
        CustomerController customerController = newCustomer.GetComponent<CustomerController>();
        customerController.mySeat = seatIndex;
        customerController.destination = seat;

        // Set the customer's leave point
        int leavePosIndex = UnityEngine.Random.Range(0, customerLeavePoint.Length);
        customerController.leavePoint = new Vector3(customerLeavePoint[leavePosIndex].position.x, seat.y, customerLeavePoint[leavePosIndex].position.z);
    }

    private void CorrectOrderEvent()
    {
        // for (int i = 0; i < progresses.Length; i++)
        // {
        //     if (i < progressCount)
        //     {
        //         progressCount--;

        //         progresses[progressCount].transform.GetChild(0).gameObject.SetActive(true);

        //         Vector3 punch = new Vector3(.7f, .7f, .7f);
        //         progresses[progressCount].transform.GetChild(0).DOPunchScale(punch, 0.3f, 0, 0.3f);

        //         // AudioManager.Instance.PlaySFX(correctSfx);

        //         break;
        //     }
        // }

        progressCount += pointPerCustomer;
        pointText.text = progressCount.ToString();

        if (progressCount >= maxPoint)
        {
            GameManager.Instance.isGameActive = false;

            StartCoroutine(Win()); // change with Win(); if there is an animation when winning
        }
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(0.7f);

        // Anim
        gameOverWinUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);
        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverWinUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        // string sceneName = SceneManager.GetActiveScene().name;
        // int level;
        // if (int.TryParse(sceneName.Substring("Level".Length), out level) && level == GameManager.Instance.LoadUnlockedLevel() && level < GameManager.Instance.maxLevel)
        // {
        //     GameManager.Instance.canStageButtonAnim = true;
        //     Debug.Log("Unlocked Anim");
        // }

        // Unlock level selection
        // GameManager.Instance.UnlockLevelSelection();

        yield return new WaitForSeconds(1.5f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true); // the confetti
        // AudioManager.Instance.PlaySFX(winSfx);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Win");
    }

    private void IncorrectOrderEvent()
    {
        // for (int i = 0; i < hearts.Length; i++)
        // {
        //     if (i < healthCount)
        //     {
        //         healthCount--;

        //         Vector3 punch = new Vector3(.7f, .7f, .7f);
        //         hearts[healthCount].transform.GetChild(0).DOPunchScale(punch, .3f, 0, 0.3f).OnComplete(() =>
        //         {
        //             hearts[healthCount].transform.GetChild(0).gameObject.SetActive(false);
        //         });

        //         break;
        //     }
        // }

        // if (healthCount <= 0)
        // {
        //     GameManager.Instance.isGameActive = false;

        //     // Lose();
        //     StartCoroutine(Lose()); // change with Lose(); if there is an animation when winning
        // }
    }

    private IEnumerator Lose()
    {
        yield return new WaitForSeconds(1);

        // Anim
        gameOverLoseUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverLoseUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        // AudioManager.Instance.PlaySFX(loseSfx);

        EventHandler.CallChaseCustomerEvent();

        Debug.Log("Game Over - Lose");
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
