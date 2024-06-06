using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private float scrollStep = 1f; // Amount to scroll per button press
    [SerializeField] private float scrollDuration = 0.167f; // Duration of the smooth scroll
    private ScrollRect scrollRect;

    public bool isScrollActive;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            leftButton.onClick.AddListener(ScrollLeft);
            rightButton.onClick.AddListener(ScrollRight);

            isScrollActive = false;
        }
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
