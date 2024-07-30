using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float minX = -4.25f; // Minimum x value for the camera
    [SerializeField] private float maxX = 4.25f;  // Maximum x value for the camera
    private Vector3 touchStart;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        // Initialize the camera's position to the minimum x value
        Vector3 initialPosition = mainCamera.transform.position;
        initialPosition.x = minX;
        mainCamera.transform.position = initialPosition;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Store the initial touch position in world coordinates
            touchStart = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            // Get the current touch position in world coordinates
            Vector3 touchEnd = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // Calculate the movement delta
            float deltaX = touchStart.x - touchEnd.x;

            // Move the camera
            Vector3 newPosition = mainCamera.transform.position + new Vector3(deltaX, 0, 0);

            // Clamp the new position's x value between minX and maxX
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

            // Set the camera's position to the new position
            mainCamera.transform.position = newPosition;

            // Update touchStart to the current position for continuous movement
            touchStart = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}