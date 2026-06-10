using UnityEngine;
using Unity.Netcode;

public class ButtonDeliver : NetworkBehaviour
{
    [SerializeField] private GameObject tray;
    [SerializeField] private GameObject deliveryTable;

    public void OnClick()
    {
        if (tray == null)
            return;

        if (IsServer)
        {
            DeliveryPlate();
        }
        else
        {
            DeliveryPlateClientRpc();
        }
    }

    private void DeliveryPlate()
    {
        GameObject plate = tray.GetComponent<Tray>().GetPlate();
        if (plate != null)
        {
            deliveryTable.GetComponent<DeliveryTable>().DeliverPlate(plate.GetComponent<Plate>());
            tray.GetComponent<Tray>().PlateNetId = 0;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DeliveryPlateClientRpc()
    {
        DeliveryPlate();
    }
}
