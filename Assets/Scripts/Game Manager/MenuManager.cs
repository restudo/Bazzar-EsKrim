using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuObj;

    [Header("Collection Menu")]
    [SerializeField] private GameObject collectionObj;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionObj;
    [SerializeField] private GameObject levelButtonContainer;

    private Button[] buttons;

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
    }

    private void Start()
    {
        // Initialize the buttons array with the child buttons of buttonContainer
        buttons = new Button[levelButtonContainer.transform.childCount];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = levelButtonContainer.transform.GetChild(i).GetComponent<Button>();
            int index = i;
            buttons[i].onClick.AddListener(() => OnLevelButtonClicked(index));
        }
    }

    private void OnLevelButtonClicked(int index)
    {
        if (GameManager.Instance.gameStates == GameStates.LevelSelection)
        {
            // Load the level based on the button clicked
            LoadToLevel(index + 1);
        }
    }

    private void SetActiveMenu(bool mainMenu, bool collection, bool levelSelection)
    {
        mainMenuObj.SetActive(mainMenu);
        collectionObj.SetActive(collection);
        levelSelectionObj.SetActive(levelSelection);
    }

    public void LoadToLevel(int levelSelected)
    {
        // Set game state and current level
        GameManager.Instance.currentLevel = levelSelected;
        GameManager.Instance.UnlockIngredientLevel();
        GameManager.Instance.isGameActive = true;

        GameManager.Instance.gameStates = GameStates.MainGame;

        // Load the selected level
        // SceneController.Instance.LoadScene((Scenes)levelSelected - 1);
        SceneController.Instance.FadeAndLoadScene(Scenes.Level);
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
        if (GameManager.Instance.gameStates != GameStates.MainMenu)
        {
            LoadToMainMenu();
        }
        else
        {
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
