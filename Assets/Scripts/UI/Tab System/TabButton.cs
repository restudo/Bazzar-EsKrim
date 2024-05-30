using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TabGroup tabGroup;
    private Button button;
    // [SerializeField] private Image imageBackground;

    private void Start()
    {
        // tabGroup.Subscribe(this);

        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance.isGameActive && button.interactable)
        {
            tabGroup.OnTabSelected(this);
        }
    }
}
