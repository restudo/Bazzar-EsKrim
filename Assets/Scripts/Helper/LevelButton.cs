using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class LevelButton : MonoBehaviour
    {
        [Space(10)]
        [SerializeField] private GameObject glowSprite;
        [SerializeField] private GameObject darkSprite;
        [SerializeField] private GameObject lockedIcon;

        [Space(10)]
        [SerializeField] private GameObject lvTag;
        [SerializeField] private Button startButton;

        private int lvIndex;
        private bool isUnlocked;

        private void LoadToLevel(MenuManager menuManager)
        {
            menuManager.AnimateButton(0.1f);

            EventHandler.CallLoadToLevelEvent(lvIndex);
        }

        public void Init(int index, bool unlock, MenuManager menuManager)
        {
            isUnlocked = unlock;
            lvIndex = index;

            startButton.onClick.AddListener(() => LoadToLevel(menuManager));

            glowSprite.SetActive(false);
            glowSprite.SetActive(false);
            lvTag.SetActive(false);
            startButton.gameObject.SetActive(false);

            if (isUnlocked)
            {
                darkSprite.SetActive(false);
                lockedIcon.SetActive(false);
            }
            else
            {
                darkSprite.SetActive(true);
                lockedIcon.SetActive(true);
            }
        }

        public void Selected()
        {
            glowSprite.SetActive(true);
            lvTag.SetActive(true);
            startButton.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            glowSprite.SetActive(false);
            lvTag.SetActive(false);
            startButton.gameObject.SetActive(false);
        }
    }
}
