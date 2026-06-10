using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public GameObject leftControllerObject;
    public GameObject rightControllerObject;

    private void Awake()
    {
        Singleton = this;
    }
}
