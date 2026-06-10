using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Ingredient : Supply
{
    public IngredientType IngredientType { get; protected set; }

    public bool fixVertical = false;
    [SerializeField] public NetworkVariable<Vector3> vecTransform = new NetworkVariable<Vector3>(new Vector3(0f, 0f, 0f),
                                            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Vector3 VecTransform
    {
        get
        {
            return vecTransform.Value;
        }
        set
        {
            vecTransform.Value = value;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (IsStaticObject && transform.rotation != Quaternion.Euler(0f, 0f, 0f))
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (fixVertical && transform.position != VecTransform)
        {
            transform.position = VecTransform;
            fixVertical = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public virtual (bool, float) PlaceOnPlate(float currentHeight, Transform plateTransform, bool checkSelected = true)
    {
        if (!IsServer || (checkSelected && grabInteractable.isSelected))
            return (false, 0.0f);

        currentHeight = plateTransform.gameObject.GetComponent<Plate>().GetCurrentHeight();

        NetworkObject plateNetObj = plateTransform.GetComponent<NetworkObject>();

        Vector3 newPosition = new Vector3(plateTransform.position.x,
                                                plateTransform.position.y + currentHeight + (ObjectHeight / 2),
                                                plateTransform.position.z);
        //ApplyPlacement(newPosition, plateNetObj);
        //SetWorldPlacementRpc(newPosition, plateTransform.GetComponent<NetworkObject>().NetworkObjectId);
        VecTransform = newPosition;
        ServerPlacementRpc(newPosition, plateTransform.GetComponent<NetworkObject>().NetworkObjectId);

        return (true, ObjectHeight);
    }

    private void ApplyPlacement(Vector3 newPosition, NetworkObject plateNetObj)
    {
        transform.position = newPosition;
        transform.rotation = Quaternion.identity;
        //transform.rotation = plateNetObj.transform.rotation;

        NetworkObject netObj = GetComponent<NetworkObject>();
        netObj.TrySetParent(plateNetObj);

        //GetComponent<NetworkTransform>().InLocalSpace = true;
        //transform.localPosition = new Vector3(0f, ObjectHeight / 100, 0f);
        //transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    [Rpc(SendTo.Everyone)]
    public void SetPlacedStateRpc()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        IsStaticObject = true;
        grabInteractable.enabled = false;

        //TransformY = transform.position.y;
        fixVertical = true;
    }

    [Rpc(SendTo.Everyone)]
    public void SetWorldPlacementRpc(Vector3 worldPos, ulong plateId)
    {
        GetComponent<NetworkTransform>().enabled = false;

        //if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(plateId, out var plateNetObj))
        //{
        //    transform.SetParent(plateNetObj.transform, true); // mantener WORLD
        //}
        transform.position = worldPos;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        //// estado final
        //Collider collider = GetComponent<Collider>();
        //if (collider != null)
        //    collider.enabled = false;

        //IsStaticObject = true;
        //grabInteractable.enabled = false;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ServerPlacementRpc(Vector3 newPosition, ulong plateId)
    {
        SetWorldPlacementRpc(newPosition, plateId);
    }
}
