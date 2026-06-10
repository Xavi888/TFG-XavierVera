using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class Plate : Supply
{
    public List<Ingredient> IngredientList
    {
        get; private set;
    }
    [SerializeField] private float currentHeight;

    private bool needFix = false;
    private bool isFixed = false;
    [SerializeField] public NetworkVariable<bool> isOnTray = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool IsOnTray
    {
        get
        {
            return isOnTray.Value;
        }
        set
        {
            isOnTray.Value = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        IngredientList = new List<Ingredient>();
    }

    protected override void Start()
    {
        base.Start();
        currentHeight = ObjectHeight / 2;
    }

    protected override void Update()
    {
        base.Update();

        if (needFix) // Arreglo para evitar mover el plato con otros objetos
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            needFix = false;
            isFixed = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        string supplierName = "P_PlateSupplier";
        GameObject og = GameObject.Find(supplierName);
        if (og != null)
            origin = og.GetComponent<Supplier>();
    }

    public void BuildOnPlate(List<Ingredient> ingredients) // Pertenece al juego del camarero y no se toca para el multiplayer por el momento
    {
        foreach (Ingredient ingredient in ingredients)
        {
            (bool placed, float otherHeight) = ingredient.PlaceOnPlate(currentHeight, transform, false);
            if (placed)
            {
                Debug.Log(IngredientList);
                IngredientList.Add(ingredient);
                currentHeight += otherHeight;
            }
        }
    }

    private void OnCollisionEnter(Collision otherCollision)
    {
        if (!IsOnTray) // Evita colocar ingredientes si el plato no está en la bandeja
            return;

        GameObject other = otherCollision.collider.gameObject;
        if (other.CompareTag("Ingredient"))
        {
            if (!isFixed)
                needFix = true;

            if (!IsStaticObject && !grabInteractable.isSelected)
            {
                Ingredient ingredient = other.GetComponent<Ingredient>();
                
                //El grabInteractable del KetchupBottle no se detecta cuando es el otro usuario el que lo coge. Se usa Net Variable para unificarlo.
                if (ingredient.IngredientType == IngredientType.Ketchup && ingredient.grabInteractable.isSelected)
                    ingredient.GetComponent<KetchupBottle>().IsSelectedOnCollision = true;

                if (IsServer)
                {
                    TryPlaceIngredient(ingredient);
                }
                else
                {
                    ulong id = ingredient.GetComponent<NetworkObject>().NetworkObjectId;
                    RequestPlaceIngredientRpc(id);
                }
            }
        }
    }

    public void TryPlaceIngredient(Ingredient ingredient)
    {
        (bool placed, float otherHeight) = ingredient.PlaceOnPlate(currentHeight, transform);
        if (placed)
        {
            if (ingredient.IngredientType != IngredientType.Ketchup)
            {
                IngredientList.Add(ingredient);
                ingredient.SetPlacedStateRpc();
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPlaceIngredientRpc(ulong ingredientId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ingredientId, out NetworkObject netObj))
            return;

        Ingredient ingredient = netObj.GetComponent<Ingredient>();

        if (ingredient == null)
            return;

        TryPlaceIngredient(ingredient);
    }

    public float GetCurrentHeight()
    {
        float h = ObjectHeight / 2f;

        foreach (var ing in IngredientList)
        {
            h += ing.ObjectHeight;
        }

        return h;
    }

    [Rpc(SendTo.Everyone)]
    public void DisableGrabRpc()
    {
        if (grabInteractable != null)
            grabInteractable.enabled = false;
    }

    public void ClearPlate()
    {
        if (!IsServer)
            return;

        foreach (Ingredient item in IngredientList)
        {
            if (item != null)
            {
                if (item.gameObject.TryGetComponent(out NetworkObject netObj))
                {
                    if (netObj.IsSpawned)
                        netObj.Despawn();
                }
            }
        }
        IngredientList.Clear();
    }
}
