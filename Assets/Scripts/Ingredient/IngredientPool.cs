using UnityEngine;
using UnityEngine.Pool;

public class IngredientPool : MonoBehaviour
{
    public ObjectPool<Ingredient> ingredientPool;

    [SerializeField] private Ingredient ingredientPrefab;
    // [SerializeField] private GameObject gameControlObj;

    private MainGameController mainGameController;
    private Ingredient lastIngredient;
    private Transform parent;

    // private void Awake()
    // {
    //     mainGameController = gameControlObj.GetComponent<MainGameController>();
    // }

    private void Start()
    {
        lastIngredient = null;

        mainGameController = GameObject.FindGameObjectWithTag("MainGameController").GetComponent<MainGameController>();

        ingredientPool = new ObjectPool<Ingredient>(CreateIngredient, OnTakeIngredientFromPool, OnReturnIngredientToPool, OnDestroyIngredient, true, mainGameController.maxOrderHeight, mainGameController.maxOrderHeight);
    }

    private Ingredient CreateIngredient()
    {
        Ingredient ingredient = Instantiate(ingredientPrefab, parent);

        // set the ingredient's pool
        ingredient.SetIngredientPool(ingredientPool);

        return ingredient;
    }

    private void OnTakeIngredientFromPool(Ingredient ingredient)
    {
        lastIngredient = ingredient;

        ingredient.SetReleaseFlag(false);

        ingredient.gameObject.SetActive(true);
    }

    private void OnReturnIngredientToPool(Ingredient ingredient)
    {
        lastIngredient = null;

        // set back to normal
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.IngredientCode = 0;
        ingredient.IngredientType = IngredientType.none;
        ingredient.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
        ingredient.transform.GetChild(1).localPosition = Vector3.zero;

        ingredient.gameObject.SetActive(false);
    }

    private void OnDestroyIngredient(Ingredient ingredient)
    {
        Destroy(ingredient.gameObject);
    }

    public Ingredient GetLastIngredient()
    {
        return lastIngredient;
    }

    public void SetParent(Transform parent)
    {
        this.parent = parent;
    }
}
