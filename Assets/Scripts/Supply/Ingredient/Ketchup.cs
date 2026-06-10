using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ketchup : Ingredient
{
    protected override void Start() {
        base.Start();
        IngredientType = IngredientType.Ketchup;
    }

    protected override void Update() {
        base.Update();
    }
}
