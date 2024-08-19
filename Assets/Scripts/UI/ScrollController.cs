using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private float scrollStep = 1f; // Amount to scroll per button press
    [SerializeField] private float scrollDuration = 0.167f; // Duration of the smooth scroll
    private int maxContentForButtonShow; // Duration of the smooth scroll
    private ScrollRect scrollRect;
    private UiInventoryPage uiInventoryPage;

    public bool isScrollActive;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            switch (transform.tag)
            {
                case nameof(IngredientType.Base):
                    // maxContentForButtonShow = GameManager.Instance.GetBaseUnlock();
                    break;
                case nameof(IngredientType.Flavor):
                    // maxContentForButtonShow = GameManager.Instance.GetFlavorUnlock();
                    break;
                case nameof(IngredientType.Topping):
                    // maxContentForButtonShow = GameManager.Instance.GetToppingUnlock();
                    break;
                default:
                    Debug.LogWarning("Unknown ingredient type: " + transform.tag);
                    maxContentForButtonShow = 0;
                    break;
            }

            StartCoroutine(yey());
        }
    }

    private IEnumerator yey()
    {
        yield return new WaitForSeconds(0.0001f);

        int content = 0;
            uiInventoryPage = scrollRect.content.GetComponent<UiInventoryPage>();
            // foreach (UiInventorySlot uiInventorySlot in uiInventoryPage.inventorySlot)
            // {
            //     if(uiInventorySlot.ingredientDetails != null)
            //     {
            //         content++;
            //     }
            // }

            Debug.Log(maxContentForButtonShow);
            Debug.Log(content);

            // Sprite sprite = scrollRect.content.GetComponent<UiInventoryPage>().blankSprite;
            // foreach (Transform child in scrollRect.content.transform)
            // {
            //     if(child.GetComponent<UiInventorySlot>().inventorySlotImage.sprite != sprite)
            //     {
            //     }
            // }

            if (content > maxContentForButtonShow)
            {
                leftButton.onClick.AddListener(ScrollLeft);
                rightButton.onClick.AddListener(ScrollRight);
            }
            else
            {
                leftButton.gameObject.SetActive(false);
                rightButton.gameObject.SetActive(false);
            }

            isScrollActive = false;
    }

    public void ScrollLeft()
    {
        if (isScrollActive)
        {
            Scroll(-scrollStep);
        }
    }

    public void ScrollRight()
    {
        if (isScrollActive)
        {
            Scroll(scrollStep);
        }
    }

    private void Scroll(float step)
    {
        float newHorizontalPosition = Mathf.Clamp(scrollRect.horizontalNormalizedPosition + step, 0f, 1f);
        scrollRect.DOKill(); // Stop any existing tweens
        scrollRect.DOHorizontalNormalizedPos(newHorizontalPosition, scrollDuration).SetEase(Ease.InOutQuad);
    }
}
