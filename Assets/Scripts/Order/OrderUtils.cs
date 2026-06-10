using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class OrderUtils
{
    public static List<IngredientType> GetIngredientTypesFromBlocks(List<ProgrammingBlock> blocks)
    {
        List<IngredientType> ingredientsTypes = new List<IngredientType>();
        foreach (ProgrammingBlock block in blocks)
        {
            ingredientsTypes.AddRange(block.Execute());
        }
        return ingredientsTypes;
    }

    public static List<IngredientType> GetIngredientTypesFromPlate(List<Ingredient> plateIngredients)
    {
        List<IngredientType> ingredientsTypes = new List<IngredientType>();
        foreach (Ingredient ingredient in plateIngredients)
        {
            ingredientsTypes.Add(ingredient.IngredientType);
        }
        return ingredientsTypes;
    }

    public static string GetOrderDescription(List<IngredientType> order)
    {
        StringBuilder orderDescription = new StringBuilder("Quiero una hamburguesa con ");
        List<string> ingredientDescriptions = new List<string>();
        int count = 1;

        for (int i = 0; i < order.Count; i++)
        {
            if (i < order.Count - 1 && order[i] == order[i + 1])
            {
                count++;
            }
            else
            {
                string ingredientName = GetIngredientName(order[i]);
                if (order[i] != IngredientType.BreadTop && order[i] != IngredientType.BreadBottom)
                {
                    if (count > 1)
                    {
                        ingredientDescriptions.Add(count + " " + ingredientName);
                    }
                    else
                    {
                        ingredientDescriptions.Add(ingredientName);
                    }
                    count = 1; // Reset count
                }
            }
        }

        if (ingredientDescriptions.Count > 0)
        {
            // Añadir "y" antes del último ingrediente si hay más de uno
            if (ingredientDescriptions.Count > 1)
            {
                string lastIngredient = ingredientDescriptions[ingredientDescriptions.Count - 1];
                ingredientDescriptions.RemoveAt(ingredientDescriptions.Count - 1);
                orderDescription.Append(string.Join(", ", ingredientDescriptions));
                orderDescription.Append(" y " + lastIngredient);
            }
            else
            {
                orderDescription.Append(ingredientDescriptions[0]);
            }
        }
        else
        {
            orderDescription.Append("nada especial");
        }

        return orderDescription.ToString();
    }

    private static string GetIngredientName(IngredientType ingredientType)
    {
        Dictionary<IngredientType, string> ingredientNames = new Dictionary<IngredientType, string>
    {
        { IngredientType.Meat, "carne" },
        { IngredientType.Lettuce, "lechuga" },
        { IngredientType.Cheese, "queso" },
        { IngredientType.Ketchup, "ketchup" },
    };

        if (ingredientNames.TryGetValue(ingredientType, out string name))
        {
            return name;
        }
        else
        {
            return "Ingrediente desconocido";
        }
    }
}
