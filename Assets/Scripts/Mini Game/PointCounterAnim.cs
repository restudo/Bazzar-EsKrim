using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;
using System.Collections;

namespace BazarEsKrim
{
    public class PointCounterAnim : MonoBehaviour
    {
        [SerializeField] private float minIncrement;
        [SerializeField] private float MaxIncrement;
        [SerializeField] private float releaseDelay;

        private ObjectPool<PointCounterAnim> pointPool;

        private IEnumerator Move()
        {
            float randomY = Random.Range(minIncrement, MaxIncrement);

            transform.DOMoveY(transform.position.y + randomY, 0.2f).SetEase(Ease.OutExpo);

            yield return new WaitForSeconds(releaseDelay);

            pointPool.Release(this);
        }

        public void MoveUp()
        {
            StartCoroutine(Move());
        }

        public void SetPool(ObjectPool<PointCounterAnim> pool)
        {
            pointPool = pool;
        }
    }
}
