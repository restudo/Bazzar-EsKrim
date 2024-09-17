using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxFactor; // Parallax factor for this layer

    private float previousLayerPositionX;

    private void OnEnable()
    {
        EventHandler.CameraMove += ApplyParallax;
    }

    private void OnDisable()
    {
        EventHandler.CameraMove -= ApplyParallax;
    }

    void Start()
    {
        // Initialize previousLayerPositionX to current layer position
        previousLayerPositionX = transform.position.x;
    }

    private void ApplyParallax(float deltaX)
    {
        // Apply the parallax effect to this layer
        Vector3 newLayerPosition = transform.position;
        newLayerPosition.x += deltaX * parallaxFactor;
        transform.position = newLayerPosition;

        // Update the previousLayerPositionX for the next frame
        previousLayerPositionX = transform.position.x;
    }
}
