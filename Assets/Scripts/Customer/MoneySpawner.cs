using UnityEngine;
using UnityEngine.Pool;

public class MoneySpawner : MonoBehaviour
{
    public ObjectPool<Money> moneyPool;
    [SerializeField] private Money moneyObj;

    private Vector3 _customerPosition;

    private void OnEnable()
    {
        EventHandler.SetMoneyPosToCustomerPos += SetToCustomerPosition;
    }

    private void OnDisable()
    {
        EventHandler.SetMoneyPosToCustomerPos -= SetToCustomerPosition;
    }

    private void Start()
    {
        moneyPool = new ObjectPool<Money>(CreateMoney, OnTakeMoneyFromPool, OnReturnMoneyToPool, OnDestroyMoney, true, 3, 5);
    }

    private Money CreateMoney()
    {
        // spawn new instance of the money
        Money money = Instantiate(moneyObj, transform.position, Quaternion.identity, transform);

        // assign the money's pool
        money.SetPool(moneyPool);

        return money;
    }

    private void OnTakeMoneyFromPool(Money money)
    {
        // set the transform
        money.transform.position = _customerPosition;
        // money.transform.eulerAngles = transform.eulerAngles;

        // activate
        money.gameObject.SetActive(true);
    }

    private void OnReturnMoneyToPool(Money money)
    {
        // deactivate
        money.gameObject.SetActive(false);

        money.transform.position = transform.position;
    }

    private void OnDestroyMoney(Money money)
    {
        Destroy(money.gameObject);
    }

    private void SetToCustomerPosition(Vector3 customerPosition)
    {
        _customerPosition = customerPosition;
    }
}