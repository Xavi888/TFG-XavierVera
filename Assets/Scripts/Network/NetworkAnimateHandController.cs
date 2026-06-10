using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NetworkAnimateHandController : NetworkBehaviour
{
    public InputActionReference gripInputActionReference;
    public InputActionReference triggerInputActionReference;

    private Animator _handAnimator;
    private float _gripValue;
    private float _triggerValue;

    private void Start()
    {
        _handAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (IsOwner)
        {
        AnimateGrip();
        AnimateTrigger();
        }
    }

    private void AnimateGrip()
    {
        _gripValue = gripInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat("Grip", _gripValue);
    }

    private void AnimateTrigger()
    {
        _triggerValue = triggerInputActionReference.action.ReadValue<float>();
        _handAnimator.SetFloat("Trigger", _triggerValue);
    }
}
