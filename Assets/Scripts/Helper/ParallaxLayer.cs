using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float parallaxFactor; // Parallax factor for this layer

    private void OnEnable()
    {
        EventHandler.CameraMove += ApplyParallax;
    }

    private void OnDisable()
    {
        EventHandler.CameraMove -= ApplyParallax;
    }

    private void ApplyParallax(float deltaX)
    {
        Vector3 newPosition = transform.position;
        newPosition.x += deltaX * parallaxFactor;
        transform.position = newPosition;
    }
}
