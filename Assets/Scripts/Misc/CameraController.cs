using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float minX = -4.25f; // Minimum x value for the object
    [SerializeField] private float maxX = 4.25f;  // Maximum x value for the object
    private Vector3 touchStart;

    private void Start()
    {
        // Initialize the object's position to the minimum x value
        transform.position = new Vector3(minX, transform.position.y, transform.position.z);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Convert screen touch position to world position
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            // Convert screen touch position to world position
            Vector3 touchEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Calculate the direction vector based on the touch movement
            Vector3 direction = new Vector3(touchEnd.x - touchStart.x, 0, 0); // Inverted direction for opposite movement

            // Calculate the new position for the object
            Vector3 newPosition = transform.position + direction;

            // Clamp the new position's x value between minX and maxX
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

            // Set the object's position to the new position
            transform.position = newPosition;

            // Update touchStart to the current position for continuous movement
            touchStart = touchEnd;
        }
    }
}
