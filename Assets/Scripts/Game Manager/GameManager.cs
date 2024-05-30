using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public bool isGameActive;
    [HideInInspector] public bool isHolding;

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

        PlayerPrefs.SetInt("BaseUnlocked", 2);
        PlayerPrefs.SetInt("FlavorUnlocked", 5);
        PlayerPrefs.SetInt("ToppingUnlocked", 7);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            PlayerPrefs.SetInt("BaseUnlocked", GetBaseUnlock() + 1);
            Debug.Log(GetBaseUnlock());
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerPrefs.SetInt("FlavorUnlocked", GetFlavorUnlock() + 1);
            Debug.Log(GetFlavorUnlock());
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.SetInt("ToppingUnlocked", GetToppingUnlock() + 1);
            Debug.Log(GetToppingUnlock());
        }
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
}
