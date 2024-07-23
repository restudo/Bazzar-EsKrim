using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    public ObjectPool<FallingObject> fallingObjectPool;

    [SerializeField] private FallingObject[] fallingObjects;
    [SerializeField] private float spawnDelay;
    [SerializeField] private float xOffset; // Offset to add to minX and maxX

    private Vector3 screenLeft;
    private Vector3 screenRight;
    private float minX;
    private float maxX;
    private float elapsedTime;

    private void Start()
    {
        fallingObjectPool = new ObjectPool<FallingObject>(CreateFallingObject, OnTakeFallingObjectFromPool, OnReturnFallingObjectToPool, OnDestroyFallingObject, true, 15, 20);

        // Get screen boundaries in world coordinates
        screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Camera.main.nearClipPlane));

        minX = screenLeft.x + xOffset; // Adding xOffset to minX
        maxX = screenRight.x - xOffset; // Subtracting xOffset from maxX
    }

    void Update()
    {
        if (GameManager.Instance.gameStates == GameStates.MiniGame && GameManager.Instance.isGameActive)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= spawnDelay)
            {
                elapsedTime = 0.0f;

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
        fallingObject.transform.position = new Vector3(randomX, transform.position.y, transform.position.z);

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
