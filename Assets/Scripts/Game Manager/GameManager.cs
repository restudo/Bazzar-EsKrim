using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameStates gameStates { get; set; }
    // public int randomMaxOrder = 2;
    public SO_LevelDataList[] levelDataLists;
    public int currentLevel;
    public bool isGameActive;
    [HideInInspector] public bool isHolding;

    private int unlockedLevel;

    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // TODO: change to int variable
    private void Start()
    {
        PlayerPrefs.DeleteKey("UnlockedLevel");

        isGameActive = false;
        PlayerPrefs.SetInt("UnlockedLevel", 4);

        foreach (var level in levelDataLists)
        {            
            for (int i = 0; i < level.mainGameLevelData.sO_RecipeList.Length; i++)
            {
                level.mainGameLevelData.sO_RecipeList[i].Set();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UnlockLevel();
        }
    }

    public int LoadUnlockedLevel()
    {
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return unlockedLevel;
    }

    public void UnlockLevel()
    {
        if (currentLevel >= LoadUnlockedLevel())
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
            PlayerPrefs.Save();
        }
    }
}
