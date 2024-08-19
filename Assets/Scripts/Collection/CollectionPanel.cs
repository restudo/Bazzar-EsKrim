using System;
using System.Collections.Generic;
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
        public TextMeshProUGUI ingredientCount;
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
            titleText.text = "???";

            for (int i = 0; i < collectionIngredients.Length; i++)
            {
                // collectionIngredients[i].ingredientImage = null;
                collectionIngredients[i].ingredientName.text = "???";

                ingredientContainer.transform.GetChild(i).gameObject.SetActive(false);
            }

            canCheckHolderYPos = true;
        }

        private Dictionary<int, int> GetMergedIngredients(SO_RecipeList recipeList)
        {
            var ingredientCounts = new Dictionary<int, int>();

            foreach (var ingredientCode in recipeList.ingredientsCodes)
            {
                int ingredientCodeIndex = (int)ingredientCode;
                if (ingredientCounts.ContainsKey(ingredientCodeIndex))
                {
                    ingredientCounts[ingredientCodeIndex]++;
                }
                else
                {
                    ingredientCounts[ingredientCodeIndex] = 1;
                }
            }

            return ingredientCounts;
        }

        public void SetCollectionIngredient(SO_RecipeList recipe, bool isUnlocked)
        {
            recipeList = recipe;
            if (recipeList == null || !isUnlocked) return;

            titleText.text = recipeList.recipeName;

            var mergedIngredients = GetMergedIngredients(recipeList);
            int i = 0;

            foreach (var kvp in mergedIngredients)
            {
                int ingredientCodeIndex = kvp.Key;
                int ingredientCount = kvp.Value;

                IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCodeIndex);

                // Update the iceCreamHolder position if needed
                if (canCheckHolderYPos)
                {
                    foreach (IngredientHolderPosInfos holderPos in collectionManager.holderPanelYPos.ingredientHolderPosInfos)
                    {
                        if (holderPos.coneTypes == ingredientDetails.coneTypes)
                        {
                            iceCreamHolder.anchoredPosition = new Vector3(
                                iceCreamHolder.anchoredPosition.x,
                                holderPos.holderYPosByHeight[mergedIngredients.Count - 1]);

                            canCheckHolderYPos = false;
                        }
                    }
                }

                var ingredientUI = collectionIngredients[i];
                ingredientUI.ingredientImage.sprite = ingredientDetails.collectionIngredientSprite;
                ingredientUI.ingredientName.text = ingredientDetails.ingredientName;
                ingredientUI.ingredientCount.text = "x" + ingredientCount;
                ingredientContainer.transform.GetChild(i).gameObject.SetActive(true);

                if (ingredientDetails.ingredientType == IngredientType.Topping)
                {
                    ingredientUI.ingredientImage.GetComponent<RectTransform>().localScale = Vector3.one;
                }

                // TODO: change to object pooling
                GameObject prefab = Instantiate(collectionManager.ingredientPrefab, iceCreamHolder.transform);
                Transform lastChild = prefab.transform.GetChild(prefab.transform.childCount - 1);

                if (lastIngredient != null)
                {
                    prefab.transform.position = ingredientDetails.ingredientType != IngredientType.Topping
                        ? lastIngredient.transform.GetChild(prefab.transform.childCount - 1).transform.position
                        : lastIngredient.transform.position;
                }

                // set the sprite renderer first to determine the size of prefab, then add image component
                prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = ingredientDetails.dressIngredientSprite;
                prefab.transform.GetChild(0).AddComponent<Image>().sprite = ingredientDetails.dressIngredientSprite;
                prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                lastChild.localPosition = new Vector3(lastChild.localPosition.x, ingredientDetails.nextIngredientPosY, lastChild.localPosition.z);

                prefab.SetActive(true);
                lastIngredient = prefab;

                i++;
            }
        }


        // public void SetRecipeList(SO_RecipeList recipe)
        // {
        //     recipeList = recipe;
        // }
    }
}
