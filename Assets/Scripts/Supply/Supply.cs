using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class Supply : NetworkBehaviour
{
    protected bool firstTimeGrabbed = true;
    
    public Supplier origin;
    protected Rigidbody rb;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public Renderer supplyRenderer;

    public float ObjectHeight {
        get; protected set;
    }

    public bool IsStaticObject {
        get; set;
    }

    protected virtual void Awake() {
        supplyRenderer = GetComponent<Renderer>();
        supplyRenderer.enabled = false;
        rb = GetComponent<Rigidbody>();
        
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(selectEnteredHandler);
        grabInteractable.selectExited.AddListener(selectExitedHandler);
        rb.isKinematic = true;
        IsStaticObject = true;
        ObjectHeight = GetComponent<Collider>().bounds.size.y;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    //public override void OnNetworkDespawn()
    //{
    //    base.OnNetworkDespawn();

    //    if (grabInteractable != null)
    //    {
    //        grabInteractable.selectEntered
    //            .RemoveListener(selectEnteredHandler);

    //        grabInteractable.selectExited
    //            .RemoveListener(selectExitedHandler);
    //    }
    //}

    protected virtual void Start() {
        
    }
    protected virtual void Update() {
        if (!IsStaticObject && rb.isKinematic) {
            rb.isKinematic = false;
        }
        if  (IsStaticObject && !rb.isKinematic) {
            rb.isKinematic = true;
        }
    }
    private void selectEnteredHandler(SelectEnterEventArgs selectEnterEventArgs) {
        if (!firstTimeGrabbed)
            return;
        
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        IsStaticObject = false;
        firstTimeGrabbed = false;
        ShowRendererRpc();
        if (IsServer)
        {
            HandleFirstGrab();
        }
        else
        {
            NotifySupplierGrabbedRpc();
        }
    }

    private void selectExitedHandler(SelectExitEventArgs selectExitEventArgs) {
    }

    private void HandleFirstGrab()
    {
        if (origin != null)
        {
            origin.HasSupply = false;
            origin.SuppliedItems.Add(gameObject);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifySupplierGrabbedRpc(RpcParams rpcParams = default)
    {
        if (!IsServer)
            return;

        HandleFirstGrab();
    }

    [Rpc(SendTo.Everyone)]
    public void ShowRendererRpc()
    {
        if (supplyRenderer != null)
            supplyRenderer.enabled = true;
    }
}
