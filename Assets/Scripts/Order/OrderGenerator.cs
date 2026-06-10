using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class OrderGenerator
{
    private static List<IngredientType> possibleContent = new List<IngredientType>{
        IngredientType.Meat,
        IngredientType.Cheese,
        IngredientType.Lettuce,
        IngredientType.Ketchup
    };

    private static List<IngredientType> possibleVariables = new List<IngredientType>{
        IngredientType.Meat,
        IngredientType.Cheese,
        IngredientType.Lettuce
    };
    public static Dictionary<IngredientType, int> IngredientStock { get; set; } = new Dictionary<IngredientType, int>{
        {IngredientType.Meat, 5},
        {IngredientType.Cheese, 5},
        {IngredientType.Lettuce, 5},
        {IngredientType.Ketchup, 500}
    };
    public static List<ProgrammingBlock> GenerateProgrammingBlocksChefOrder(int level, int maxLines)
    {
        List<ProgrammingBlock> programmingBlocksOrder = new List<ProgrammingBlock>();
        programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, IngredientType.BreadBottom));
        programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, IngredientType.Meat));
        IngredientStock[IngredientType.Meat] -= 1;

        int currentLines = 3; // Contando el pan inferior y superior

        int ingredientCount = GetIngredientCountByLevel(level);
        // Put Ingredients
        IngredientType lastIngredient = IngredientType.Meat;
        for (int i = 0; i < ingredientCount && currentLines < maxLines; i++)
        {
            IngredientType randomIngredient = GetRandomAvailableIngredient(lastIngredient, 1);
            programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, randomIngredient));
            lastIngredient = randomIngredient;
            currentLines++;

        }

        // If Ingredients
        if (level == 2)
        {
            currentLines += 2;
            int ifIngredientCount = Random.Range(1, maxLines - currentLines);
            List<ProgrammingBlock> ifBlocks = new List<ProgrammingBlock>();
            for (int i = 0; i < ifIngredientCount; i++)
            {
                IngredientType randomIngredient = GetRandomAvailableIngredient(lastIngredient, 1);
                lastIngredient = randomIngredient;
                ifBlocks.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, randomIngredient));
                currentLines++;
            }
            IngredientType randomVariable = GetRandomVariable();
            ConditionalOperatorType randomOperator = GetRandomOperator();
            int randomValue = Random.Range(1, 3);
            programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.If, ifBlocks, randomVariable, randomOperator, randomValue));
        }
        // For Ingredients
        if (level == 3)
        {
            currentLines += 2;
            int forIngredientCount = Random.Range(1, maxLines - currentLines);
            List<ProgrammingBlock> forBlocks = new List<ProgrammingBlock>();
            int randomIterations = Random.Range(1, 3);
            for (int i = 0; i < forIngredientCount; i++)
            {
                IngredientType randomIngredient = GetRandomAvailableIngredient(lastIngredient, randomIterations);
                lastIngredient = randomIngredient;
                forBlocks.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, randomIngredient));
                currentLines++;
            }
            programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.For, forBlocks, randomIterations));
        }
        if (level >= 4)
        {
            currentLines += 4;
            int ifIngredientCount = Random.Range(1, maxLines - currentLines - 1);
            int forIngredientCount = maxLines - currentLines - ifIngredientCount;
            List<ProgrammingBlock> ifBlocks = new List<ProgrammingBlock>();
            for (int i = 0; i < ifIngredientCount; i++)
            {
                IngredientType randomIngredient = GetRandomAvailableIngredient(lastIngredient, 1);
                lastIngredient = randomIngredient;
                ifBlocks.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, randomIngredient));
                currentLines++;
            }
            IngredientType randomVariable = GetRandomVariable();
            ConditionalOperatorType randomOperator = GetRandomOperator();
            int randomValue = Random.Range(1, 3);
            programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.If, ifBlocks, randomVariable, randomOperator, randomValue));


            int randomIterations = Random.Range(1, 3);
            List<ProgrammingBlock> forBlocks = new List<ProgrammingBlock>();
            for (int i = 0; i < forIngredientCount; i++)
            {
                IngredientType randomIngredient = GetRandomAvailableIngredient(lastIngredient, randomIterations);
                lastIngredient = randomIngredient;
                forBlocks.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, randomIngredient));
                currentLines++;
            }
            programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.For, forBlocks, randomIterations));
        }

        programmingBlocksOrder.Add(ProgrammingBlockFactory.CreateBlock(ProgrammingBlockType.Put, IngredientType.BreadTop));

        return programmingBlocksOrder;
    }

    private static IngredientType GetRandomAvailableIngredient(IngredientType lastIngredient, int quantity = 1)
    {
        List<IngredientType> availableIngredients = new List<IngredientType>();
        foreach (var ingredient in possibleContent)
        {
            if (ingredient == IngredientType.Ketchup && lastIngredient == IngredientType.Ketchup)
                continue; // Salta el ketchup si el último ingrediente fue ketchup

            if (IngredientStock[ingredient] >= quantity)
            {
                availableIngredients.Add(ingredient);
            }
        }

        if (availableIngredients.Count > 0)
        {
            IngredientType randomIngredient = availableIngredients[Random.Range(0, availableIngredients.Count)];
            IngredientStock[randomIngredient] -= quantity;
            return randomIngredient;
        }
        return IngredientType.BreadBottom; // Retorna BreadBottom si no hay ingredientes disponibles
    }

    private static int GetIngredientCountByLevel(int level)
    {
        switch (level)
        {
            case 0: return Random.Range(1, 3);
            case 1: return Random.Range(2, 6);
            case 2: return Random.Range(1, 3);
            case 3: return Random.Range(1, 3);
            case 4: return Random.Range(0, 1);
            default: return Random.Range(0, 1);
        }
    }

    private static IngredientType GetRandomIngredientType()
    {
        return possibleContent[Random.Range(0, possibleContent.Count)];
    }

    private static IngredientType GetRandomVariable()
    {
        return possibleVariables[Random.Range(0, possibleVariables.Count)];
    }

    private static ConditionalOperatorType GetRandomOperator()
    {
        System.Array values = System.Enum.GetValues(typeof(ConditionalOperatorType));
        ConditionalOperatorType randomOperator = (ConditionalOperatorType)values.GetValue(Random.Range(0, values.Length));
        return randomOperator;
    }

    private static int GetRandomIterationsCount() {
        return Random.Range(2, 6);
    }

    public static List<IngredientType> GenerateIngredientListWaiterOrder(int level)
    {
        List<IngredientType> ingredientList = new List<IngredientType>();
        ingredientList.Add(IngredientType.BreadBottom);
        ingredientList.Add(IngredientType.Meat);
        int ingredientCount = GetIngredientCountByLevel(level);
        for (int i = 0; i < ingredientCount; i++)
        {
            IngredientType randomIngredient = GetRandomIngredientType();
            ingredientList.Add(randomIngredient);
        }
        if (level == 2) {
            IngredientType randomIngredient = GetRandomIngredientType();
            for (int i = 0; i < GetRandomIterationsCount(); i++)
            {
                ingredientList.Add(randomIngredient);
            }
        }
        if (level == 3) {
            IngredientType randomIngredient1 = GetRandomIngredientType();
            IngredientType randomIngredient2 = GetRandomIngredientType();
            for (int i = 0; i < (GetRandomIterationsCount() % 2) + 1 ; i++)
            {
                ingredientList.Add(randomIngredient1);
                ingredientList.Add(randomIngredient2);
            }
        }
        if (level == 4) {
            IngredientType randomIngredient1 = GetRandomIngredientType();
            IngredientType randomIngredient2 = GetRandomIngredientType();
            IngredientType randomIngredient3 = GetRandomIngredientType();
            for (int i = 0; i < (GetRandomIterationsCount() % 2) + 1 ; i++)
            {
                ingredientList.Add(randomIngredient1);
                ingredientList.Add(randomIngredient2);
                ingredientList.Add(randomIngredient3);
            }
        }
        ingredientList.Add(IngredientType.BreadTop);
        return ingredientList;
    }


}
