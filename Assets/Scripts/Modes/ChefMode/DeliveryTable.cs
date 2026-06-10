using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class DeliveryTable : MonoBehaviour
{
    private PlateDeliveredEvent plateDelivered = new PlateDeliveredEvent();

    private void Start() {
        plateDelivered.AddListener(GameObject.FindGameObjectWithTag("GameController").GetComponent<ChefGameController>().PlateDelivered);
    }
    //private void OnTriggerEnter(Collider other) {
    //    if (!NetworkManager.Singleton.IsServer)
    //        return;

    //    if (other.CompareTag("Plate")) {
            
    //        Plate plate = other.GetComponent<Plate>();
    //        DeliverPlate(plate);
    //        //plateDelivered.Invoke(plate.IngredientList);

    //        //foreach (Ingredient ingredient in plate.IngredientList)
    //        //{
    //        //    if (ingredient != null)
    //        //    {
    //        //        NetworkObject netObj = ingredient.GetComponent<NetworkObject>();

    //        //        if (netObj != null && netObj.IsSpawned)
    //        //            netObj.Despawn(true);
    //        //    }
    //        //}

    //        //NetworkObject plateNetObj = plate.GetComponent<NetworkObject>();
    //        //if (plateNetObj != null && plateNetObj.IsSpawned)
    //        //    plateNetObj.Despawn(true);
    //    }
    //}

    public void DeliverPlate(Plate plate)
    {
        plateDelivered.Invoke(plate.IngredientList);

        foreach (Ingredient ingredient in plate.IngredientList)
        {
            if (ingredient != null)
            {
                NetworkObject netObj = ingredient.GetComponent<NetworkObject>();

                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
            }
        }

        NetworkObject plateNetObj = plate.GetComponent<NetworkObject>();
        if (plateNetObj != null && plateNetObj.IsSpawned)
            plateNetObj.Despawn(true);
    }
}
[Serializable]
public class PlateDeliveredEvent : UnityEvent<List<Ingredient>> {}
