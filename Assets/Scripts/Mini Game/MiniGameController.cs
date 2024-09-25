using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MiniGameController : MonoBehaviour
{
    [Header("Level Manager")]
    [SerializeField] private LevelManager levelManager;

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
    [SerializeField] private GameObject gameOverWinUI;
    [SerializeField] private GameObject confettiVFX;
    [SerializeField] private CanvasGroup recipeUnlockUI;
    [SerializeField] private Image recipeVisual;
    [SerializeField] private TextMeshProUGUI recipeText;
    private const float recipeUnlockShowDuration = 2f;
    private const float recipeMoveDuration = 0.8f;
    private const float recipeFadeDuration = 1.5f;
    private const float delayBeforeMove = 0.8f;
    // private const float gameOverWinCloseDuration = 1f;

    [Space(5)]
    // [SerializeField] private RecipeDetails[] recipes;

    [Header("Spawner")]
    [SerializeField] private GameObject[] objectSpawners;

    private void Awake()
    {
        int currentLevel = GameManager.Instance.currentLevel;
        maxScore = GameManager.Instance.levelDataLists[currentLevel - 1].miniGameLevelData.maxScore;

        progressSlider.maxValue = maxScore;
        progressSlider.value = 0;

        gameOverWinUI.gameObject.SetActive(false);
        recipeUnlockUI.gameObject.SetActive(false);
        confettiVFX.SetActive(false);

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

        GameManager.Instance.isGameActive = true;

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
        // int score = 0;
        // scoreGameOverText.text = score.ToString();

        // TODO: change with the right gameflow
        // Unlock level selection
        GameManager.Instance.UnlockLevel();

        gameOverWinUI.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        // gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.gameObject.SetActive(true);
        // gameOverWinUI.gameObject.SetActive(true);

        gameOverWinUI.transform.DOScale(1, 0.4f).SetEase(Ease.OutExpo).SetDelay(0.3f);
        gameOverWinUI.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        yield return new WaitForSeconds(1f);
        confettiVFX.SetActive(true);
        OpenNewRecipePanel();

        // yield return new WaitForSeconds(0.5f);
        // while (score < currentScore)
        // {
        //     score++;
        //     scoreGameOverText.text = score.ToString();
        //     yield return new WaitForSeconds(animSpeed);
        // }
    }

    public void OpenNewRecipePanel()
    {
        int currentLevel = GameManager.Instance.currentLevel;
        int recipeIndex = currentLevel - 1;
        float recipeInitialX = recipeVisual.transform.localPosition.x;

        recipeText.text = GameManager.Instance.recipeLists[recipeIndex].recipeName;
        recipeVisual.sprite = GameManager.Instance.recipeLists[recipeIndex].recipeSprite;
        recipeVisual.SetNativeSize();

        recipeUnlockUI.transform.localScale = Vector3.zero;
        recipeUnlockUI.alpha = 0;

        // Create a sequence for chaining animations
        Sequence sequence = DOTween.Sequence();

        // First, close the gameOverWinUI panel
        sequence.AppendCallback(() =>
            {
                // gameOverWinUI.gameObject.SetActive(false);

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
            });

        // Play the sequence
        sequence.Play();
    }


    public void LoadToLevelSelection()
    {
        GameManager.Instance.gameStates = GameStates.LevelSelection;
        GameManager.Instance.isGameActive = false;
        DOTween.KillAll();

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }
}
