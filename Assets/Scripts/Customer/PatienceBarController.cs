using UnityEngine;
using System.Collections;

public class PatienceBarController : MonoBehaviour
{
    [SerializeField] private GameObject fillObj; // The fill GameObject

    private float decreaseDuration; // Duration over which patience decreases to zero
    private float maxPatience; // The maximum patience value
    private float currentPatience;
    private Vector3 initialFillScale;
    private Vector3 initialFillPosition;

    private Coroutine decreaseCoroutine;
    private CustomerController customerController;

    private void Start()
    {
        customerController = GetComponent<CustomerController>();

        maxPatience = customerController.customerPatience;

        initialFillScale = fillObj.transform.localScale;
        initialFillPosition = fillObj.transform.localPosition;

        decreaseDuration = maxPatience;
        currentPatience = maxPatience;
    }

    public void StartDecreasingPatience()
    {
        // Start the coroutine to decrease patience over time
        decreaseCoroutine = StartCoroutine(DecreasePatienceOverTime());

        Debug.Log("Starting");
    }

    private IEnumerator DecreasePatienceOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < decreaseDuration)
        {
            // Calculate the new patience value
            currentPatience = Mathf.Lerp(maxPatience, 0, elapsedTime / decreaseDuration);

            //if customer has waited for half of his/her patience, make him/her bored.
            if (currentPatience <= maxPatience / 2)
            {
                customerController.UpdateCustomerMood(1); //1 is bored index
            }

            // Update the fill scale
            UpdateFill();

            // Increment elapsed time by the time of the last frame
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the patience is set to zero at the end
        currentPatience = 0;
        UpdateFill();
        StartCoroutine(customerController.Leave());
    }

    private void UpdateFill()
    {
        // Calculate the fill scale based on the current patience
       float fillScaleY = currentPatience / maxPatience;

        // Set the fill GameObject's local scale
        fillObj.transform.localScale = new Vector3(initialFillScale.x, fillScaleY * initialFillScale.y, initialFillScale.z);

        // Adjust the fill position to keep the top anchored
        fillObj.transform.localPosition = new Vector3(initialFillPosition.x, initialFillPosition.y - (1 - fillScaleY) * initialFillScale.y * 0.5f, initialFillPosition.z);
    }

    // Optional: To stop decreasing patience at any point
    public void StopDecreasingPatience()
    {
        if (decreaseCoroutine != null)
        {
            StopCoroutine(decreaseCoroutine);
        }
    }
}
