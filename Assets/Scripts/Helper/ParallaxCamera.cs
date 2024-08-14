using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    private float oldPosition;

    void Start()
    {
        oldPosition = transform.position.x;
    }

    void Update()
    {
        if (transform.position.x != oldPosition)
        {
            float delta = oldPosition - transform.position.x;
            EventHandler.CallCameraMoveEvent(delta);

            oldPosition = transform.position.x;
        }
    }
}