using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class MoneySpawner : MonoBehaviour
{
    public ObjectPool<Money> moneyPool;
    [SerializeField] private Sprite normalCust;
    [SerializeField] private Sprite specialCust;
    [SerializeField] private Money moneyObj;

    [Space(20)]
    [SerializeField] private RectTransform moneySliderUI;
    [SerializeField] private Transform moneyHolder;

    private Vector3 _customerPosition;
    private Sprite moneySprite;

    private void OnEnable()
    {
        EventHandler.SetMoneyPosToCustomerPos += SetToCustomerPosition;
    }

    private void OnDisable()
    {
        EventHandler.SetMoneyPosToCustomerPos -= SetToCustomerPosition;
    }

    private void Awake()
    {
        // transform.position = customerDestinationPoint.transform.position;

        moneyPool = new ObjectPool<Money>(CreateMoney, OnTakeMoneyFromPool, OnReturnMoneyToPool, OnDestroyMoney, true, 3, 5);
    }

    private Money CreateMoney()
    {
        // spawn new instance of the money
        Money money = Instantiate(moneyObj, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, moneyHolder);

        // assign the money's pool
        // Use anchoredPosition to get the UI element's local position relative to its anchors
        money.SetPool(moneyPool);

        return money;
    }

    private void OnTakeMoneyFromPool(Money money)
    {
        // set the transform
        money.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(_customerPosition);
        // money.transform.eulerAngles = transform.eulerAngles;

        money.GetComponent<Image>().sprite = moneySprite;

        // activate
        money.gameObject.SetActive(true);

        money.SetRectTransform(moneySliderUI);
        money.Animate();
    }

    private void OnReturnMoneyToPool(Money money)
    {
        // deactivate
        money.gameObject.SetActive(false);

        money.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position);
    }

    private void OnDestroyMoney(Money money)
    {
        Destroy(money.gameObject);
    }

    private void SetToCustomerPosition(Vector3 customerPosition, bool isRecipeOrder)
    {
        _customerPosition = customerPosition;
        moneySprite = isRecipeOrder ? specialCust : normalCust;
    }
}
