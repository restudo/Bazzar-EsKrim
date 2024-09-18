using BazarEsKrim;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
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

    private void Awake()
    {
        // switch (GameManager.Instance.gameStates)
        // {
        //     case GameStates.MainMenu:
        //         LoadToMainMenu();
        //         break;
        //     case GameStates.Collection:
        //         LoadToCollection();
        //         break;
        //     case GameStates.LevelSelection:
        //         LoadToLevelSelection();
        //         break;
        //     default:
        //         Debug.LogError("Current Game State = " + GameManager.Instance.gameStates.ToString());
        //         break;
        // }
        LoadToMainMenu();

        collectionManager = collectionObj.GetComponent<CollectionManager>();

        foreach (Button button in backButtons)
        {
            button.onClick.AddListener(Back);
        }
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

    public void LoadToMainMenu()
    {
        GameManager.Instance.gameStates = GameStates.MainMenu;

        SetCameraToZero();

        SetActiveMenu(mainMenu: true, collection: false, levelSelection: false);

        // stop anim
        skeletonGraphicUI.AnimationState.SetEmptyAnimation(0, 0);

        // play anim
        skeletonGraphicUI.AnimationState.SetAnimation(0, coverIn, false);
        skeletonGraphicUI.AnimationState.AddAnimation(0, coverIdle, true, 0);
    }

    public void LoadToLevelSelection()
    {
        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SetCameraToZero();

        SetActiveMenu(mainMenu: false, collection: false, levelSelection: true);
    }

    public void LoadToCollection()
    {
        GameManager.Instance.gameStates = GameStates.Collection;

        SetCameraToZero();

        SetActiveMenu(mainMenu: false, collection: true, levelSelection: false);
    }

    public void Back()
    {
        if (GameManager.Instance.gameStates == GameStates.CollectionPanel)
        {
            collectionManager.CloseCollectionPanel();
        }
        else if (GameManager.Instance.gameStates != GameStates.MainMenu)
        {
            SetCameraToZero();
            LoadToMainMenu();
        }
        else
        {
            Debug.Log("QUIT");

            Application.Quit();
        }
    }

    public void LoadToMiniGame()
    {
        GameManager.Instance.isGameActive = true;

        GameManager.Instance.gameStates = GameStates.MiniGame;

        SceneController.Instance.FadeAndLoadScene(Scenes.Level);
    }
}
