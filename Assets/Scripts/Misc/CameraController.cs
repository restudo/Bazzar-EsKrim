using UnityEngine;

namespace BazarEsKrim
{
    public class CameraController : MonoBehaviour
    {
        public float panSpeed = 3f;       // Speed of camera pan based on mouse/touch movement
        public float smoothTime = 0.3f;   // Time to smoothly ease to a stop
        public float minX = -10f;         // Minimum X position the camera can move to
        public float maxX = 10f;          // Maximum X position the camera can move to
        public float dragThreshold = 0.1f; // Tolerance for dragging before moving the camera

        private float velocity = 0f;        // Reference velocity for SmoothDamp
        private float targetPositionX;      // Target X position for the camera
        private float previousCameraPositionX; // Previous X position of the camera for parallax calculations
        private bool isDragging = false;    // Tracks whether the user is dragging the camera
        private Vector2 lastMousePosition;  // Last position of the mouse or touch

        void Start()
        {
            Init();
        }

        private void Init()
        {
            // Set the initial target position to the current camera position
            targetPositionX = transform.position.x;
            previousCameraPositionX = targetPositionX;  // Initialize previous camera position
        }

        void Update()
        {
            if (GameManager.Instance.gameStates == GameStates.LevelSelection)
            {
                HandleInput();  // Handles both mouse and touch input

                // Smoothly move the camera to the target position
                float newPositionX = Mathf.SmoothDamp(transform.position.x, targetPositionX, ref velocity, smoothTime);
                transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);

                // Notify parallax layers about the camera movement
                EventHandler.CallCameraMoveEvent(newPositionX - previousCameraPositionX);

                // Update previous camera position
                previousCameraPositionX = newPositionX;
            }
        }

        void HandleInput()
        {
            // Use mouse input for editor or standalone platforms
            HandleMouseInput();
        }

        void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Convert mouse position to world space
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Convert mouse position to world space
                float distance = Vector2.Distance(currentMousePosition, lastMousePosition); // Calculate distance between current and last positions

                // Only apply movement if the horizontal delta exceeds the threshold
                if (distance > dragThreshold)
                {
                    float mouseDeltaX = currentMousePosition.x - lastMousePosition.x; // Calculate horizontal delta in world space
                    targetPositionX -= mouseDeltaX * panSpeed * Time.deltaTime;
                    targetPositionX = Mathf.Clamp(targetPositionX, minX, maxX);
                    lastMousePosition = currentMousePosition; // Update the last mouse position after applying movement

                    // After calculating the movement, update lastMousePosition to the current touch position
                    lastMousePosition = currentMousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false; // Only stop dragging when the mouse button is released
            }
        }

        public void SetToTarget(float targetX)
        {
            // Set camera's position to the given target X value
            Vector3 initialPosition = transform.position;
            initialPosition.x = targetX;
            transform.position = initialPosition;

            Init();

            Debug.Log("Set Camera To Target " + targetX);
        }
    }
}
