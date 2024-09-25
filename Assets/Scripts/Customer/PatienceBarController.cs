using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class PatienceBarController : MonoBehaviour
{
    [SerializeField] private Slider patienceSlider; // slider
    [SerializeField] private float blinkDuration;
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private Color _blinkColor;

    private float patienceDuration; // The maximum patience value
    private float currentPatience;
    private Coroutine decreaseCoroutine;
    private CustomerController customerController;
    private Image fillImage;
    private Color colorOrigin;

    private void Awake()
    {
        customerController = GetComponent<CustomerController>();

        // Get the fill image of the slider
        fillImage = patienceSlider.fillRect.GetComponent<Image>();
        colorOrigin = fillImage.color;
    }

    public void StartDecreasingPatience()
    {
        fillImage.color = colorOrigin;

        patienceDuration = customerController.customerPatience;
        currentPatience = patienceDuration;

        // Initialize the slider
        if (patienceSlider != null)
        {
            patienceSlider.maxValue = patienceDuration;
            patienceSlider.value = patienceDuration;
        }

        // Start the coroutine to decrease patience over time
        decreaseCoroutine = StartCoroutine(DecreasePatienceOverTime());
    }

    private IEnumerator DecreasePatienceOverTime()
    {
        while (currentPatience > 0)
        {
            // Check if the game is active
            if (GameManager.Instance.isGameActive)
            {
                // Decrease patience over time
                currentPatience -= Time.deltaTime;

                // If customer has waited for half of their patience, make them bored
                if (currentPatience <= patienceDuration / 2)
                {
                    customerController.UpdateCustomerMood(1); // 1 is bored index
                }

                // Update the patience slider if it exists
                if (patienceSlider != null)
                {
                    patienceSlider.value = currentPatience;
                }
            }

            // Wait for the next frame before checking again
            yield return null;
        }

        // Handle when patience runs out
        HandlePatienceDepleted();
    }


    private void HandlePatienceDepleted()
    {
        customerController.StartLeaving();
    }

    // Optional: To stop decreasing patience at any point
    public void StopDecreasingPatience()
    {
        if (decreaseCoroutine != null)
        {
            StopCoroutine(decreaseCoroutine);
        }
    }

    public void DecreaseWithValue(float value)
    {
        float newPatience = Mathf.Clamp(currentPatience - value, 0, patienceDuration);

        if (fillImage == null) return;

        // Store the original color
        // Color originalColor = fillImage.color;
        // Color blinkColor = _blinkColor;

        // Blink animation: Change color to red and back to the original color
        Sequence blinkSequence = DOTween.Sequence();
        // Use a loop to blink
        for (int i = 0; i < blinkCount; i++)
        {
            blinkSequence.Append(fillImage.DOColor(_blinkColor, blinkDuration));
            blinkSequence.Append(fillImage.DOColor(colorOrigin, blinkDuration));
        }

        // After blinking, update the slider value
        blinkSequence.Append(patienceSlider.DOValue(newPatience, 0.15f).OnUpdate(() => currentPatience = patienceSlider.value));

        // Check and update customer mood after the slider value update
        blinkSequence.OnComplete(() =>
        {
            // If customer has waited for half of his/her patience, make him/her bored.
            if (newPatience <= patienceDuration / 2 && currentPatience > patienceDuration / 2)
            {
                customerController.UpdateCustomerMood(1); // 1 is bored index
            }

            // Handle when patience runs out
            if (newPatience == 0)
            {
                HandlePatienceDepleted();
            }
        });

        // Start the sequence
        blinkSequence.Play();
    }
}