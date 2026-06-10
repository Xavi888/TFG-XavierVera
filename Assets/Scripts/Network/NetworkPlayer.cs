using UnityEngine;
using Unity.Netcode;
using Unity.XR.CoreUtils;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Renderer[] disableList;

    private Vector3 chefPosition = new Vector3(-0.9f, 0.2f, 0.474f); // Se posiciona al cocinero en el centro de la mesa
    private Vector3 advisorPosition = new Vector3(-2.3f, 0.2f, 1.5f); // Se posiciona al ayudante detrás de la plancha

    private Vector3 chefOnePosition = new Vector3(-0.9f, 0.2f, 0.75f); // Se posiciona al cocinero 1 en el lado izquierdo de la mesa
    private Vector3 chefTwoPosition = new Vector3(-0.9f, 0.2f, 0.2f); // Se posiciona al cocinero 2 en el lado derecho de la mesa

    public enum NetworkPlayerRole : byte
    {
        None,
        PairProgrammingChef,
        PairProgrammingAdvisor,
        VersionControlChefOne,
        VersionControlChefTwo
    }
    private NetworkVariable<NetworkPlayerRole> playerRole = new NetworkVariable<NetworkPlayerRole>(
        NetworkPlayerRole.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerRole.OnValueChanged += OnRoleChanged;

        if (IsOwner)
        {
            foreach (var item in disableList)
            {
                item.enabled = false;
            }

            ApplyLocalRole(playerRole.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerRole.OnValueChanged -= OnRoleChanged;
    }

    private void OnRoleChanged(NetworkPlayerRole previousRole, NetworkPlayerRole newRole)
    {
        if (!IsOwner)
            return;

        ApplyLocalRole(newRole);
    }

    public void SetRoleServer(NetworkPlayerRole role)
    {
        if (!IsServer)
            return;

        playerRole.Value = role;
    }

    private void ApplyLocalRole(NetworkPlayerRole role)
    {
        switch (role)
        {
            case NetworkPlayerRole.PairProgrammingChef:
                MoveLocalPlayerToPosition(chefPosition);
                SetChefControlsEnabled(true);
                Debug.Log("Soy CHEF en PairProgramming");
                break;

            case NetworkPlayerRole.PairProgrammingAdvisor:
                MoveLocalPlayerToPosition(advisorPosition);
                SetChefControlsEnabled(false);
                Debug.Log("Soy AYUDANTE en PairProgramming");
                break;

            case NetworkPlayerRole.VersionControlChefOne:
                MoveLocalPlayerToPosition(chefOnePosition);
                SetChefControlsEnabled(true);
                Debug.Log("Soy COCINERO 1 en VersionControl");
                break;

            case NetworkPlayerRole.VersionControlChefTwo:
                MoveLocalPlayerToPosition(chefTwoPosition);
                SetChefControlsEnabled(true);
                Debug.Log("Soy COCINERO 2 en VersionControl");
                break;
        }
    }

    private void MoveLocalPlayerToPosition(Vector3 targetPosition)
    {
        if (VRRigReferences.Singleton == null || VRRigReferences.Singleton.root == null)
            return;

        Transform localRoot = VRRigReferences.Singleton.root;

        localRoot.position = targetPosition;
    }

    private void SetChefControlsEnabled(bool enabled)
    {
        if (VRRigReferences.Singleton.leftControllerObject != null)
            VRRigReferences.Singleton.leftControllerObject.SetActive(enabled);

        if (VRRigReferences.Singleton.rightControllerObject != null)
            VRRigReferences.Singleton.rightControllerObject.SetActive(enabled);
    }

        // Update is called once per frame
        void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position;
            head.rotation = VRRigReferences.Singleton.head.rotation;

            leftHand.position = VRRigReferences.Singleton.leftHand.position;
            leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

            rightHand.position = VRRigReferences.Singleton.rightHand.position;
            rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
        }
    }
}
