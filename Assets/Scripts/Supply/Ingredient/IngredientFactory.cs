using System.Collections.Generic;
using UnityEngine;
public class IngredientFactory : MonoBehaviour
{
    public static Ingredient CreateIngredient(IngredientType ingredientType)
    {
        switch (ingredientType) {
            case IngredientType.Meat:
                Meat meat = Resources.Load<Meat>("Prefabs/Supply/P_Meat");
                Meat meatGameObject = Instantiate(meat);
                meatGameObject.Cooked = true;
                meatGameObject.supplyRenderer.enabled = true;
                return meatGameObject;
            case IngredientType.Cheese:
                Cheese cheese = Resources.Load<Cheese>("Prefabs/Supply/P_Cheese");
                Cheese cheeseGameObject = Instantiate(cheese);
                cheeseGameObject.supplyRenderer.enabled = true;
                return cheeseGameObject;
            case IngredientType.Ketchup:
                Ketchup ketchup = Resources.Load<Ketchup>("Prefabs/Supply/P_Ketchup");
                Ketchup ketchupGameObject = Instantiate(ketchup);
                ketchupGameObject.supplyRenderer.enabled = true;
                return ketchupGameObject;
            case IngredientType.Lettuce:
                Lettuce lettuce = Resources.Load<Lettuce>("Prefabs/Supply/P_Lettuce");
                Lettuce lettuceGameObject = Instantiate(lettuce);
                lettuceGameObject.supplyRenderer.enabled = true;
                return lettuceGameObject;
            case IngredientType.BreadBottom:
                BreadBottom breadBottom = Resources.Load<BreadBottom>("Prefabs/Supply/P_BreadBottom");
                BreadBottom breadBottomGameObject = Instantiate(breadBottom);
                breadBottomGameObject.supplyRenderer.enabled = true;
                return breadBottomGameObject;
            case IngredientType.BreadTop:
                BreadTop breadTop = Resources.Load<BreadTop>("Prefabs/Supply/P_BreadTop");
                BreadTop breadTopGameObject = Instantiate(breadTop);
                breadTopGameObject.supplyRenderer.enabled = true;
                return breadTopGameObject;
            default:
                return null;
        }
    }
}

