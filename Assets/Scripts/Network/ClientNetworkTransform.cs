using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ClientNetworkTransform : NetworkTransform
{
    private NetworkObject netObj;
    private XRGrabInteractable grabInteractable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        netObj = GetComponent<NetworkObject>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Detectar cuando se agarra el objeto
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (IsOwner)
            return;

        if (IsServer)
        {
            netObj.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            RequestOwnershipRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipRpc(RpcParams rpcParams = default)
    {
        // ESTE código ya corre en el servidor
        if (!IsServer)
            return;

        ulong senderId =
            rpcParams.Receive.SenderClientId;

        if (netObj.OwnerClientId != senderId)
            netObj.ChangeOwnership(senderId);
    }
}