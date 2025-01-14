using System;
using System.Collections;
using BazarEsKrim;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MiniGameController : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioClip buttonSfx;
    [SerializeField] private AudioClip winSfx;

    [Header("Score")]
    // [SerializeField] private TextMeshProUGUI scoreText;
    // [SerializeField] private TextMeshProUGUI scoreGameOverText;
    // [SerializeField] private float animSpeed;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private int basketVisualMultiplier = 10;
    private int currentScore;
    private int maxScore;
    private int scoreTier;
    private const float animationTime = .15f;

    // [Header("Timer")]
    // [SerializeField] private TextMeshProUGUI timerText;
    // private float currentTime;
    // private TimeSpan time;

    [Header("Game Over")]
    [SerializeField] private Animator scrollUIAnim;
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private GameObject confettiVFX;
    [SerializeField] private CanvasGroup recipeUnlockUI;
    [SerializeField] private Image recipeVisual;
    [SerializeField] private TextMeshProUGUI recipeText;

    private const float recipeUnlockShowDuration = 2f;
    private const float recipeMoveDuration = 0.8f;
    private const float recipeFadeDuration = 1.5f;
    private const float delayBeforeMove = 0.8f;
    private const string RECIPE_CHARGE_ANIM = "New Recipe Charge";

    private bool canOpenNewRecipePanel;
    private bool isSequencePlaying = false;

    [Header("Spawner")]
    [SerializeField] private GameObject[] objectSpawners;

    [Header("Audios")]
    [SerializeField] private AudioClip newRecipeVO;

    private void Awake()
    {
        int currentLevel = GameManager.Instance.currentLevel;
        maxScore = GameManager.Instance.levelDataLists[currentLevel - 1].miniGameLevelData.maxScore;

        progressSlider.maxValue = maxScore;
        progressSlider.value = 0;

        gameOverWinUI.SetActive(false);
        scrollUIAnim.gameObject.SetActive(false);
        recipeUnlockUI.gameObject.SetActive(false);
        confettiVFX.SetActive(false);

        canOpenNewRecipePanel = false;

        foreach (GameObject spawner in objectSpawners)
        {
            spawner.SetActive(false);
        }

        for (int i = 0; i < GameManager.Instance.levelDataLists[GameManager.Instance.currentLevel - 1].miniGameLevelData.totalObjectSpawner; i++)
        {
            objectSpawners[i].SetActive(true);
        }

        // currentTime = GameManager.Instance.levelDataLists[GameManager.Instance.currentLevel - 1].miniGameLevelData.timer;
    }

    private void OnEnable()
    {
        EventHandler.AddMiniGameScore += AddMiniGameScore;
    }

    private void OnDisable()
    {
        EventHandler.AddMiniGameScore -= AddMiniGameScore;
    }

    private void Start()
    {
        currentScore = 0;
        scoreTier = 0;

        // GameManager.Instance.isGameActive = true;

        // scoreText.text = currentScore.ToString();
    }

    // private void Update()
    // {
    //     if (GameManager.Instance.isGameActive && GameManager.Instance.gameStates == GameStates.MiniGame)
    //     {
    //         ManageTimer();
    //     }
    // }

    // private void ManageTimer()
    // {
    //     currentTime -= Time.deltaTime;
    //     time = TimeSpan.FromSeconds(currentTime);

    //     if (currentTime <= 0f)
    //     {
    //         GameManager.Instance.isGameActive = false;
    //         // levelManager.Win();
    //         StartCoroutine(Win());
    //         EventHandler.CallminiGameWinEvent();

    //         Debug.Log("Countdown time is up!");
    //     }
    //     else
    //     {
    //         timerText.text = time.Seconds.ToString();
    //     }
    // }

    private void AddMiniGameScore()
    {
        currentScore++;

        progressSlider.DOValue(currentScore, animationTime).OnUpdate(() => currentScore = (int)progressSlider.value);
        // scoreText.text = currentScore.ToString();

        if (currentScore % basketVisualMultiplier == 0)
        {
            // Call the score tier event to activate the basket visual
            EventHandler.CallMiniGameScoreTierEvent(scoreTier);

            scoreTier++;
        }

        // if curent score surpase the max score
        if (currentScore >= maxScore)
        {
            GameManager.Instance.isGameActive = false;

            // EventHandler.CallminiGameWinEvent();
            StartCoroutine(Win());
        }
    }

    private IEnumerator Win()
    {
        GameManager.Instance.UnlockLevel();

        gameOverWinUI.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.gameObject.SetActive(true);
        gameOverWinUI.transform.DOScale(1, 0.4f).SetEase(Ease.OutExpo).SetDelay(0.3f);
        gameOverWinUI.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f).OnComplete(() =>
        {
            DOTween.Kill(gameOverWinUI);
        });

        yield return new WaitForSeconds(1f);
        PlayScrollAnim();
    }

    private void PlayScrollAnim()
    {
        scrollUIAnim.gameObject.SetActive(true);

        AudioManager.Instance.PlayVO(newRecipeVO);
    }

    public void SetTouchOn()
    {
        canOpenNewRecipePanel = true;
    }

    public void RecipeRechargeAnim()
    {
        if (canOpenNewRecipePanel)
        {
            AudioManager.Instance.PlaySFX(buttonSfx);

            scrollUIAnim.Play(RECIPE_CHARGE_ANIM);
        }
    }

    public void OpenNewRecipePanel()
    {
        if (!canOpenNewRecipePanel || isSequencePlaying)
        {
            return;
        }

        isSequencePlaying = true; // Set flag to prevent overlapping sequences

        scrollUIAnim.gameObject.SetActive(false);

        int currentLevel = GameManager.Instance.currentLevel;
        int recipeIndex = currentLevel - 1;
        float recipeInitialX = recipeVisual.transform.localPosition.x;

        recipeText.text = GameManager.Instance.recipeLists[recipeIndex].recipeName;
        recipeVisual.sprite = GameManager.Instance.recipeLists[recipeIndex].recipeSprite;
        recipeVisual.SetNativeSize();

        recipeUnlockUI.transform.localScale = Vector3.zero;
        recipeUnlockUI.alpha = 0;

        confettiVFX.SetActive(true);
        AudioManager.Instance.PlaySFX(winSfx);

        // Kill any ongoing animations for recipeVisual
        recipeVisual.transform.DOKill();

        // Create a sequence for chaining animations
        Sequence sequence = DOTween.Sequence();

        // First, close the gameOverWinUI panel
        sequence.AppendCallback(() =>
            {
                // Set recipe visual to center horizontally
                recipeVisual.transform.localPosition = new Vector3(0, recipeVisual.transform.localPosition.y, recipeVisual.transform.localPosition.z);

                recipeUnlockUI.gameObject.SetActive(true);
            })

            // Show the recipeUnlockUI with scaling animation
            .Append(recipeUnlockUI.transform.DOScale(1, recipeUnlockShowDuration).SetEase(Ease.OutExpo))

            // Move recipe visual after a delay
            .AppendInterval(delayBeforeMove)
            .Append(recipeVisual.transform.DOLocalMoveX(recipeInitialX, recipeMoveDuration).SetEase(Ease.InOutQuad))

            // Fade in the recipeUnlockUI
            .Append(recipeUnlockUI.DOFade(1, recipeFadeDuration).SetEase(Ease.OutSine))
            .AppendCallback(() =>
            {
                levelManager.AddMiniGameOverButtonEvent();
            })

            // Reset the sequence flag after completion
            .OnComplete(() =>
            {
                isSequencePlaying = false;
            });

        // Play the sequence
        sequence.Play();
    }
}
