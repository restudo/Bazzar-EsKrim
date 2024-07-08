using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float minX = -4.25f; // Minimum x value for the camera
    [SerializeField] private float maxX = 4.25f;  // Maximum x value for the camera
    private Vector3 touchStart;

    private void Start()
    {
        Camera.main.transform.position = new Vector3(minX, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 touchEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = new Vector3(touchStart.x - touchEnd.x, 0, 0); // Only pan horizontally
            Vector3 newPosition = Camera.main.transform.position + direction;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX); // Clamp the x position
            Camera.main.transform.position = newPosition;
        }
    }
}
