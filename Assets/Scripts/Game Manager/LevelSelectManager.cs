using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class LevelSelectManager : MonoBehaviour
    {
        [SerializeField] private GameObject levelButtonContainer;

        private Button[] buttons;
        private LevelButton[] levelbuttons;
        private int lastSelectedButtonIndex = -1;

        private void OnEnable()
        {
            EventHandler.LoadToLevel += LoadToLevel;
        }

        private void OnDisable()
        {
            EventHandler.LoadToLevel -= LoadToLevel;
        }

        private void Start()
        {
            // Initialize the buttons array with the child buttons of buttonContainer
            buttons = new Button[levelButtonContainer.transform.childCount];
            levelbuttons = new LevelButton[levelButtonContainer.transform.childCount];

            int unlockedLevel = GameManager.Instance.LoadUnlockedLevel();

            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;

                buttons[index] = levelButtonContainer.transform.GetChild(index).GetComponent<Button>();
                levelbuttons[index] = levelButtonContainer.transform.GetChild(index).GetComponent<LevelButton>();

                // Hide or show the locked icon based on the unlocked level
                buttons[index].transform.GetChild(buttons[index].transform.childCount - 1).gameObject.SetActive(index + 1 > unlockedLevel);
                buttons[index].transform.GetChild(buttons[index].transform.childCount - 2).gameObject.SetActive(index + 1 > unlockedLevel);

                buttons[index].onClick.AddListener(() => OnLevelButtonClicked(index, index + 1 <= unlockedLevel));
                levelbuttons[index].Init(index, index + 1 <= unlockedLevel);
            }

            // init first
            if (unlockedLevel > 0)
            {
                // set camera
                Camera.main.transform.position = new Vector3(buttons[unlockedLevel - 1].transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                levelbuttons[unlockedLevel - 1].Selected();
                lastSelectedButtonIndex = unlockedLevel - 1;
            }
        }

        private void OnLevelButtonClicked(int index, bool isUnlocked)
        {
            if (GameManager.Instance.gameStates == GameStates.LevelSelection)
            {
                if (isUnlocked)
                {
                    if (lastSelectedButtonIndex > -1)
                    {
                        levelbuttons[lastSelectedButtonIndex].Deselect();
                    }
                    levelbuttons[index].Selected();

                    lastSelectedButtonIndex = index;
                }
                else
                {
                    // TODO: add something if level still locked
                    Debug.Log("Level " + (index + 1) + " still locked");
                }
            }
        }

        public void LoadToLevel(int levelSelected)
        {
            // Set game state and current level
            GameManager.Instance.currentLevel = levelSelected + 1;
            // GameManager.Instance.UnlockIngredientLevel();
            GameManager.Instance.isGameActive = true;

            GameManager.Instance.gameStates = GameStates.MainGame;

            // Load the selected level
            // SceneController.Instance.LoadScene((Scenes)levelSelected - 1);
            SceneController.Instance.FadeAndLoadScene(Scenes.Level);
        }
    }
}