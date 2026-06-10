using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadTop : Ingredient
{
    protected override void Start() {
        base.Start();
        IngredientType = IngredientType.BreadTop;
    }

    protected override void Update() {
        base.Update();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        string supplierName = "P_BreadTopSupplier";
        GameObject og = GameObject.Find(supplierName);
        if (og != null)
            origin = og.GetComponent<Supplier>();
    }
}
