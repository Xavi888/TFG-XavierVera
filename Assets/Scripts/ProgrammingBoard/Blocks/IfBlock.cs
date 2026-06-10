using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IfBlock : ProgrammingBlock
{
    public List<ProgrammingBlock> SuccessBlocks
    {
        get; set;
    }
    [SerializeField] private TMP_Dropdown variableDropdown;
    [SerializeField] private TMP_Dropdown operatorDropdown;
    [SerializeField] private TMP_Dropdown valueDropdown;
    public bool isTrue;

    private List<int> valueOptions = new List<int>() { 1, 2, 3, 4, 5 };

    public IngredientType Variable { get; set; }

    public ConditionalOperatorType Operator { get; set; }

    public int Value { get; set; }

    public IfBlock()
    {
        BlockType = ProgrammingBlockType.If;
        SuccessBlocks = new List<ProgrammingBlock>();
    }
    protected override void Start()
    {
        base.Start();
        InitBlock();
    }

    public void InitBlock()
    {
        variableDropdown.ClearOptions();
        variableDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(IngredientType))));
        variableDropdown.SetValueWithoutNotify((int)Variable);

        operatorDropdown.ClearOptions();
        operatorDropdown.AddOptions(GetOperatorSymbols());
        operatorDropdown.SetValueWithoutNotify((int)Operator);

        valueDropdown.ClearOptions();
        valueDropdown.AddOptions(valueOptions.ConvertAll<string>(x => x.ToString()));
        valueDropdown.SetValueWithoutNotify(valueOptions.IndexOf(Value));

        variableDropdown.onValueChanged.AddListener(delegate { VariableDropdownChanged(); });
        operatorDropdown.onValueChanged.AddListener(delegate { OperatorDropdownChanged(); });
        valueDropdown.onValueChanged.AddListener(delegate { ValueDropdownChanged(); });

    }

    private void VariableDropdownChanged()
    {
        Variable = (IngredientType)variableDropdown.value;
        Debug.Log(Variable + " " + Operator + " " + Value);
    }

    private void OperatorDropdownChanged()
    {
        Operator = (ConditionalOperatorType)operatorDropdown.value;
        Debug.Log(Variable + " " + Operator + " " + Value);
    }

    private void ValueDropdownChanged()
    {
        Value = valueOptions[valueDropdown.value];
        Debug.Log(Variable + " " + Operator + " " + Value);
    }

    private List<string> GetOperatorSymbols()
    {
        var symbols = new List<string>();
        foreach (var op in Enum.GetValues(typeof(ConditionalOperatorType)))
        {
            switch (op)
            {
                case ConditionalOperatorType.GreaterThan:
                    symbols.Add(">");
                    break;
                case ConditionalOperatorType.LessThan:
                    symbols.Add("<");
                    break;
                case ConditionalOperatorType.GreaterThanOrEqual:
                    symbols.Add(">=");
                    break;
                case ConditionalOperatorType.LessThanOrEqual:
                    symbols.Add("<=");
                    break;
                default:
                    symbols.Add(op.ToString());
                    break;
            }
        }
        return symbols;
    }
    public void Evaluate(Supplier supplier) {
        switch (Operator) {
            case ConditionalOperatorType.GreaterThan:
                isTrue = supplier.SupplyAmount > Value;
                break;
            case ConditionalOperatorType.LessThan:
                isTrue = supplier.SupplyAmount < Value;
                break;
            case ConditionalOperatorType.GreaterThanOrEqual:
                isTrue = supplier.SupplyAmount >= Value;
                break;
            case ConditionalOperatorType.LessThanOrEqual:
                isTrue = supplier.SupplyAmount <= Value;
                break;
        }
    }
    public override List<IngredientType> Execute()
    {
        List<IngredientType> ingredients = new List<IngredientType>();
        if (isTrue)
        {
            foreach (var block in SuccessBlocks)
            {
                ingredients.AddRange(block.Execute());
            }
        }
        return ingredients;
    }
}
