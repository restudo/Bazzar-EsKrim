using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    [Serializable]
    public struct CollectionIngredients
    {
        public Image ingredientImage;
        public TextMeshProUGUI ingredientName;
    }

    public class CollectionPanel : MonoBehaviour
    {
        //TODO: add SO_Collection for main image thumbnail
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private RectTransform iceCreamHolder;
        [SerializeField] private GameObject ingredientContainer;
        // [SerializeField] private Image mainImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CollectionIngredients[] collectionIngredients;

        private SO_RecipeList recipeList;
        private GameObject lastIngredient = null;
        private bool canCheckHolderYPos;

        private void Awake()
        {
            // mainImage = null;
            titleText.text = "";

            for (int i = 0; i < collectionIngredients.Length; i++)
            {
                // collectionIngredients[i].ingredientImage = null;
                collectionIngredients[i].ingredientName.text = "";

                ingredientContainer.transform.GetChild(i).gameObject.SetActive(false);
            }

            canCheckHolderYPos = true;
        }

        public void SetCollectionIngredient()
        {
            if (recipeList == null)
            {
                return;
            }

            titleText.text = recipeList.recipeName;

            int ingredientLength = recipeList.ingredientsCodes.Length;

            for (int i = 0; i < ingredientLength; i++)
            {
                ingredientContainer.transform.GetChild(i).gameObject.SetActive(true);

                int ingredientCodeIndex = (int)recipeList.ingredientsCodes[i];

                IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCodeIndex);

                // change the ingredient holder pos in collection main image panel
                if (canCheckHolderYPos)
                {
                    foreach (IngredientHolderPosInfos holderPos in collectionManager.holderPanelYPos.ingredientHolderPosInfos)
                    {
                        if (holderPos.coneTypes == ingredientDetails.coneTypes)
                        {
                            iceCreamHolder.anchoredPosition = new Vector3(iceCreamHolder.anchoredPosition.x, holderPos.holderYPosByHeight[ingredientLength - 1]);

                            canCheckHolderYPos = false;
                        }
                    }
                }

                collectionIngredients[i].ingredientImage.sprite = ingredientDetails.collectionIngredientSprite;
                collectionIngredients[i].ingredientName.text = ingredientDetails.ingredientName;

                if (ingredientDetails.ingredientType == IngredientType.Topping)
                {
                    collectionIngredients[i].ingredientImage.GetComponent<RectTransform>().localScale = Vector3.one;
                }

                // TODO: change to object pooling
                GameObject prefab = Instantiate(collectionManager.ingredientPrefab, iceCreamHolder.transform);

                if (lastIngredient != null)
                {
                    if (ingredientDetails.ingredientType != IngredientType.Topping)
                    {
                        prefab.transform.position = lastIngredient.transform.GetChild(1).transform.position;
                    }
                    else
                    {
                        prefab.transform.position = lastIngredient.transform.position;
                    }
                }

                prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = ingredientDetails.dressIngredientSprite;

                Transform nextPosTransform = prefab.transform.GetChild(prefab.transform.childCount - 1);
                nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

                prefab.transform.GetChild(0).AddComponent<Image>().sprite = ingredientDetails.dressIngredientSprite;
                prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

                lastIngredient = prefab;
            }
        }

        public void SetRecipeList(SO_RecipeList recipe)
        {
            recipeList = recipe;
        }
    }
}
