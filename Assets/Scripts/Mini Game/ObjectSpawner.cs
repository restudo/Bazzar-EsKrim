using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    public ObjectPool<FallingObject> fallingObjectPool;

    [SerializeField] private FallingObject[] fallingObjects;
    [SerializeField] private float xOffset; // Offset to add to minX and maxX

    private Vector3 screenLeft;
    private Vector3 screenRight;
    private float spawnDelay;
    private float minX;
    private float maxX;
    private float excludeMinX;
    private float excludeMaxX;
    private float elapsedTime;
    private float yPos;

    private void Start()
    {
        fallingObjectPool = new ObjectPool<FallingObject>(CreateFallingObject, OnTakeFallingObjectFromPool, OnReturnFallingObjectToPool, OnDestroyFallingObject, true, 20, 25);

        // Get screen boundaries in world coordinates
        screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Camera.main.nearClipPlane));

        minX = screenLeft.x + xOffset; // Adding xOffset to minX
        maxX = screenRight.x - xOffset; // Subtracting xOffset from maxX

        spawnDelay = Random.Range(0.5f, 2f);

        yPos = transform.position.y;
    }

    void Update()
    {
        if (GameManager.Instance.gameStates == GameStates.MiniGame && GameManager.Instance.isGameActive)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= spawnDelay)
            {
                spawnDelay = Random.Range(0.5f, 2f);

                elapsedTime = 0f;

                // Spawn the object
                fallingObjectPool.Get();
            }
        }
    }

    private FallingObject CreateFallingObject()
    {
        // spawn
        FallingObject fallingObject = Instantiate(fallingObjects[Random.Range(0, fallingObjects.Length)], transform);

        // assign the pool
        fallingObject.SetPool(fallingObjectPool);

        return fallingObject;
    }

    private void OnTakeFallingObjectFromPool(FallingObject fallingObject)
    {
        // Generate a random x-coordinate within the screen width range with offset
        float randomX = Random.Range(minX, maxX);

        // set the transform
        // z transform is set to the Camera nearClipPlane, so its always on top 
        fallingObject.transform.position = new Vector3(randomX, yPos, -Camera.main.nearClipPlane);

        // activate
        fallingObject.gameObject.SetActive(true);
    }

    private void OnReturnFallingObjectToPool(FallingObject fallingObject)
    {
        // deactivate
        fallingObject.gameObject.SetActive(false);
    }

    private void OnDestroyFallingObject(FallingObject fallingObject)
    {
        Destroy(fallingObject.gameObject);
    }
}
