using UnityEngine;
using UnityEngine.Pool;

namespace BazarEsKrim
{
    public class CounterSpawner : MonoBehaviour
    {
        public ObjectPool<PointCounterAnim> pointCounterPool;

        [SerializeField] private PointCounterAnim pointCounterObj;
        [SerializeField] private Canvas canvas; // Reference to the Canvas

        private void OnEnable()
        {
            EventHandler.SetPointCounterPos += SetPosAndTakePoint;
        }

        private void OnDisable()
        {
            EventHandler.SetPointCounterPos -= SetPosAndTakePoint;
        }

        private void Start()
        {
            pointCounterPool = new ObjectPool<PointCounterAnim>(CreatePointCounter, OnTakePointCounterFromPool, OnReturnPointCounterToPool, OnDestroyPointCounter, true, 20, 25);
        }

        private PointCounterAnim CreatePointCounter()
        {
            // spawn
            PointCounterAnim pointObj = Instantiate(pointCounterObj, transform);

            // assign the pool and canvas
            pointObj.SetPool(pointCounterPool);

            return pointObj;
        }

        private void OnTakePointCounterFromPool(PointCounterAnim pointCounter)
        {
            // activate
            pointCounter.gameObject.SetActive(true);
        }

        private void OnReturnPointCounterToPool(PointCounterAnim pointCounter)
        {
            // deactivate
            pointCounter.gameObject.SetActive(false);
        }

        private void OnDestroyPointCounter(PointCounterAnim pointCounter)
        {
            Destroy(pointCounter.gameObject);
        }

        private void SetPosAndTakePoint(Vector3 fallingObjPos, Vector3 basketPos)
        {
            // Get a point counter from the pool
            PointCounterAnim pointCounter = pointCounterPool.Get();

            // // Convert the world position to screen point
            // Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition.position);

            // // Convert the screen point to UI position
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, canvas.worldCamera, out Vector2 uiPosition);

            // Set the UI position
            pointCounter.transform.position = new Vector2(fallingObjPos.x, basketPos.y);

            pointCounter.MoveUp();
        }
    }
}
