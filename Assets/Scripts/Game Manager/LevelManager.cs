using System.Collections;
using BazarEsKrim;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    // Duration for scaling down and up
    [SerializeField] private float buttonScaleAnimDuration = 0.1f;
    [SerializeField] private Button mainGameNextButton;
    [SerializeField] private Button mainGameRestartButton;
    [SerializeField] private Button mainGamePauseButton;
    [SerializeField] private Button[] mainGameResumeButton;
    [SerializeField] private Button[] mainGameHomeButton;
    [Space(5)]
    [SerializeField] private Button miniGameOverNextButton;
    [SerializeField] private Button miniGameHomeButton;

    [Header("Audio")]
    [SerializeField] private AudioClip gameplayBgm;
    [SerializeField] private AudioClip buttonSfx;

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

        AudioManager.Instance.PlayMusic(gameplayBgm, 0.4f);
    }

    private void ActiveGame(bool main, bool mini)
    {
        mainGame.SetActive(main);
        miniGame.SetActive(mini);
    }

    private void Pause()
    {
        GameManager.Instance.isGamePaused = true;

        AnimateButton(0.1f);

        // TODO: open pause menu
        mainGameController.OpenPauseMenu();

        EventHandler.CallTogglePauseEvent();
    }

    private void Resume()
    {
        GameManager.Instance.isGamePaused = false;

        AnimateButton(0.1f);

        // TODO: close pause menu
        mainGameController.ClosePauseMenu();

        EventHandler.CallTogglePauseEvent();
    }

    private void ReloadScene()
    {
        DOTween.KillAll();

        AnimateButton(0.1f);

        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneController.Instance.FadeAndLoadScene(Scenes.Level); // add scene transition rolling door
    }

    private void LoadToLevelSelection()
    {
        DOTween.KillAll();

        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }

    private void LoadToMainMenu()
    {
        DOTween.KillAll();

        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.MainMenu;

        SceneController.Instance.FadeAndLoadScene(Scenes.Menu);
    }

    private void LoadToMiniGame()
    {
        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.MiniGame;

        SceneController.Instance.FadeAndSetActiveGameobject(mainGame, miniGame);
    }

    // private void OpenNewRecipePanel()
    // {
    //     miniGameController.OpenNewRecipePanel();
    // }

    public void AddMiniGameOverButtonEvent()
    {
        miniGameOverNextButton.onClick.AddListener(LoadToLevelSelection);
        miniGameHomeButton.onClick.AddListener(LoadToMainMenu);
    }

    // Animate button with a dynamic scale change
    public void AnimateButton(float scaleDecrement)
    {
        // Get the currently clicked button
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;

        AudioManager.Instance.PlaySFX(buttonSfx);

        if (clickedButton != null)
        {
            // Calculate the target scale
            Vector3 targetScale = clickedButton.transform.localScale - new Vector3(scaleDecrement, scaleDecrement, 0);

            // Scale down and then back up
            clickedButton.transform.DOScale(targetScale, buttonScaleAnimDuration)
                .OnComplete(() =>
                {
                    clickedButton.transform.DOScale(clickedButton.transform.localScale + new Vector3(scaleDecrement, scaleDecrement, 0), buttonScaleAnimDuration);
                });
        }
    }
}
