using System.Collections.Generic;
using System.Linq;

public static class FeedbackGenerator
{
    public static List<FeedbackType> CheckPlateOrder(List<Ingredient> plateIngredients, List<ProgrammingBlock> orderBlocks)
    {
        List<FeedbackType> feedbackTypes = new List<FeedbackType>();
        List<IngredientType> orderIngredientTypes = OrderUtils.GetIngredientTypesFromBlocks(orderBlocks);
        List<IngredientType> plateIngredientTypes = OrderUtils.GetIngredientTypesFromPlate(plateIngredients);
        if (!CheckMeat(plateIngredients))
        {
            feedbackTypes.Add(FeedbackType.RawMeat);
        }
        if (orderIngredientTypes.Count > plateIngredientTypes.Count)
        {
            feedbackTypes.Add(FeedbackType.InsufficientIngredients);
        }
        if (orderIngredientTypes.Count < plateIngredientTypes.Count)
        {
            feedbackTypes.Add(FeedbackType.ExtraIngredients);
        }
        if (orderIngredientTypes.SequenceEqual(plateIngredientTypes))
        {
            feedbackTypes.Add(FeedbackType.Correct);
        }
        else
        {
            feedbackTypes.Add(FeedbackType.Incorrect);
        }
        return feedbackTypes;
    }

    public static List<FeedbackType> CheckProgrammedOrder() {
        return null;
    }

    private static bool CheckMeat(List<Ingredient> plateIngredients)
    {
        foreach (Ingredient ingredient in plateIngredients)
        {
            if (ingredient.IngredientType == IngredientType.Meat)
            {
                return ((Meat)ingredient).Cooked;
            }
        }
        return true;
    }

    


}
