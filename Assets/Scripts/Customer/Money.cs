using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Money : MonoBehaviour
{
    private ObjectPool<Money> _moneyPool;
    private RectTransform _rectTransform;
    private RectTransform moneySlider;

    private const float upPos = 150f; // Example: 150 units for 1.5f distance on screen

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void BackToThePool()
    {
        // Return the UI element to the pool
        _moneyPool.Release(this);
    }

    public void SetPool(ObjectPool<Money> moneyPool)
    {
        // Set the object pool reference
        _moneyPool = moneyPool;
    }

    public void SetRectTransform(RectTransform moneySliderUI)
    {
        moneySlider = moneySliderUI;
    }

    public void Animate()
    {
        // Get screen position of the target UI element
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, moneySlider.position);

        // Convert screen position to local point in the context of another UI element
        RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPos, Camera.main, out localPoint);

        // Create a new DOTween sequence
        Sequence sequence = DOTween.Sequence();

        // First animation: Move up by 150 units in the Y direction
        sequence.Append(_rectTransform.DOLocalMoveY(_rectTransform.localPosition.y + upPos, 0.8f)
            .SetEase(Ease.OutExpo));

        // Second animation: Move to slider UI
        // x axis + 55f to match the middle of ui money slider visual
        sequence.Append(_rectTransform.DOLocalMove(new Vector2(localPoint.x + 55f, localPoint.y), 1f)
            .SetEase(Ease.OutExpo));

        sequence.AppendCallback(BackToThePool);

        // Third step: Apply a "boing" effect using DOScale on the moneySlider element
        sequence.Append(moneySlider.DOScale(Vector3.one * 1.2f, 0.3f)  // Scale up slightly
            .SetEase(Ease.OutElastic))  // Add "boing" effect
            .Append(moneySlider.DOScale(Vector3.one, 0.2f));  // Return to normal size
    }
}
