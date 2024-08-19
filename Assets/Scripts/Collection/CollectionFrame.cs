using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class CollectionFrame : MonoBehaviour
    {
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private RectTransform iceCreamHolder;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject frameBlocker;
        [SerializeField] private GameObject tagBlocker;
        [SerializeField] private GameObject lockedIcon;

        private SO_RecipeList recipeList;
        private GameObject lastIngredient = null;
        private bool canCheckHolderYPos;

        private void Awake()
        {
            titleText.text = "???";

            frameBlocker.SetActive(true);
            tagBlocker.SetActive(true);
            lockedIcon.SetActive(true);

            canCheckHolderYPos = true;
        }

        public void SetCollectionIngredient(SO_RecipeList recipe, bool isUnlocked)
        {
            recipeList = recipe;
            if (recipeList == null)
            {
                return;
            }

            if (isUnlocked)
            {
                titleText.text = recipeList.recipeName;

                frameBlocker.SetActive(false);
                tagBlocker.SetActive(false);
                lockedIcon.SetActive(false);
            }

            int ingredientLength = recipeList.ingredientsCodes.Length;

            for (int i = 0; i < ingredientLength; i++)
            {
                int ingredientCodeIndex = (int)recipeList.ingredientsCodes[i];

                IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCodeIndex);

                // change the ingredient holder pos in collection main image panel
                if (canCheckHolderYPos)
                {
                    foreach (IngredientHolderPosInfos holderPos in collectionManager.holderFrameYPos.ingredientHolderPosInfos)
                    {
                        if (holderPos.coneTypes == ingredientDetails.coneTypes)
                        {
                            iceCreamHolder.anchoredPosition = new Vector3(iceCreamHolder.anchoredPosition.x, holderPos.holderYPosByHeight[ingredientLength - 1]);

                            canCheckHolderYPos = false;
                        }
                    }
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

        // public void SetRecipeList(SO_RecipeList recipe)
        // {
        //     recipeList = recipe;
        // }
    }
}
