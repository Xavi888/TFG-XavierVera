using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Supplier : NetworkBehaviour
{
    [SerializeField] private int supplyAmount = 5;
    public int SupplyAmount {
        get {
            return supplyAmount;
        }
        set {
            supplyAmount = value;
        }
    }
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private float supplyPositionOffset = 0.05f;
    private NetworkVariable<bool> hasSupply = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject currentSupply;
    public List<GameObject> SuppliedItems { get; set; } = new List<GameObject>(); 

    public bool HasSupply {
        get
        {
            return hasSupply.Value;
        }
        set
        {
            hasSupply.Value = value;
        }
    }

    private Vector3 supplyPosition;
    

    private void Start() {

        supplyPosition = new Vector3(transform.position.x, transform.position.y + supplyPositionOffset, transform.position.z);
    }

    private void Update() {

        if (!IsServer)
        {
            if (textField.text == "?")
                // Solo pasa por aquí la primera vez que spawnea un cliente. Se controla que aparezcan los 2 antes de empezar a jugar
                UpdateDisplay(SupplyAmount);

            return;
        }

        CheckSupply();
    }

    private void CheckSupply() {
        if (!hasSupply.Value && SupplyAmount > 0) {

            SpawnSupply();

            hasSupply.Value = true;
            SupplyAmount--;
            UpdateDisplayClientRpc(SupplyAmount + 1);
        } else if (!hasSupply.Value && SupplyAmount == 0) {
            SupplyAmount--;
            UpdateDisplayClientRpc(0);
        }
    }

    public void ResetSupply() {
        if (!IsServer)
            return;

        if (HasSupply) {
            if (currentSupply.TryGetComponent(out NetworkObject netObj))
            {
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
        }
        SupplyAmount = 5;
        HasSupply = false;
    }

    public void ClearSupplieds() {
        if (!IsServer)
            return;

        foreach (GameObject item in SuppliedItems) {
            if (item != null) {
                if (item.TryGetComponent(out NetworkObject netObj))
                {
                    if (netObj.IsSpawned)
                        netObj.Despawn();
                }
            }
        }
        SuppliedItems.Clear(); 
    }

    private void UpdateDisplay(int quantity) {
        textField.text = quantity.ToString(); 
    }

    private void SpawnSupply()
    {
        currentSupply = Instantiate(supplyPrefab, supplyPosition, Quaternion.identity);
        NetworkObject netObj = currentSupply.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        //netObj.TrySetParent(GetComponent<NetworkObject>());
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void UpdateDisplayClientRpc(int quantity)
    {
        UpdateDisplay(quantity);
    }
}
