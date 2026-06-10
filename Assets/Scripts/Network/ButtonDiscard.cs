using UnityEngine;
using Unity.Netcode;

public class ButtonDiscard : NetworkBehaviour
{
    [SerializeField] private GameObject tray;

    public void OnClick()
    {
        if (tray == null)
            return;

        if (IsServer)
        {
            DiscardPlate();
        }
        else
        {
            DiscardPlateClientRpc();
        }
    }

    private void DiscardPlate()
    {
        GameObject plate = tray.GetComponent<Tray>().GetPlate();
        if (plate != null)
        {
            plate.GetComponent<Plate>().ClearPlate();
            if (plate.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
            tray.GetComponent<Tray>().PlateNetId = 0;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DiscardPlateClientRpc()
    {
        DiscardPlate();
    }
}
