using BazarEsKrim;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // Duration for scaling down and up
    [SerializeField] private float buttonScaleAnimDuration = 0.1f;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuObj;
    [SerializeField] private SkeletonGraphic skeletonGraphicUI;
    private const string coverIn = "in";
    private const string coverIdle = "idle";

    [Header("Collection Menu")]
    [SerializeField] private GameObject collectionObj;
    private CollectionManager collectionManager;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionObj;

    [Header("Back Buttons")]
    [SerializeField] private Button[] backButtons;

    [Header("Audio")]
    [SerializeField] private AudioClip tittleBgm;
    [SerializeField] private AudioClip levelSelectBgm;
    [SerializeField] private AudioClip buttonSfx;
    [SerializeField] private AudioClip startVo;

    private bool isFirstTimeLoad = false;

    private void Awake()
    {
        isFirstTimeLoad = true;

        GameManager.Instance.isGameActive = false;

        switch (GameManager.Instance.gameStates)
        {
            case GameStates.MainMenu:
                LoadToMainMenu(isFirstTimeLoad);
                break;
            case GameStates.Collection:
                LoadToCollection();
                break;
            case GameStates.LevelSelection:
                LoadToLevelSelection();
                break;
            default:
                Debug.LogError("Current Game State = " + GameManager.Instance.gameStates.ToString());
                break;
        }

        collectionManager = collectionObj.GetComponent<CollectionManager>();

        foreach (Button button in backButtons)
        {
            button.onClick.AddListener(Back);
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic(tittleBgm);
        AudioManager.Instance.PlayVO(startVo);
    }

    private void SetActiveMenu(bool mainMenu, bool collection, bool levelSelection)
    {
        mainMenuObj.SetActive(mainMenu);
        collectionObj.SetActive(collection);
        levelSelectionObj.SetActive(levelSelection);
    }

    private void SetCameraToZero()
    {
        Camera.main.transform.position = new Vector3(0, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }

    public void LoadToMainMenu(bool isFirstTimeLoad)
    {
        GameManager.Instance.isGameActive = false;

        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.MainMenu;

        SetActiveMenu(mainMenu: true, collection: false, levelSelection: false);

        if (isFirstTimeLoad)
        {
            // stop anim
            skeletonGraphicUI.AnimationState.SetEmptyAnimation(0, 0);

            // play anim
            skeletonGraphicUI.AnimationState.SetAnimation(0, coverIn, false);
            skeletonGraphicUI.AnimationState.AddAnimation(0, coverIdle, true, 0);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(tittleBgm);
        }
    }

    public void LoadToLevelSelection()
    {
        GameManager.Instance.isGameActive = false;

        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SetCameraToZero();

        SetActiveMenu(mainMenu: false, collection: false, levelSelection: true);

        AudioManager.Instance.PlayMusic(levelSelectBgm, 0.4f);
    }

    public void LoadToCollection()
    {
        GameManager.Instance.isGameActive = false;

        AnimateButton(0.1f);

        GameManager.Instance.gameStates = GameStates.Collection;

        SetActiveMenu(mainMenu: false, collection: true, levelSelection: false);
    }

    public void Back()
    {
        GameManager.Instance.isGameActive = false;

        AnimateButton(0.1f);

        if (GameManager.Instance.gameStates == GameStates.CollectionPanel)
        {
            collectionManager.CloseCollectionPanel();
        }
        else if (GameManager.Instance.gameStates != GameStates.MainMenu)
        {
            LoadToMainMenu(false);
        }
        else
        {
            Debug.Log("QUIT");

            Application.Quit();
        }
    }

    // Animate button with a dynamic scale change
    public void AnimateButton(float scaleDecrement)
    {
        if (isFirstTimeLoad)
        {
            isFirstTimeLoad = false;
            return;
        }

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

    public void LoadToMiniGame()
    {
        GameManager.Instance.isGameActive = true;

        GameManager.Instance.gameStates = GameStates.MiniGame;

        SceneController.Instance.FadeAndLoadScene(Scenes.Level);
    }
}
