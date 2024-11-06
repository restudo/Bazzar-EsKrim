using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using System.Collections;

public class FallingObject : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField] private float minfallDuration;
    [SerializeField] private float maxfallDuration;
    [SerializeField] private float rotationSpeedMax;
    [SerializeField] private float releaseDelay;

    [Space(20)]
    [Header("Flash")]
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.1f; // Duration of each flash
    [SerializeField] private int flashCount = 1; // Number of flashes

    private ObjectPool<FallingObject> fallingObjectPool;
    private GameObject basket;
    // private GameObject topBasket;
    private GameObject fallingObjectDestroyer;
    private Collider2D destroyerCol;
    private Bounds bounds;
    private Vector2 min;
    private Vector2 max;
    private Vector2 randomPoint;
    private SpriteRenderer spRend;
    private Material originalMaterial;
    private bool isCanBeTouch;

    private const int sortOrder = 2;
    private const int additionalY = 2;

    private void Awake()
    {
        basket = GameObject.FindGameObjectWithTag("Basket");
        // topBasket = basket.transform.GetChild(0).gameObject;
        fallingObjectDestroyer = GameObject.FindGameObjectWithTag("Falling Object Destroyer");
        spRend = GetComponentInChildren<SpriteRenderer>();

        destroyerCol = fallingObjectDestroyer.GetComponent<Collider2D>();
        bounds = destroyerCol.bounds;
        min = bounds.min;
        max = bounds.max;
        randomPoint = Vector2.zero;

        //set the mat to original
        originalMaterial = spRend.material;
    }

    private void OnEnable()
    {
        EventHandler.MiniGameWin += MiniGameWin;

        isCanBeTouch = true;

        spRend.material = originalMaterial;
        spRend.sortingOrder = -sortOrder;

        randomPoint = Vector2.zero;

        StartCoroutine(FallDelay(Random.Range(minfallDuration, maxfallDuration)));
        // GetRandomPointInPolygon();
    }

    private void OnDisable()
    {
        EventHandler.MiniGameWin -= MiniGameWin;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.isGameActive && isCanBeTouch && GameManager.Instance.gameStates == GameStates.MiniGame)
        {
            isCanBeTouch = false;

            // make it flash
            FlashEffect();

            spRend.sortingOrder = sortOrder;

            float randomOffset = Random.Range(-2, 2);
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
        // if (GameManager.Instance.isGameActive)
        // {
            if (other.gameObject == fallingObjectDestroyer && spRend.sortingOrder == -sortOrder)
            {
                isCanBeTouch = false;

                // DOTween.Kill(transform);

                StartCoroutine(ReleasePoolWithDelay());
            }

            if (other.gameObject == basket && spRend.sortingOrder == sortOrder)
            {
                isCanBeTouch = false;

                DOTween.Kill(transform);

                EventHandler.CallAddMiniGameScoreEvent();
                EventHandler.CallBasketFlashEffectEvent(transform.position.x);
                EventHandler.CallSetPointCounterPosEvent(transform.position, basket.transform.position);

                // StartCoroutine(ReleasePoolWithDelay());
                fallingObjectPool.Release(this);
            }
        // }
    }

    // private void GetRandomPointInPolygon()
    // {
    //     while (true)
    //     {
    //         randomPoint = new Vector2(
    //             Random.Range(min.x, max.x),
    //             Random.Range(min.y, max.y)
    //         );

    //         if (destroyerCol.OverlapPoint(randomPoint))
    //         {
    //             StartCoroutine(FallDelay(randomPoint));
    //             return;
    //         }
    //     }
    // }

    private IEnumerator FallDelay(float fallDuration)
    {
        yield return new WaitForSeconds(0.2f);

        transform.DOMoveY(fallingObjectDestroyer.transform.position.y - additionalY, fallDuration).SetEase(Ease.Linear);

        float randomSpeed = Random.Range(rotationSpeedMax - rotationSpeedMax, rotationSpeedMax);

        // Randomly decide the rotation direction
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;

        // Rotate the sprite infinitely around the Z-axis in the chosen direction
        transform.DORotate(new Vector3(0, 0, 360 * direction), 1 / randomSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental) // Infinite loop, adds the rotation value each loop
            .SetEase(Ease.Linear); // Linear ease for constant rotation speed
    }

    private IEnumerator ReleasePoolWithDelay()
    {
        yield return new WaitForSeconds(releaseDelay);

        isCanBeTouch = false;

        DOTween.Kill(transform);

        fallingObjectPool.Release(this);
    }

    private void FlashEffect()
    {
        if (spRend != null)
        {
            spRend.material = originalMaterial;

            StartCoroutine(StartFlashEffect());
        }
    }

    private IEnumerator StartFlashEffect()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // Swap to the flashMaterial.
            spRend.material = flashMaterial;

            // Pause the execution of this function for "duration" seconds.
            yield return new WaitForSeconds(flashDuration);

            // After the pause, swap back to the original material.
            spRend.material = originalMaterial;
        }
    }

    private void MiniGameWin()
    {
        // DOTween.Kill(transform);

        // fallingObjectPool.Release(this);
    }

    public void SetPool(ObjectPool<FallingObject> pool)
    {
        fallingObjectPool = pool;
    }
}
