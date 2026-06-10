using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit;

public class PutBlock : ProgrammingBlock
{   
    
    [SerializeField] private TMP_Dropdown typeDropdown;
    private IngredientType type;
    public IngredientType Type {
        get { 
            return type; 
            }
        set { 
            type = value;
            }
    }

    public PutBlock() {
        BlockType = ProgrammingBlockType.Put;
    }

    protected override void Start() {
        base.Start();
        InitBlock();
        typeDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(typeDropdown); });
    }

    private void Update() {
    }
    public void InitBlock(){
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(IngredientType))));
        typeDropdown.SetValueWithoutNotify((int) Type);
    }
    public void SetInteractuable(bool interactuable){
        typeDropdown.interactable = interactuable;
    }
    private void DropdownValueChanged(TMP_Dropdown dropdown) {
        Type = (IngredientType)dropdown.value;
        Debug.Log(Type);
    }

    public override List<IngredientType> Execute()
    {
        return new List<IngredientType>() {Type};
    }
}
