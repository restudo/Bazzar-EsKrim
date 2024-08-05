using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using System.Collections;

public class FallingObject : MonoBehaviour
{
    [SerializeField] private float fallDuration;
    [SerializeField] private float releaseDelay;

    private ObjectPool<FallingObject> fallingObjectPool;
    private GameObject basket;
    // private GameObject topBasket;
    private GameObject fallingObjectDestroyer;
    private PolygonCollider2D destroyerCol;
    private Bounds bounds;
    private Vector2 min;
    private Vector2 max;
    private Vector2 randomPoint;
    private SpriteRenderer spRend;
    private bool isCanBeTouch;

    // private const int sortOrder = 2;

    private void Awake()
    {
        basket = GameObject.FindGameObjectWithTag("Basket");
        // topBasket = basket.transform.GetChild(0).gameObject;
        fallingObjectDestroyer = GameObject.FindGameObjectWithTag("Falling Object Destroyer");
        spRend = GetComponentInChildren<SpriteRenderer>();

        destroyerCol = fallingObjectDestroyer.GetComponent<PolygonCollider2D>();
        bounds = destroyerCol.bounds;
        min = bounds.min;
        max = bounds.max;
        randomPoint = Vector2.zero;
    }

    private void OnEnable()
    {
        isCanBeTouch = true;

        // spRend.sortingOrder = -sortOrder;

        randomPoint = Vector2.zero;
        GetRandomPointInPolygon();
    }

    private void OnMouseUp()
    {
        if (GameManager.Instance.isGameActive && isCanBeTouch && GameManager.Instance.gameStates == GameStates.MiniGame)
        {
            isCanBeTouch = false;

            // spRend.sortingOrder = sortOrder;

            float randomOffset = Random.Range(-1, 1);
            // Vector3 topBasketPos = topBasket.transform.position;
            Vector3 basketPos = basket.transform.position;

            DOTween.Kill(transform);

            // transform.DOMove(new Vector3(topBasketPos.x + randomOffset, topBasketPos.y, topBasketPos.z), 1f).SetEase(Ease.OutExpo).OnComplete(() =>
            // {
            // transform.DOMove(new Vector3(topBasketPos.x + randomOffset, topBasketPos.y, topBasketPos.z), 0.8f).SetEase(Ease.InExpo);

            transform.DOJump(new Vector3(basketPos.x + randomOffset, basketPos.y, basketPos.z), 5f, 1, 0.5f, false);
            // });
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.gameObject == topBasket)
        // {
        //     DOTween.Kill(transform);


        // }

        if (other.gameObject == fallingObjectDestroyer)
        {
            // isCanBeTouch = false;

            StartCoroutine(ReleasePoolWithDelay());
        }

        if (other.gameObject == basket)
        {
            isCanBeTouch = false;

            DOTween.Kill(transform);

            EventHandler.CallAddMiniGameScoreEvent();
            EventHandler.CallBasketFlashEffectEvent();

            // StartCoroutine(ReleasePoolWithDelay());
            fallingObjectPool.Release(this);
        }
    }

    private void GetRandomPointInPolygon()
    {
        while (true)
        {
            randomPoint = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (destroyerCol.OverlapPoint(randomPoint))
            {
                StartCoroutine(FallDelay(randomPoint));
                return;
            }
        }
    }

    private IEnumerator FallDelay(Vector2 randomPoint)
    {
        yield return new WaitForSeconds(0.2f);

        // int additionalY = 2;
        transform.DOMoveY(randomPoint.y, fallDuration).SetEase(Ease.Linear);
    }

    private IEnumerator ReleasePoolWithDelay()
    {
        yield return new WaitForSeconds(releaseDelay);

        isCanBeTouch = false;

        DOTween.Kill(transform);

        fallingObjectPool.Release(this);
    }

    public void SetPool(ObjectPool<FallingObject> pool)
    {
        fallingObjectPool = pool;
    }
}
