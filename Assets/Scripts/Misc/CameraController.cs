using UnityEngine;
using System;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 3f;       // Speed of camera pan based on mouse/touch movement
    public float smoothTime = 0.3f;     // Time to smoothly ease to a stop
    public float minX = -10f;           // Minimum X position the camera can move to
    public float maxX = 10f;            // Maximum X position the camera can move to

    private float velocity = 0f;        // Reference velocity for SmoothDamp
    private float targetPositionX;      // Target X position for the camera
    private float previousCameraPositionX; // Previous X position of the camera for parallax calculations
    private bool isDragging = false;    // Tracks whether the user is dragging the camera
    private Vector2 lastMousePosition;  // Last position of the mouse or touch

    void Start()
    {
        // transform.position = Vector2.zero;

        Init();
    }

    private void Init()
    {
        // Set the initial target position to the current camera position
        targetPositionX = transform.position.x;
    }

    void Update()
    {
        if (GameManager.Instance.gameStates == GameStates.LevelSelection)
        {
            HandleInput();  // Handles both mouse and touch input

            float newPositionX = Mathf.SmoothDamp(transform.position.x, targetPositionX, ref velocity, smoothTime);

            transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);

            // Notify parallax layers about the camera movement
            EventHandler.CallCameraMoveEvent(newPositionX - previousCameraPositionX);

            previousCameraPositionX = newPositionX;
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePosition;
            targetPositionX -= mouseDelta.x * panSpeed * Time.deltaTime;

            targetPositionX = Mathf.Clamp(targetPositionX, minX, maxX);

            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    public void SetToTarget(float targetX)
    {
        // Initialize the camera's position to the minimum x value
        Vector3 initialPosition = transform.position;
        initialPosition.x = targetX;
        transform.position = initialPosition;

        Init();

        Debug.Log("Set Camera To Target " + targetX);
    }
}
