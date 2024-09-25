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
        public float holdSmoothTime = 0.1f; // Reduced smooth time when holding after drag

        private float currentSmoothTime;
        private float velocity = 0f;        // Reference velocity for SmoothDamp
        private float targetPositionX;      // Target X position for the camera
        private float previousCameraPositionX; // Previous X position of the camera for parallax calculations
        private bool isDragging = false;    // Tracks whether the user is dragging the camera
        private Vector2 lastMousePosition;  // Last position of the mouse or touch
        private bool isHolding = false;     // Tracks whether the user is holding the mouse after dragging

        void Start()
        {
            Init();
        }

        private void Init()
        {
            // Set the initial target position to the current camera position
            targetPositionX = transform.position.x;
            previousCameraPositionX = targetPositionX;  // Initialize previous camera position

            currentSmoothTime = smoothTime;
        }

        void Update()
        {
            if (GameManager.Instance.gameStates == GameStates.LevelSelection)
            {
                HandleInput();  // Handles both mouse and touch input

                // Smoothly move the camera to the target position
                float newPositionX = Mathf.SmoothDamp(transform.position.x, targetPositionX, ref velocity, currentSmoothTime);
                transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);

                // Notify parallax layers about the camera movement
                EventHandler.CallCameraMoveEvent(newPositionX - previousCameraPositionX);

                // Update previous camera position
                previousCameraPositionX = newPositionX;
            }
        }

        void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                isHolding = false;
                lastMousePosition = Input.mousePosition; // Track mouse position in screen space
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentMousePosition = Input.mousePosition; // Track current mouse position in screen space
                float mouseDeltaX = currentMousePosition.x - lastMousePosition.x; // Calculate horizontal delta in screen space

                // If movement exceeds the dragThreshold, start moving the camera
                if (Mathf.Abs(mouseDeltaX) > dragThreshold)
                {
                    // Convert screen delta to world space for movement
                    Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(currentMousePosition.x, 0, 0))
                                         - Camera.main.ScreenToWorldPoint(new Vector3(lastMousePosition.x, 0, 0));

                    targetPositionX -= worldDelta.x * panSpeed; // Move the camera target
                    targetPositionX = Mathf.Clamp(targetPositionX, minX, maxX);

                    currentSmoothTime = smoothTime; // Use regular smoothTime during dragging

                    lastMousePosition = currentMousePosition; // Update last mouse position
                    isHolding = false; // Reset holding if dragging
                }
                else
                {
                    // If the mouse delta is small (below threshold), enter "holding" mode
                    if (!isHolding)
                    {
                        currentSmoothTime = holdSmoothTime; // Set smooth time for holding
                        isHolding = true; // Set the holding flag
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                currentSmoothTime = smoothTime;  // Reset smoothTime when the mouse button is released
                isDragging = false; // Stop dragging
                isHolding = false;  // Stop holding
            }
        }

        public void SetToTarget(float targetX)
        {
            // Set camera's position to the given target X value
            // Vector3 initialPosition = transform.position;
            // initialPosition.x = targetX;
            // transform.position = initialPosition;

            // Smoothly move the camera to the target position
            Init();

            // Smoothly move the camera to the target position
            float newPositionX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocity, currentSmoothTime);
            transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);

            // Notify parallax layers about the camera movement
            EventHandler.CallCameraMoveEvent(newPositionX - previousCameraPositionX);

            // Update previous camera position
            previousCameraPositionX = newPositionX;

            Debug.Log("Set Camera To Target " + targetX);
        }
    }
}
