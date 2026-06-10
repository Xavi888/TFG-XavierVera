using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ForBlock : ProgrammingBlock
{
    [SerializeField] private TMP_Dropdown iterationsDropdown;
    public List<ProgrammingBlock> IterationBlocks
    {
        get; set;
    }
    public int Iterations
    {
        get; set;
    }

    private List<int> valueOptions = new List<int>() { 1, 2, 3, 4, 5 };

    public ForBlock()
    {
        BlockType = ProgrammingBlockType.For;
        IterationBlocks = new List<ProgrammingBlock>();
    }

    protected override void Start() {
        base.Start();
        InitBlock();
    }

    public void InitBlock()
    {
        iterationsDropdown.ClearOptions();
        iterationsDropdown.AddOptions(valueOptions.ConvertAll<string>(x => x.ToString()));
        iterationsDropdown.SetValueWithoutNotify(valueOptions.IndexOf(Iterations));

        iterationsDropdown.onValueChanged.AddListener(delegate { IterationsDropdownChanged(); });
    }

    private void IterationsDropdownChanged()
    {
        Iterations = valueOptions[iterationsDropdown.value];
    }
    public override List<IngredientType> Execute()
    {
        List<IngredientType> ingredients = new List<IngredientType>();
        for (int i = 0; i < Iterations; i++)
        {
            foreach (var block in IterationBlocks)
            {
                ingredients.AddRange(block.Execute());
            }
        }
        return ingredients;
    }
}
