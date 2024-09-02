using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class LevelButton : MonoBehaviour
    {
        [Space(10)]
        [SerializeField] private Image originSprite;
        [SerializeField] private Image darkSprite;
        [SerializeField] private GameObject glowSprite;

        [Space(10)]
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private GameObject lvTag;
        [SerializeField] private Button startButton;

        private int lvIndex;
        private bool isUnlocked;

        private void LoadToLevel()
        {
            EventHandler.CallLoadToLevelEvent(lvIndex);
        }

        public void Init(int index, bool unlock)
        {
            isUnlocked = unlock;
            lvIndex = index;

            darkSprite.sprite = originSprite.sprite;
            darkSprite.SetNativeSize();
            darkSprite.transform.position = originSprite.transform.position;

            startButton.onClick.AddListener(LoadToLevel);

            glowSprite.SetActive(false);
            glowSprite.SetActive(false);
            lvTag.SetActive(false);
            startButton.gameObject.SetActive(false);

            if (isUnlocked)
            {
                darkSprite.gameObject.SetActive(false);
                lockedIcon.SetActive(false);
            }
            else
            {
                darkSprite.gameObject.SetActive(true);
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
