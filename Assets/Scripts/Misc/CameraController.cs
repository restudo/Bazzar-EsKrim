using System.Collections;
using UnityEngine;

namespace BazarEsKrim
{
    public class CameraController : MonoBehaviour
    {
        [Header("Control Params")]
        public float panSpeed = 3f;       // Speed of camera pan based on mouse/touch movement
        public float smoothTime = 0.3f;   // Time to smoothly ease to a stop
        public float minX = -10f;         // Minimum X position the camera can move to
        public float maxX = 10f;          // Maximum X position the camera can move to
        public float dragThreshold = 0.1f; // Tolerance for dragging before moving the camera
        public float holdSmoothTime = 0.1f; // Reduced smooth time when holding after drag

        [Space(20)]
        [Header("Animation Params")]
        public float moveSpeed = 20f;
        public float smoothThreshold = 3f;

        private float currentSmoothTime;
        private float velocity = 0f;        // Reference velocity for SmoothDamp
        private float targetPositionX;      // Target X position for the camera
        private float previousCameraPositionX; // Previous X position of the camera for parallax calculations
        private bool isDragging = false;    // Tracks whether the user is dragging the camera
        private bool isHolding = false;     // Tracks whether the user is holding the mouse after dragging
        private bool isAnim = false; // Tracks wheter is it an set to target
        private Vector2 lastMousePosition;  // Last position of the mouse or touch
        private Coroutine moveAnimCoroutine; // Store the coroutine reference

        void Awake()
        {
            Init();
        }

        private void OnDisable()
        {
            if (moveAnimCoroutine != null)
            {
                StopCoroutine(moveAnimCoroutine);
                moveAnimCoroutine = null; // Clear the reference
            }
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
            if (GameManager.Instance.gameStates == GameStates.LevelSelection && !isAnim)
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

        private IEnumerator SetAfterDelay(float targetX)
        {
            // Wait for a delay before starting the movement
            yield return new WaitForSeconds(0.2f);

            float distanceToTarget;

            // move towards the target position
            while (true)
            {
                distanceToTarget = Mathf.Abs(transform.position.x - targetX);

                if (distanceToTarget <= smoothThreshold)
                    break; // Stop the fast move when close to the target

                // Move directly towards the target without smoothing
                Vector3 moveStep = Vector3.MoveTowards(transform.position, new Vector3(targetX, transform.position.y, transform.position.z), moveSpeed * Time.deltaTime);
                transform.position = moveStep;

                EventHandler.CallCameraMoveEvent(transform.position.x - previousCameraPositionX);

                previousCameraPositionX = transform.position.x;

                yield return null;
            }

            // smoothly move the camera to the target position using SmoothDamp
            while (distanceToTarget > 0.01f) // Continue smoothing until almost exactly at the target
            {
                float newPositionX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocity, smoothTime);
                transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);

                // Notify parallax layers about the camera movement
                EventHandler.CallCameraMoveEvent(newPositionX - previousCameraPositionX);

                previousCameraPositionX = newPositionX;

                distanceToTarget = Mathf.Abs(newPositionX - targetX);

                yield return null; // Wait for the next frame
            }

            // Ensure the final position is exactly the target
            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
            EventHandler.CallCameraMoveEvent(targetX - previousCameraPositionX);

            Init();

            isAnim = false;

            moveAnimCoroutine = null;
        }

        public void SetToTarget(float targetX)
        {
            isAnim = true;

            // Smoothly move the camera to the target position
            Init();

            moveAnimCoroutine = StartCoroutine(SetAfterDelay(targetX));
        }
    }
}
