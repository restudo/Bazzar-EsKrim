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
        isGameActive = false;

        PlayerPrefs.DeleteKey("UnlockedLevel");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            UnlockLevel();
        }

        // if(Input.GetKeyDown(KeyCode.U))
        // {
        //     randomMaxOrder--;
        // }

        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     PlayerPrefs.SetInt("BaseUnlocked", GetBaseUnlock() + 1);
        //     Debug.Log(GetBaseUnlock());
        // }
        // if (Input.GetKeyDown(KeyCode.O))
        // {
        //     PlayerPrefs.SetInt("FlavorUnlocked", GetFlavorUnlock() + 1);
        //     Debug.Log(GetFlavorUnlock());
        // }
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     PlayerPrefs.SetInt("ToppingUnlocked", GetToppingUnlock() + 1);
        //     Debug.Log(GetToppingUnlock());
        // }
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     PlayerPrefs.SetInt("RecipeUnlocked", GetRecipeUnlock() + 1);
        //     Debug.Log(GetRecipeUnlock());
        // }
    }

    public int GetBaseUnlock()
    {
        return PlayerPrefs.GetInt("BaseUnlocked");
    }

    public int GetFlavorUnlock()
    {
        return PlayerPrefs.GetInt("FlavorUnlocked");
    }

    public int GetToppingUnlock()
    {
        return PlayerPrefs.GetInt("ToppingUnlocked");
    }

    public int GetRecipeUnlock()
    {
        return PlayerPrefs.GetInt("RecipeUnlocked", 0);
    }

    public void UnlockIngredientLevel()
    {
        switch (currentLevel)
        {
            case 1:
                PlayerPrefs.SetInt("BaseUnlocked", 2);
                PlayerPrefs.SetInt("FlavorUnlocked", 3);
                PlayerPrefs.SetInt("ToppingUnlocked", 3);
                break;
            case 2:
                PlayerPrefs.SetInt("BaseUnlocked", 4);
                PlayerPrefs.SetInt("FlavorUnlocked", 6);
                PlayerPrefs.SetInt("ToppingUnlocked", 6);

                // PlayerPrefs.SetInt("RecipeUnlocked", GetRecipeUnlock() + 1);
                break;
            case 3:
                PlayerPrefs.SetInt("BaseUnlocked", 6);
                PlayerPrefs.SetInt("FlavorUnlocked", 10);
                PlayerPrefs.SetInt("ToppingUnlocked", 10);

                // PlayerPrefs.SetInt("RecipeUnlocked", GetRecipeUnlock() + 1);
                break;
            default:
                break;
        }
    }

    public int LoadUnlockedLevel()
    {
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        return unlockedLevel;
    }

    public void UnlockLevel()
    {
        if(currentLevel >= LoadUnlockedLevel())
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
            PlayerPrefs.Save();
        }
    }
}
