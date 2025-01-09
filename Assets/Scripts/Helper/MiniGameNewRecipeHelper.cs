using UnityEngine;
using UnityEngine.EventSystems;

namespace BazarEsKrim
{
    public class MiniGameNewRecipeHelper : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private MiniGameController miniGameController;    

        public void SetTouchOn()
        {
            miniGameController.SetTouchOn();
        }   

        public void OpenNewRecipePanel()
        {
            miniGameController.OpenNewRecipePanel();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            miniGameController.RecipeRechargeAnim();
        }
    }
}
