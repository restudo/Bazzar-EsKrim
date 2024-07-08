using UnityEngine;
using UnityEngine.Pool;

public class Ingredient : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private int _ingredientCode;
    public int IngredientCode { get { return _ingredientCode; } set { _ingredientCode = value; } }
    [SerializeField] private IngredientType _ingredientType;
    public IngredientType IngredientType { get { return _ingredientType; } set { _ingredientType = value; } }
    // [SerializeField] private GameObject[] allIngredients;
    // [SerializeField] private SO_IngredientObject allIngredients;

    private ObjectPool<Ingredient> ingredientPool;
    private bool isReleased = false;

    // private void Awake()
    // {
    //     spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    // }

    // private void Start()
    // {
    //     if (IngredientCode != 0)
    //     {
    //         Init(IngredientCode);
    //     }
    // }

    // public void Init(int ingredientCodeParam)
    // {
    //     foreach (GameObject item in allIngredients.ingredientObjects)
    //     {
    //         if (item.GetComponent<Ingredient>().IngredientCode == ingredientCodeParam)
    //         {
    //             transform.GetChild(transform.childCount - 1).localPosition = item.transform.GetChild(item.transform.childCount - 1).localPosition;
    //         }
    //     }
    // }  

    public void SetIngredientPool(ObjectPool<Ingredient> pool)
    {
        ingredientPool = pool;
    }

    public ObjectPool<Ingredient> GetIngredientPool()
    {
        return ingredientPool;
    }

    public void SetReleaseFlag(bool isReleased)
    {
        this.isReleased = isReleased;
    }

    public bool GetReleaseFlag()
    {
        return isReleased;
    }
}
