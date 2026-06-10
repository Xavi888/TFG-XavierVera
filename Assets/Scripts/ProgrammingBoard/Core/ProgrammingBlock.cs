using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public  class ProgrammingBlock : MonoBehaviour
{
    public ProgrammingBlockType BlockType {
        get; protected set;
    }

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    public delegate void BlockGrabbedHandler(ProgrammingBlock block);
    public event BlockGrabbedHandler OnBlockGrabbed;

    protected virtual void Start() {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(selectEnteredHandler);
        grabInteractable.selectExited.AddListener(selectExitedHandler);
    }

    private void selectEnteredHandler(SelectEnterEventArgs selectEnterEventArgs) {   
        if (CompareTag("ProgrammingBlockOnBoard")) {
            OnBlockGrabbed?.Invoke(this);
        } 
    }

    private void selectExitedHandler(SelectExitEventArgs selectExitEventArgs) {
        tag = "ProgrammingBlock";
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    public virtual List<IngredientType> Execute(){
        return null;
    } 

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Floor")) {
            Destroy(gameObject);
        }
    }

}
