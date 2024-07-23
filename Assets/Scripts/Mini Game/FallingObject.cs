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
    private GameObject topBasket;
    private GameObject fallingObjectDestroyer;
    private SpriteRenderer spRend;
    private bool isCanBeTouch;

    private const int sortOrder = 2;

    private void Awake()
    {
        basket = GameObject.FindGameObjectWithTag("Basket");
        topBasket = basket.transform.GetChild(0).gameObject;
        fallingObjectDestroyer = GameObject.FindGameObjectWithTag("Falling Object Destroyer");
        spRend = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isCanBeTouch = true;

        spRend.sortingOrder = -sortOrder;

        StartCoroutine(FallDelay());
    }

    private void OnMouseUp()
    {
        if (GameManager.Instance.isGameActive && isCanBeTouch && GameManager.Instance.gameStates == GameStates.MiniGame)
        {
            isCanBeTouch = false;

            spRend.sortingOrder = sortOrder;

            float randomOffset = Random.Range(-1, 1);
            Vector3 topBasketPos = topBasket.transform.position;

            DOTween.Kill(transform);

            transform.DOMove(new Vector3(topBasketPos.x + randomOffset, topBasketPos.y, topBasketPos.z), 1f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                transform.DOMove(basket.transform.position, 0.5f).SetEase(Ease.InExpo);
            });
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
            isCanBeTouch = false;

            StartCoroutine(ReleasePoolWithDelay());
        }

        if(other.gameObject == basket)
        {
            EventHandler.CallAddMiniGameScoreEvent();

            isCanBeTouch = false;

            StartCoroutine(ReleasePoolWithDelay());
        }
    }

    private IEnumerator FallDelay()
    {
        yield return new WaitForSeconds(releaseDelay / 2);

        int additionalY = 2;
        transform.DOMoveY(fallingObjectDestroyer.transform.position.y - additionalY, fallDuration).SetEase(Ease.Linear);
    }

    private IEnumerator ReleasePoolWithDelay()
    {
        yield return new WaitForSeconds(releaseDelay);

        fallingObjectPool.Release(this);
    }

    public void SetPool(ObjectPool<FallingObject> pool)
    {
        fallingObjectPool = pool;
    }
}
