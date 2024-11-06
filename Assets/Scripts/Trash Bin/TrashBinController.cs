using DG.Tweening;
using UnityEngine;

public class TrashBinController : MonoBehaviour
{
    public bool isCloseEnoughToTrashbin = true; //Do not modify this.
    //Textures for open/closed states
    public Sprite[] state;

    private Transform topSprite;
    private Vector3 topPosition;
    private Vector3 topRotation;

    private Vector3 bodyPosition;
    private Vector3 bodyScale;

    private GameObject deliveryPlate;
    private IngredientHolder ingredientHolder;
    private Collider2D trashBinCol;
    private Collider2D deliveryPlateCol;

    private int currentClick;
    private bool isDeliveryPlateColliding;
    private bool isMousePositionColliding;
    private bool isAnimate;
    private Vector2 mousePosition;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        trashBinCol = GetComponent<Collider2D>();
        topSprite = gameObject.transform.GetChild(0).transform;

        deliveryPlate = FindObjectOfType<IngredientHolder>().gameObject;
        if (deliveryPlate != null)
        {
            ingredientHolder = deliveryPlate.GetComponent<IngredientHolder>();
            deliveryPlateCol = deliveryPlate.GetComponent<Collider2D>();
        }

        isCloseEnoughToTrashbin = false;
        isAnimate = false;
        spriteRenderer.sprite = state[0];

        mainCamera = Camera.main;

        currentClick = 0;

        topPosition = topSprite.localPosition;
        topRotation = topSprite.localEulerAngles;

        bodyPosition = transform.localPosition;
        bodyScale = transform.localScale;
    }

    private void OnEnable()
    {
        EventHandler.CloseTrashBin += CloseTrashBin;
        // EventHandler.SquishTrashBin += Squish;
    }

    private void OnDisable()
    {
        EventHandler.CloseTrashBin -= CloseTrashBin;
        // EventHandler.SquishTrashBin -= Squish;
    }

    void LateUpdate()
    {
        //check if player wants to move the order to trash bin
        if (ingredientHolder.canDeliverOrder)
        {
            CheckDistanceToDelivery();
        }
    }

    void CheckDistanceToDelivery()
    {
        if (trashBinCol == null || deliveryPlateCol == null)
        {
            isCloseEnoughToTrashbin = false;
            return;
        }

        // Get the mouse position in world coordinates
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Check if the delivery plate bounds intersect with the trash bin bounds
        isDeliveryPlateColliding = trashBinCol.bounds.Intersects(deliveryPlateCol.bounds);

        // Check if the mouse position is within the bounds of the trash bin collider
        isMousePositionColliding = trashBinCol.bounds.Contains(mousePosition);

        // Set the flag if both conditions are true
        // Optionally change the texture based on the flag
        if (isDeliveryPlateColliding && isMousePositionColliding)
        {
            isCloseEnoughToTrashbin = true;

            if (!isAnimate)
            {
                // OpenTrashBin();

                isAnimate = true;
            }
        }
        else
        {
            isCloseEnoughToTrashbin = false;

            if (isAnimate)
            {
                // CloseTrashBin();

                // Squish();

                // Debug.Log("Squish and close");

                isAnimate = false;
            }
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.gameStates == GameStates.MainGame && GameManager.Instance.isGameActive)
        {
            currentClick++;

            if (currentClick == 1)
            {
                OpenTrashBin();
            }

            if (currentClick == 2)
            {
                deliveryPlate.transform.DOMove(transform.position, 0.4f).SetEase(Ease.OutExpo).OnComplete(() =>
                {
                    EventHandler.CallResetMainQueueEvent();
                    EventHandler.CallResetPlatePositionEvent();

                    CloseTrashBin();

                    // Squish();

                    currentClick = 0;
                });

            }
        }

    }

    // private void Squish()
    // {
    //     // // squish
    //     // transform.DOScaleY(0.6f, 0.1f).SetEase(Ease.OutExpo);
    //     // transform.DOLocalMoveY(-3.5616f, 0.1f).SetEase(Ease.OutExpo).OnComplete(() =>
    //     // {
    //     //     transform.DOScaleY(bodyScale.y, 0.1f).SetEase(Ease.OutExpo);
    //     //     transform.DOLocalMoveY(bodyPosition.y, 0.1f).SetEase(Ease.OutExpo);
    //     // });
    // }

    public void CloseTrashBin()
    {
        spriteRenderer.sprite = state[0];

        // topSprite.DOLocalRotate(topRotation, 0.15f).SetEase(Ease.OutExpo);
        // topSprite.DOLocalMove(topPosition, 0.15f).SetEase(Ease.OutExpo);
    }

    public void OpenTrashBin()
    {
        spriteRenderer.sprite = state[1];

        // topSprite.DOLocalRotate(new Vector3(0, 0, -29), 0.3f).SetEase(Ease.OutExpo);
        // topSprite.DOLocalMove(new Vector3(0.242f, 1.776f, 0), 0.3f).SetEase(Ease.OutExpo);
    }
}
