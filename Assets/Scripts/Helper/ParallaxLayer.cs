using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxFactor; // Parallax factor for this layer

    private Vector3 previousLayerPosition;

    private void OnEnable()
    {
        EventHandler.CameraMove += ApplyParallax;

        SetTransformToInitial();
    }

    private void OnDisable()
    {
        EventHandler.CameraMove -= ApplyParallax;
    }

    void Awake()
    {
        // Initialize previousLayerPositionX to current layer position
        previousLayerPosition = transform.position;
    }

    private void ApplyParallax(float deltaX)
    {
        // Apply the parallax effect to this layer
        Vector3 newLayerPosition = transform.position;
        newLayerPosition.x += deltaX * parallaxFactor;
        transform.position = newLayerPosition;
    }

    private void SetTransformToInitial()
    {
        transform.position = previousLayerPosition;
    }
}
