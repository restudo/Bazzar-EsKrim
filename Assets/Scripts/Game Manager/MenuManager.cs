using BazarEsKrim;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuObj;

    [Header("Collection Menu")]
    [SerializeField] private GameObject collectionObj;
    private CollectionManager collectionManager;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionObj;

    [Header("Back Buttons")]
    [SerializeField] private Button[] backButtons;

    private void Awake()
    {
        switch (GameManager.Instance.gameStates)
        {
            case GameStates.MainMenu:
                LoadToMainMenu();
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

    private void SetActiveMenu(bool mainMenu, bool collection, bool levelSelection)
    {
        mainMenuObj.SetActive(mainMenu);
        collectionObj.SetActive(collection);
        levelSelectionObj.SetActive(levelSelection);
    }

    public void LoadToMainMenu()
    {
        GameManager.Instance.gameStates = GameStates.MainMenu;

        SetActiveMenu(mainMenu: true, collection: false, levelSelection: false);
    }

    public void LoadToLevelSelection()
    {
        GameManager.Instance.gameStates = GameStates.LevelSelection;

        SetActiveMenu(mainMenu: false, collection: false, levelSelection: true);
    }

    public void LoadToCollection()
    {
        GameManager.Instance.gameStates = GameStates.Collection;

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
