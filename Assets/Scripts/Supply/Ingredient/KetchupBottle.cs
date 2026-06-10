using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class KetchupBottle : Ingredient
{
    [SerializeField] private GameObject ketchupPrefab;
    [SerializeField] private float cooldownTime = 2.0f; 
    private bool isOnCooldown = false;
    private float cooldownTimer = 0.0f;
    public NetworkVariable<bool> isSelectedOnCollision = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool IsSelectedOnCollision
    {
        get
        {
            return isSelectedOnCollision.Value;
        }
        set
        {
            if (IsServer)
            {
                isSelectedOnCollision.Value = value;
            }
            else
            {
                SetSelectedOnCollisionRpc(value);
            }
        }
    }

    protected override void Awake(){
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        ObjectHeight = GetComponent<Collider>().bounds.size.y;
    }
    protected override void Start()
    {
        base.Start();
        IngredientType = IngredientType.Ketchup;
    }

    protected override void Update()
    {
        base.Update();
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
            }
        }
    }

    public override (bool, float) PlaceOnPlate(float currentHeight, Transform plateTransform, bool checkSelected = true)
    {
        if (!IsServer)
            return (false, 0.0f);
        
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(GetComponent<NetworkObject>().NetworkObjectId, out NetworkObject netIngr);

        if (checkSelected && IsSelectedOnCollision && !isOnCooldown)
        {
            IsSelectedOnCollision = false;
            currentHeight = plateTransform.gameObject.GetComponent<Plate>().GetCurrentHeight();

            NetworkObject plateNetObj = plateTransform.GetComponent<NetworkObject>();
            
            GameObject ketchupGameObject = Instantiate(ketchupPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            //ketchupGameObject.GetComponent<NetworkTransform>().enabled = false;
            NetworkObject netObj = ketchupGameObject.GetComponent<NetworkObject>();
            netObj.Spawn(true);
            
            Ingredient ketchupIngredient = ketchupGameObject.GetComponent<Ingredient>();
            Vector3 newPosition = new Vector3(plateTransform.position.x,
                                              plateTransform.position.y + currentHeight + (ketchupIngredient.ObjectHeight / 2),
                                              plateTransform.position.z);

            ketchupIngredient.VecTransform = newPosition;
            ketchupIngredient.ShowRendererRpc();
            ketchupIngredient.ServerPlacementRpc(newPosition, plateNetObj.NetworkObjectId);
            ketchupIngredient.SetPlacedStateRpc();

            plateTransform.GetComponent<Plate>().IngredientList.Add(ketchupIngredient);
            plateTransform.GetComponent<Supply>().origin.SuppliedItems.Add(ketchupGameObject);

            StartCooldown();

            return (true, ketchupIngredient.ObjectHeight);
        }

        return (false, 0.0f);
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetSelectedOnCollisionRpc(bool value)
    {
        isSelectedOnCollision.Value = value;
    }
}
