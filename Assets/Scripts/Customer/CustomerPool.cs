using BazarEsKrim;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerPool : MonoBehaviour
{
    public ObjectPool<CustomerController> customerPool;
    [SerializeField] private CustomerController customerPrefabs;
    [SerializeField] private Transform parent;
    // private LevelManager levelManager;

    private void Awake()
    {
        customerPool = new ObjectPool<CustomerController>(CreateCustomer, OnGetCustomer, OnReleaseCustomer, OnDestroyCustomer, false, 3, 5);
    }

    private CustomerController CreateCustomer()
    {
        CustomerController customer = Instantiate(customerPrefabs, parent);

        return customer;
    }

    private void OnGetCustomer(CustomerController customer)
    {
        customer.gameObject.SetActive(true);
    }

    private void OnReleaseCustomer(CustomerController customer)
    {
        customer.gameObject.SetActive(false);
    }

    private void OnDestroyCustomer(CustomerController customer)
    {
        Destroy(customer.gameObject);
    }

    // public GameObject GetCustomer()
    // {
    //     return customerPool.Get();
    // }

    // public void ReleaseCustomer(GameObject customer)
    // {
    //     customerPool.Release(customer);
    // }
}
