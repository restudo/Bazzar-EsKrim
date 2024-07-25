using System;
using TMPro;
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
        public SO_LevelDataList[] sO_LevelDataList;
        // public SO_IngredientList sO_IngredientList;
        //TODO: add SO_Collection for main image thumbnail

        [SerializeField] private GameObject ingredientContainer;
        [SerializeField] private Image mainImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CollectionIngredients[] collectionIngredients;

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
        }

        public void SetCollectionIngredient(int currentCollection)
        {
            LevelDataMainGame currentLevelData = sO_LevelDataList[currentCollection].mainGameLevelData;

            if(currentLevelData.sO_RecipeList == null)
            {
                return;
            }

            int recipeLengthInCurrentCollection = currentLevelData.sO_RecipeList.Length;

            for (int i = 0; i < recipeLengthInCurrentCollection; i++)
            {
                mainImage.sprite = currentLevelData.sO_RecipeList[i].recipeSprite; // main image
                titleText.text = currentLevelData.sO_RecipeList[i].recipeName;

                int ingredientLength = currentLevelData.sO_RecipeList[i].ingredientsCodes.Length;

                for (int j = 0; j < ingredientLength; j++)
                {
                    ingredientContainer.transform.GetChild(j).gameObject.SetActive(true);

                    int ingredientCodeIndex = (int)currentLevelData.sO_RecipeList[i].ingredientsCodes[j];

                    IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCodeIndex);

                    collectionIngredients[j].ingredientImage.sprite = ingredientDetails.dressIngredientSprite;
                    collectionIngredients[j].ingredientName.text = ingredientDetails.ingredientCode.ToString();
                }
            }
        }
    }
}
