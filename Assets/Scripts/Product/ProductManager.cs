using UnityEngine;

public class ProductManager : MonoBehaviour
{
    //Number of ingredients this product consists.
    //must be between 1 to 6 item.
    // [Range(1, 6)]
    // public int totalIngredients;

    //In the inspector, use the number above "totalIngredients" as the lentgh of this array.
    //then assign proper ID of desired ingredients to array's childs.
    //note that IDs index should be carefully selected from existing ingrediets. in this kit we have 21 ingredients,
    //so we can choose any index from 0 to 20.
    //we also can use duplicate indexs. meaning a product can consist of two or more of the same ingredient.
    public int[] ingredientsCodes;
    
    //for example
    // a custom product definition is like this:
    /*
	totalIngredients = 6;
	ingredientsIDs[0] = 0;
	ingredientsIDs[1] = 4;
	ingredientsIDs[2] = 7;
	ingredientsIDs[3] = 12;
	ingredientsIDs[4] = 13;
	ingredientsIDs[5] = 18;
	*/

    //Another example
    /*
	totalIngredients = 3;
	ingredientsIDs[0] = 1;
	ingredientsIDs[1] = 2;
	ingredientsIDs[2] = 15;
	*/
}
