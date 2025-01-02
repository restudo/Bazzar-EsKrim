using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine;
using Spine.Unity;
using BazarEsKrim;

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

    [Header("Gameplay Params")]
    [SerializeField] private IngredientHolder ingredientHolder;
    [SerializeField] private SpriteRenderer[] bgSprite;
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
    [SerializeField] private GameObject confettiVFX;

    [Header("Audios")]
    [SerializeField] private AudioClip correctOrderSfx;
    [SerializeField] private AudioClip incorrectOrderSfx;
    [SerializeField] private AudioClip coneDropSfx;
    [SerializeField] private AudioClip flavorDropSfx;
    [SerializeField] private AudioClip toppingDropSfx;

    private int progressCount;
    private int customerDelay;
    private TimeSpan time;
    private float currentTime;
    private float[] bgSpriteAlphas; // Cache alpha values for the sprites
    private float doubleCustomerProbability;
    private bool canCreateNewCustomer;
    private CustomerPool customerPool; // Reference to the CustomerPool
    private GameObject character;
    private SkeletonAnimation skeleton;

    private const float animationTime = .15f;
    private const string NORMAL_CHAR_ANIM = "normal";
    private const string CORRECT_CHAR_ANIM = "correct";
    private const string WRONG_CHAR_ANIM = "wrong";

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
        character = characters[(int)levelData.character];
        skeleton = character.GetComponent<SkeletonAnimation>();

        customerCounter = 0;
        deliveryQueueIngredient = 0;
        deliveryQueueIngredientsContent.Clear();
        canCreateNewCustomer = false;
        wasLastOrderByRecipe = false;

        progressSlider.maxValue = maxPoint;
        progressSlider.value = 0;

        ingredientToday.SetActive(true);
        gameOverWinUI.transform.parent.gameObject.SetActive(false);
        gameOverLoseUI.transform.parent.gameObject.SetActive(false);
        pauseMenuUI.SetActive(false);
        confettiVFX.SetActive(false);

        availableSeatForCustomers = new Dictionary<int, (CustomerController, bool)>();
        InitializeAvailableSeats();

        SetInitialBgVisibility();
    }

    private void InitializeAvailableSeats()
    {
        for (int i = 0; i < seatPositions.Length; i++)
        {
            availableSeatForCustomers.Add(i, (null, true));  // Seat is available with no customer assigned
            availableSeats.Add(i); // Add all seats to available seats list
        }
    }

    private void SetInitialBgVisibility()
    {
        bgSprite[0].color = new Color(1f, 1f, 1f, 1f); // Sprite 1 fully visible at start
        bgSprite[1].color = new Color(1f, 1f, 1f, 0f); // Sprite 2 fully transparent at start
        bgSprite[2].color = new Color(1f, 1f, 1f, 0f); // Sprite 3 fully transparent at start
    }

    private void SetClosedBgVisibility(float[] spriteAlphas)
    {
        for (int i = 0; i < bgSprite.Length; i++)
        {
            Color color = bgSprite[i].color;
            color.a = spriteAlphas[i];  // Set the alpha to the cached value
            bgSprite[i].color = color;  // Apply the updated color back to the sprite
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

        character.SetActive(true);

        SetCharacterAnim(NORMAL_CHAR_ANIM, true);
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

    private void SetCharacterAnim(string charAnim, bool isLooping)
    {
        skeleton.AnimationState.SetAnimation(0, charAnim, isLooping);
    }

    private void AddCharacterAnim(string charAnim, bool isLooping)
    {
        skeleton.AnimationState.AddAnimation(0, charAnim, isLooping, 0);
    }

    private void ManageTimer()
    {
        currentTime -= Time.deltaTime;
        time = TimeSpan.FromSeconds(currentTime);

        if (currentTime <= 0f)
        {
            GameManager.Instance.isGameActive = false;
            EventHandler.CallChaseCustomerEvent();
            SetClosedBgVisibility(bgSpriteAlphas);
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

                // Get a different entry position for the second customer
                Vector3 entry2;
                do
                {
                    entry2 = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
                } while (entry1 == entry2);  // Ensure entry2 is not the same as entry1

                yield return CreateSingleCustomer(seat1, entry1);
                yield return CreateSingleCustomer(seat2, entry2);
            }
            else if (seat1 != -1)
            {
                Vector3 entry = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
                yield return CreateSingleCustomer(seat1, entry);
            }
        }
        else
        {
            Vector3 entry = customerEntryPos[UnityEngine.Random.Range(0, customerEntryPos.Length)].position;
            yield return CreateSingleCustomer(availableSeat, entry);
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

    private IEnumerator CreateSingleCustomer(int seatIndex, Vector3 entryPosition)
    {
        CustomerController newCustomer = customerPool.customerPool.Get();
        ingredientHolder.availableCustomers.Add(newCustomer);

        Vector3 seatPosition = seatPositions[seatIndex].position;
        newCustomer.transform.position = new Vector2(entryPosition.x, seatPosition.y);

        newCustomer.mySeat = seatIndex;
        newCustomer.destination = seatPosition;
        newCustomer.leavePoint = customerLeavePoint[UnityEngine.Random.Range(0, customerLeavePoint.Length)].position;

        newCustomer.Init();

        availableSeatForCustomers[seatIndex] = (newCustomer, false); // Mark seat as occupied
        availableSeats.Remove(seatIndex); // Update available seats

        yield return null;
    }

    private void CorrectOrderEvent(bool isRecipeOrder)
    {
        // play sfx correct order
        AudioManager.Instance.PlaySFX(correctOrderSfx);

        progressCount += isRecipeOrder ? specialRecipePoint : pointPerCustomer;
        pointText.text = progressCount.ToString();
        IncreaseSliderValue(progressCount);

        if (progressCount >= maxPoint)
        {
            GameManager.Instance.isGameActive = false;
            EventHandler.CallChaseCustomerEvent();
            SetClosedBgVisibility(bgSpriteAlphas);
            StartCoroutine(WinAnim());
        }

        SetCharacterAnim(CORRECT_CHAR_ANIM, false);
        AddCharacterAnim(NORMAL_CHAR_ANIM, true);
    }

    private void IncorrectOrderEvent()
    {
        // play sfx correct order
        AudioManager.Instance.PlaySFX(incorrectOrderSfx);

        SetCharacterAnim(WRONG_CHAR_ANIM, false);
        AddCharacterAnim(NORMAL_CHAR_ANIM, true);
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
        confettiVFX.SetActive(true);
    }

    private IEnumerator LoseAnim()
    {
        yield return new WaitForSeconds(1);
        gameOverLoseUI.transform.parent.localScale = Vector3.zero;
        gameOverLoseUI.transform.parent.gameObject.SetActive(true);
        gameOverLoseUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
    }

    private void FadeSprites()
    {
        int numberOfSprites = bgSprite.Length; // Number of sprites

        // Divide the total time evenly across all sprites, minus the time needed for the fades
        float totalFadeTime = timer * (bgSprite.Length / 10f); // Reserve 30% of the total timer for fades
        float visibleDurationForEachBgSprite = (timer - totalFadeTime) / numberOfSprites;

        // Calculate the duration for each fade transition based on the remaining time
        float bgFadeDuration = totalFadeTime / (numberOfSprites - 1); // For 3 sprites, there are 2 fades

        // Cache the current alpha values of the sprites
        bgSpriteAlphas = new float[numberOfSprites];
        for (int i = 0; i < numberOfSprites; i++)
        {
            bgSpriteAlphas[i] = bgSprite[i].color.a; // Assuming you are fading SpriteRenderer color alpha
        }

        // Chain DOTween sequence to fade between sprites
        Sequence sequence = DOTween.Sequence();

        // Sprite 1: visible for calculated duration, then fade to Sprite 2
        sequence.Append(bgSprite[0].DOFade(1f, 0f))  // Ensure Sprite 1 starts fully visible
                .AppendInterval(visibleDurationForEachBgSprite)     // Wait for the calculated visible time
                .Append(bgSprite[0].DOFade(0f, bgFadeDuration)) // Fade out sprite 1
                .Join(bgSprite[1].DOFade(1f, bgFadeDuration))   // Fade in sprite 2

                // Sprite 2: visible for calculated duration, then fade to Sprite 3
                .AppendInterval(visibleDurationForEachBgSprite)     // Wait for the calculated visible time
                .Append(bgSprite[1].DOFade(0f, bgFadeDuration)) // Fade out sprite 2
                .Join(bgSprite[2].DOFade(1f, bgFadeDuration))   // Fade in sprite 3

                // Sprite 3: visible for the final calculated duration (no fade after)
                .AppendInterval(visibleDurationForEachBgSprite)
                .OnUpdate(() =>
                {
                    // Cache the current alpha value of each sprite when the sequence is killed
                    for (int i = 0; i < numberOfSprites; i++)
                    {
                        bgSpriteAlphas[i] = bgSprite[i].color.a;
                    }
                });
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

            // Start the fade sequence
            FadeSprites();
        }
        else
        {
            pauseMenuUI.SetActive(false);
        }
    }

    public void PlaySfxByIngredientType(IngredientType ingredientType)
    {
        switch (ingredientType)
        {
            case IngredientType.Base:
                AudioManager.Instance.PlaySFX(coneDropSfx);
                break;
            case IngredientType.Flavor:
                AudioManager.Instance.PlaySFX(flavorDropSfx);
                break;
            case IngredientType.Topping:
                AudioManager.Instance.PlaySFX(toppingDropSfx);
                break;
            default:
                break;
        }
    }
}
