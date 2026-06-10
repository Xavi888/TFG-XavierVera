using UnityEngine;
using Unity.Netcode;

public class TestOwner : NetworkBehaviour
{
    void Start()
    {
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        if (IsOwnedByServer)
        {
            transform.position = new Vector3(-0.8f, 0.2f, 0.7f);
        }
        else
        {
            transform.position = new Vector3(-0.8f, 0.2f, 0.2f);
        }
    }
}
