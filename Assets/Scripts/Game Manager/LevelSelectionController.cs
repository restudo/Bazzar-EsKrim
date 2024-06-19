using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour
{
    [SerializeField] private GameObject levelButtonContainer;

    private Button[] buttons;

    void Start()
    {
        // Initialize the buttons array with the child buttons of buttonContainer
        buttons = new Button[levelButtonContainer.transform.childCount];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = levelButtonContainer.transform.GetChild(i).GetComponent<Button>();
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    void OnButtonClicked(int index)
    {
        Debug.Log("Button " + index + " clicked.");

        // Load the level based on the button clicked
        LoadToLevel(index + 1);
    }

    public void LoadToLevel(int levelSelected)
    {
        // Set game state and current level
        GameManager.Instance.currentLevel = levelSelected;
        GameManager.Instance.UnlockIngredientLevel();
        GameManager.Instance.isGameActive = true;

        // Load the selected level
        SceneController.Instance.LoadScene((Scenes)levelSelected - 1);
    }
}
