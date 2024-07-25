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
        public SO_LevelDataList[] sO_LevelDataList;
        // public SO_IngredientList sO_IngredientList;
        //TODO: add SO_Collection for main image thumbnail

        [SerializeField] private GameObject ingredientPrefab;
        [SerializeField] private GameObject iceCreamHolder;
        [SerializeField] private GameObject ingredientContainer;
        // [SerializeField] private Image mainImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private CollectionIngredients[] collectionIngredients;

        private GameObject lastIngredient = null;

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

            if (currentLevelData.sO_RecipeList == null)
            {
                return;
            }

            int recipeLengthInCurrentCollection = currentLevelData.sO_RecipeList.Length;

            for (int i = 0; i < recipeLengthInCurrentCollection; i++)
            {
                // mainImage.sprite = currentLevelData.sO_RecipeList[i].recipeSprite; // main image
                titleText.text = currentLevelData.sO_RecipeList[i].recipeName;

                int ingredientLength = currentLevelData.sO_RecipeList[i].ingredientsCodes.Length;

                for (int j = 0; j < ingredientLength; j++)
                {
                    ingredientContainer.transform.GetChild(j).gameObject.SetActive(true);

                    int ingredientCodeIndex = (int)currentLevelData.sO_RecipeList[i].ingredientsCodes[j];

                    IngredientDetails ingredientDetails = InventoryManager.Instance.GetIngredientDetails(ingredientCodeIndex);

                    collectionIngredients[j].ingredientImage.sprite = ingredientDetails.collectionIngredientSprite;
                    collectionIngredients[j].ingredientName.text = ingredientDetails.ingredientName;

                    if (ingredientDetails.ingredientType == IngredientType.Topping)
                    {
                        collectionIngredients[j].ingredientImage.GetComponent<RectTransform>().localScale = Vector3.one;
                    }

                    // // Create the new ingredient GameObject with an Image component
                    // GameObject ingredient = new GameObject("ingredient", typeof(RectTransform), typeof(Image), typeof(ContentSizeFitter));

                    // // Set the parent of the ingredient to be icecreamholder
                    // ingredient.transform.SetParent(iceCreamHolder.transform, false);

                    // ContentSizeFitter ingredientSizeFitter = ingredient.GetComponent<ContentSizeFitter>();
                    // ingredientSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    // ingredientSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    // ingredient.GetComponent<Image>().sprite = ingredientDetails.collectionIngredientSprite;

                    GameObject prefab = Instantiate(ingredientPrefab, iceCreamHolder.transform);

                    if (lastIngredient != null)
                    {
                        prefab.transform.position = lastIngredient.transform.GetChild(1).transform.position;
                    }

                    prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = ingredientDetails.dressIngredientSprite;

                    Transform nextPosTransform = prefab.transform.GetChild(prefab.transform.childCount - 1);
                    nextPosTransform.localPosition = new Vector3(nextPosTransform.localPosition.x, ingredientDetails.nextIngredientPosY, nextPosTransform.localPosition.z);

                    // ingredientGameobject.IngredientCode = (int)ingredientDetails.ingredientCode;
                    // ingredientGameobject.IngredientType = ingredientDetails.ingredientType;

                    // prefab.transform.GetChild(0).AddComponent<Image>().sprite = ingredientDetails.dressIngredientSprite;
                    // prefab.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

                    lastIngredient = prefab;

                    Debug.Log("Last Ingredient : " + j + " " + lastIngredient.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);

                    // // Convert the Transform component to RectTransform
                    // RectTransform rectTransform = prefab.GetComponent<RectTransform>();
                    // if (rectTransform == null)
                    // {
                    //     rectTransform = prefab.AddComponent<RectTransform>();
                    // }

                    // rectTransform.localScale = Vector3.one;
                }
            }
        }
    }
}
