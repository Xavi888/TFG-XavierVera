using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Tray : NetworkBehaviour
{
    private NetworkVariable<ulong> plateNetId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public ulong PlateNetId
    {
        get
        {
            return plateNetId.Value;
        }
        set
        {
            plateNetId.Value = value;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PlateNetId != 0 || !collision.gameObject.CompareTag("Plate"))
            return;

        Plate plateObj = collision.gameObject.GetComponent<Plate>();
        if (plateObj.IsStaticObject || plateObj.grabInteractable.isSelected)
            return;

        PlacePlate(collision.gameObject);

        plateObj.DisableGrabRpc();

        ulong netObjId = collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        if (IsServer) 
        {
            SetPlateId(netObjId);
        }
        else
        {
            SetPlateIdRpc(netObjId);
        }
    }

    private void PlacePlate(GameObject plateGO)
    {
        Rigidbody rb = plateGO.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        plateGO.transform.position = transform.position + Vector3.up * 0.1f; // ajusta altura
        plateGO.transform.rotation = Quaternion.identity;
    }

    public GameObject GetPlate()
    {
        if (plateNetId.Value == 0)
            return null;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(plateNetId.Value, out NetworkObject netObj))
            return netObj.gameObject;

        return null;
    }

    private void SetPlateId(ulong netObjId)
    {
        plateNetId.Value = netObjId;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject netObj))
            netObj.GetComponent<Plate>().IsOnTray = true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlateIdRpc(ulong netObjId)
    {
        SetPlateId(netObjId);
    }
}
