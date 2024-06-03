using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image vfxRenderer;
    [SerializeField] private TabGroup tabGroup;

    private Button button;
    private Image image;

    private void Start()
    {
        // tabGroup.Subscribe(this);

        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void SelectTabButton(Sprite enableSprite)
    {
        image.sprite = enableSprite;
        vfxRenderer.enabled = true;
    }

    public void DeselectTabButton(Sprite disableSprite)
    {
        image.sprite = disableSprite;
        vfxRenderer.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance.isGameActive && button.interactable)
        {
            tabGroup.OnTabSelected(this);
        }
    }
}
