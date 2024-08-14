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

        collectionManager = collectionObj.GetComponent<CollectionManager>();
    }

    private void Start()
    {
        int unlockedLevel = GameManager.Instance.LoadUnlockedLevel();

        // Initialize the buttons array with the child buttons of buttonContainer
        buttons = new Button[levelButtonContainer.transform.childCount];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = levelButtonContainer.transform.GetChild(i).GetComponent<Button>();
            int index = i;

            // hide locked icon
            if(index + 1 <= unlockedLevel)
            {
                buttons[i].transform.GetChild(buttons[i].transform.childCount - 1).gameObject.SetActive(false);
            }
            else
            {
                buttons[i].transform.GetChild(buttons[i].transform.childCount - 1).gameObject.SetActive(true);
            }

            buttons[i].onClick.AddListener(() => OnLevelButtonClicked(index, index + 1 <= unlockedLevel));
        }
    }

    private void OnLevelButtonClicked(int index, bool isUnlocked)
    {
        if (GameManager.Instance.gameStates == GameStates.LevelSelection)
        {
            if (isUnlocked)
            {
                // Load the level based on the button clicked
                LoadToLevel(index + 1);
            }
            else
            {
                // TODO: add something if level still locked
                Debug.Log("Level " + (index + 1) + " still locked");
            }
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
        // GameManager.Instance.UnlockIngredientLevel();
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
