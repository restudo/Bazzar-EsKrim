using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class CollectionManager : MonoBehaviour
    {
        public int unlockedLevel;

        [SerializeField] private Button[] collectionDetailButtons;

        [Space(20)]
        [SerializeField] private CollectionPanel[] collectionPanels;

        private void Start()
        {
            foreach (CollectionPanel collectionPanel in collectionPanels)
            {
                collectionPanel.gameObject.SetActive(false);
            }

            // add listener to collection button
            for (int i = 0; i < collectionDetailButtons.Length; i++)
            {
                int index = i;
                collectionDetailButtons[i].onClick.AddListener(() => OnCollectionDetailClicked(index));
            }

            // TODO: change to unlocked level method
            for (int i = 0; i < unlockedLevel; i++)
            {
                collectionPanels[i].SetCollectionIngredient(i);
            }
        }

        private void OnCollectionDetailClicked(int index)
        {
            if (GameManager.Instance.gameStates == GameStates.Collection)
            {
                GameManager.Instance.gameStates = GameStates.CollectionPanel;

                // TODO: avtivate the correct panel of ingredient detail using simple scroll snap
                collectionPanels[index].gameObject.SetActive(true);
                Debug.Log("Collection Number " + index);
            }
        }

        public void CloseCollectionPanel()
        {
            foreach (CollectionPanel collectionPanel in collectionPanels)
            {
                if (collectionPanel.gameObject.activeSelf)
                {
                    collectionPanel.gameObject.SetActive(false);

                    GameManager.Instance.gameStates = GameStates.Collection;

                    return;
                }
            }
        }
    }
}
