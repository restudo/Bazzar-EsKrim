using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameController : MonoBehaviour
{
    [Header("Level Manager")]
    [SerializeField] private LevelManager levelManager;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreGameOverText;
    [SerializeField] private float animSpeed;
    private int currentScore;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverWinUI;
    private float currentTime;
    private TimeSpan time;

    [Header("Spawner")]
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
            // levelManager.Win();
            StartCoroutine(Win());

            Debug.Log("Countdown time is up!");
        }
        else
        {
            timerText.text = time.Seconds.ToString();
        }
    }

    private IEnumerator Win()
    {
        int score = 0;
        scoreGameOverText.text = score.ToString();

        gameOverWinUI.transform.parent.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameOverWinUI.transform.parent.localScale = Vector3.zero;
        gameOverWinUI.transform.parent.gameObject.SetActive(true);

        gameOverWinUI.transform.parent.DOScale(1, 0.4f).SetEase(Ease.OutBounce).SetDelay(0.6f);
        gameOverWinUI.transform.parent.GetComponent<Image>().DOColor(new Color32(0, 0, 0, 150), 1.5f).SetDelay(1f);

        yield return new WaitForSeconds(1f);
        gameOverWinUI.transform.GetChild(gameOverWinUI.transform.childCount - 1).gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        while (score < currentScore)
        {
            score++;
            scoreGameOverText.text = score.ToString();
            yield return new WaitForSeconds(animSpeed);
        }

        // TODO: change with the right gameflow
        // Unlock level selection
        GameManager.Instance.UnlockLevel();
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
