using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class CollectionManager : MonoBehaviour
    {
        public MenuManager menuManager;
        public SO_RecipeList[] recipeLists;
        public SO_IngredientHolderPos holderPanelYPos;
        public SO_IngredientHolderPos holderFrameYPos;
        public GameObject ingredientPrefab;

        [SerializeField] private SimpleScrollSnap simpleScrollSnap;
        [SerializeField] private GameObject scrollScapBlocker;

        [Space(20)]
        [SerializeField] private Button[] collectionDetailButtons;

        [Space(20)]
        [SerializeField] private GameObject collectionPanelContainer;

        private CollectionPanel[] collectionPanels;
        private CollectionFrame[] collectionFrames;
        private int unlockedLevel;

        private void Awake()
        {
            simpleScrollSnap.gameObject.SetActive(true);
            scrollScapBlocker.SetActive(true);

            collectionFrames = new CollectionFrame[collectionDetailButtons.Length];
            collectionPanels = new CollectionPanel[collectionPanelContainer.transform.childCount];
        }

        private void Start()
        {
            unlockedLevel = GameManager.Instance.LoadUnlockedLevel();

            if (collectionFrames.Length == collectionPanels.Length)
            {
                // Cache components and optimize UI
                for (int i = 0; i < collectionFrames.Length; i++)
                {
                    int index = i;

                    collectionFrames[index] = collectionDetailButtons[index].GetComponent<CollectionFrame>();
                    collectionPanels[index] = collectionPanelContainer.transform.GetChild(index).GetComponent<CollectionPanel>();

                    collectionDetailButtons[index].onClick.AddListener(() => OnCollectionDetailClicked(index, index + 1 < unlockedLevel));

                    collectionPanels[index].SetCollectionIngredient(recipeLists[index], index + 1 < unlockedLevel);
                    collectionFrames[index].SetCollectionIngredient(recipeLists[index], index + 1 < unlockedLevel);
                }
            }
            else
            {
                Debug.LogWarning("Collection Frames and Collection Panels Length aren't the same");
            }

            simpleScrollSnap.gameObject.SetActive(false);
            scrollScapBlocker.SetActive(false);
        }

        private void OnCollectionDetailClicked(int index, bool isUnlocked)
        {
            if (GameManager.Instance.gameStates == GameStates.Collection)
            {
                menuManager.AnimateButton(0.1f);

                if (isUnlocked)
                {
                    GameManager.Instance.gameStates = GameStates.CollectionPanel;

                    // Enable scroll snap with async or caching if necessary
                    simpleScrollSnap.gameObject.SetActive(true);
                    simpleScrollSnap.GoToPanel(index);

                    scrollScapBlocker.SetActive(true);
                }
                else
                {
                    Debug.Log("Collection " + (index + 1) + " still locked");
                }
            }
        }

        public void CloseCollectionPanel()
        {
            simpleScrollSnap.gameObject.SetActive(false);
            scrollScapBlocker.SetActive(false);
            GameManager.Instance.gameStates = GameStates.Collection;
        }
    }
}
