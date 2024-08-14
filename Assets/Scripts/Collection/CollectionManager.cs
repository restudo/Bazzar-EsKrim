using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BazarEsKrim
{
    public class CollectionManager : MonoBehaviour
    {
        public SO_RecipeList[] recipeLists;
        public SO_IngredientHolderPos holderPanelYPos; // 0 is recipe 1, 1 is recipe 2
        public SO_IngredientHolderPos holderFrameYPos;
        public GameObject ingredientPrefab;

        [SerializeField] private SimpleScrollSnap simpleScrollSnap;

        [Space(20)]
        [SerializeField] private Button[] collectionDetailButtons;

        [Space(20)]
        [SerializeField] private GameObject collectionPanelContainer;

        private CollectionPanel[] collectionPanels;
        private CollectionFrame[] collectionFrames;
        private int unlockedLevel;

        private void Awake()
        {
            unlockedLevel = GameManager.Instance.LoadUnlockedLevel();

            collectionFrames = new CollectionFrame[collectionDetailButtons.Length];
            collectionPanels = new CollectionPanel[collectionPanelContainer.transform.childCount];



            // add listener and set ingredient to collection button
            for (int i = 0; i < collectionDetailButtons.Length; i++)
            {
                int index = i;

                collectionFrames[index] = collectionDetailButtons[index].GetComponent<CollectionFrame>();

                // hide locked icon
                if (index + 1 < unlockedLevel)
                {
                    collectionFrames[index].transform.GetChild(collectionFrames[index].transform.childCount - 1).gameObject.SetActive(false);
                }
                else
                {
                    collectionFrames[index].transform.GetChild(collectionFrames[index].transform.childCount - 1).gameObject.SetActive(true);
                }

                collectionDetailButtons[index].onClick.AddListener(() => OnCollectionDetailClicked(index, index + 1 < unlockedLevel));
            }

            for (int i = 0; i < collectionPanels.Length; i++)
            {
                collectionPanels[i] = collectionPanelContainer.transform.GetChild(i).GetComponent<CollectionPanel>();
            }
        }

        private void Start()
        {
            if (collectionFrames.Length == collectionPanels.Length)
            {
                int length = collectionFrames.Length;

                for (int i = 0; i < length; i++)
                {
                    collectionPanels[i].SetCollectionIngredient(recipeLists[i]);
                    collectionFrames[i].SetCollectionIngredient(recipeLists[i], i + 1 < unlockedLevel); // check is unlcoked as well to show recipe name
                }
            }
            else
            {
                Debug.LogWarning("Collection Frames and Collection Panels Length aren't the same");
            }

            simpleScrollSnap.gameObject.SetActive(false);
        }

        private void OnCollectionDetailClicked(int index, bool isUnlocked)
        {
            if (GameManager.Instance.gameStates == GameStates.Collection)
            {
                if (isUnlocked)
                {
                    GameManager.Instance.gameStates = GameStates.CollectionPanel;

                    simpleScrollSnap.gameObject.SetActive(true);

                    simpleScrollSnap.GoToPanel(index);
                }
                else
                {
                    // TODO: add something if level still locked
                    Debug.Log("Collection " + (index + 1) + " still locked");
                }
            }
        }

        public void CloseCollectionPanel()
        {
            simpleScrollSnap.gameObject.SetActive(false);

            GameManager.Instance.gameStates = GameStates.Collection;
        }
    }
}
