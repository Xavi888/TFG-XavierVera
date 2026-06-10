using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lettuce : Ingredient
{
    protected override void Start() {
        base.Start();
        IngredientType = IngredientType.Lettuce;
    }

    protected override void Update() {
        base.Update();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        string supplierName = "P_LettuceSupplier";
        GameObject og = GameObject.Find(supplierName);
        if (og != null)
            origin = og.GetComponent<Supplier>();
    }
}
