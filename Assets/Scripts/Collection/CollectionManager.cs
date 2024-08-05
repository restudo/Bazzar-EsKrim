using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BazarEsKrim
{
    public class CollectionManager : MonoBehaviour
    {
        public int unlockedLevel;
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

        private void Awake()
        {
            simpleScrollSnap.gameObject.SetActive(false);

            collectionFrames = new CollectionFrame[collectionDetailButtons.Length];
            // add listener to collection button
            for (int i = 0; i < collectionDetailButtons.Length; i++)
            {
                int index = i;
                collectionDetailButtons[i].onClick.AddListener(() => OnCollectionDetailClicked(index));

                collectionFrames[i] = collectionDetailButtons[i].GetComponent<CollectionFrame>();
            }

            collectionPanels = new CollectionPanel[collectionPanelContainer.transform.childCount];
            for (int i = 0; i < collectionPanels.Length; i++)
            {
                collectionPanels[i] = collectionPanelContainer.transform.GetChild(i).GetComponent<CollectionPanel>();

                collectionPanels[i].SetRecipeList(recipeLists[i]);

                // ensure the frames qty are the same as panels 
                collectionFrames[i].SetRecipeList(recipeLists[i]);
            }

            // TODO: change to unlocked level method
            for (int i = 0; i < unlockedLevel; i++)
            {
                collectionPanels[i].SetCollectionIngredient();

                // ensure the frames qty are the same as panels 
                collectionFrames[i].SetCollectionIngredient();
            }
        }

        private void OnCollectionDetailClicked(int index)
        {
            if (GameManager.Instance.gameStates == GameStates.Collection)
            {
                GameManager.Instance.gameStates = GameStates.CollectionPanel;

                simpleScrollSnap.gameObject.SetActive(true);

                simpleScrollSnap.GoToPanel(index);
            }
        }

        public void CloseCollectionPanel()
        {
            simpleScrollSnap.gameObject.SetActive(false);

            GameManager.Instance.gameStates = GameStates.Collection;
        }
    }
}
