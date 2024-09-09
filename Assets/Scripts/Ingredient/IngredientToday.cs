using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BazarEsKrim
{
    public class IngredientToday : MonoBehaviour
    {
        [SerializeField] private Image mainVisual;
        [SerializeField] private Image detailVisual;
        [SerializeField] private TextMeshProUGUI recipeName;

        private void Awake()
        {
            int currentLevel = GameManager.Instance.currentLevel;
            int recipeIndex = currentLevel - 1;
            SO_RecipeList recipeList = GameManager.Instance.recipeLists[recipeIndex];

            mainVisual.sprite = recipeList.recipeSprite;
            detailVisual.sprite = recipeList.recipeDetailSprite;
            recipeName.text = recipeList.recipeName;

            mainVisual.SetNativeSize();
            detailVisual.SetNativeSize();
        }
    }
}
