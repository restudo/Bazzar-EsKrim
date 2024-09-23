using System.Collections;
using BazarEsKrim;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("State Gameobject")]
    [SerializeField] private GameObject mainGame;
    [SerializeField] private GameObject miniGame;

    [Header("Game Controller")]
    [SerializeField] private MainGameController mainGameController;
    [SerializeField] private MiniGameController miniGameController;

    [Space(10)]
    [Header("Buttons")]
    [SerializeField] private Button mainGameNextButton;
    [SerializeField] private Button mainGameRestartButton;
    [SerializeField] private Button mainGamePauseButton;
    [SerializeField] private Button[] mainGameResumeButton;
    [SerializeField] private Button[] mainGameHomeButton;
    [Space(5)]
    [SerializeField] private Button miniGameNextButton;
    [SerializeField] private Button miniGameResumeButton;
    [SerializeField] private Button miniGameOverNextButton;
    [SerializeField] private Button miniGameHomeButton;


    private void Awake()
    {
        foreach (var level in GameManager.Instance.levelDataLists)
        {
            level.mainGameLevelData.Set();
        }

        mainGameNextButton.onClick.AddListener(LoadToMiniGame);
        mainGameRestartButton.onClick.AddListener(ReloadScene);
        mainGamePauseButton.onClick.AddListener(Pause);
        foreach (Button button in mainGameHomeButton)
        {
            button.onClick.AddListener(LoadToMainMenu);
        }
        foreach (Button button in mainGameResumeButton)
        {
            button.onClick.AddListener(Resume);
        }

        // miniGameNextButton.onClick.AddListener(OpenNewRecipePanel);

        switch (GameManager.Instance.gameStates)
        {
            case GameStates.MainGame:
                ActiveGame(main: true, mini: false);
                break;
            case GameStates.MiniGame:
                ActiveGame(main: false, mini: true);
                break;
            default:
                Debug.LogError("Current Game State is " + GameManager.Instance.gameStates.ToString());

                if (mainGame.activeSelf)
                {
                    GameManager.Instance.gameStates = GameStates.MainGame;
                }
                else if (miniGame.activeSelf)
                {
                    GameManager.Instance.gameStates = GameStates.MiniGame;
                }

                Debug.LogWarning("Current Game State has changed to = " + GameManager.Instance.gameStates.ToString());
                break;
        }
    }

    private void ActiveGame(bool main, bool mini)
    {
        mainGame.SetActive(main);
        miniGame.SetActive(mini);
    }

    private void Pause()
    {
        GameManager.Instance.isGamePaused = true;

        // TODO: open pause menu
        mainGameController.OpenPauseMenu();

        EventHandler.CallTogglePauseEvent();
    }

    private void Resume()
    {
        GameManager.Instance.isGamePaused = false;

        // TODO: close pause menu
        mainGameController.ClosePauseMenu();

        EventHandler.CallTogglePauseEvent();
    }

    private void ReloadScene()
    {
        DOTween.KillAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadToLevelSelection()
    {
        DOTween.KillAll();

        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }

    private void LoadToMainMenu()
    {
        DOTween.KillAll();

        GameManager.Instance.gameStates = GameStates.MainMenu;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }

    private void LoadToMiniGame()
    {
        GameManager.Instance.gameStates = GameStates.MiniGame;

        mainGame.SetActive(false);
        miniGame.SetActive(true);
    }

    private void OpenNewRecipePanel()
    {
        miniGameController.OpenNewRecipePanel();
    }

    public void AddMiniGameOverButtonEvent()
    {
        miniGameOverNextButton.onClick.AddListener(LoadToLevelSelection);
        miniGameHomeButton.onClick.AddListener(LoadToMainMenu);
    }
}
