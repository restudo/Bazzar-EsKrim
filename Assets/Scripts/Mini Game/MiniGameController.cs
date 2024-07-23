using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    [Header("Level Manager")]
    [SerializeField] private LevelManager levelManager;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int currentScore;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float currentTime;
    private TimeSpan time;

    [Header("Timer")]
    [SerializeField] private GameObject[] objectSpawners;

    private void Awake()
    {
        foreach (GameObject spawner in objectSpawners)
        {
            spawner.SetActive(false);
        }

        for (int i = 0; i < GameManager.Instance.levelDataLists[GameManager.Instance.currentLevel - 1].miniGameLevelData.totalObjectSpawner; i++)
        {
            objectSpawners[i].SetActive(true);
        }

        currentTime = GameManager.Instance.levelDataLists[GameManager.Instance.currentLevel - 1].miniGameLevelData.timer;
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

        GameManager.Instance.isGameActive = true;

        scoreText.text = currentScore.ToString();
    }

    private void Update()
    {
        if (GameManager.Instance.isGameActive && GameManager.Instance.gameStates == GameStates.MiniGame)
        {
            ManageTimer();
        }
    }

    private void ManageTimer()
    {
        currentTime -= Time.deltaTime;
        time = TimeSpan.FromSeconds(currentTime);

        if (currentTime <= 0f)
        {
            GameManager.Instance.isGameActive = false;
            levelManager.Win();
            Debug.Log("Countdown time is up!");
        }
        else
        {
            timerText.text = time.Seconds.ToString();
        }
    }

    private void AddMiniGameScore()
    {
        currentScore++;

        scoreText.text = currentScore.ToString();
    }

    public void LoadToLevelSelection()
    {
        DOTween.KillAll();

        GameManager.Instance.isGameActive = false;

        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }
}
