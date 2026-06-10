using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderButton : MonoBehaviour
{
    //private Animator buttonAnimator;
    private Collider buttonCollider;
    private float deadtime = 1.0f;

    private bool deadtimeActive = false;

    public delegate void ButtonPressedHandler();
    public event ButtonPressedHandler OnButtonPressed;

    private void Start()
    {
        //buttonAnimator = GetComponent<Animator>();
        buttonCollider = GetComponent<Collider>();
        //buttonAnimator.StopPlayback();
    }

    private void OnCollisionEnter(Collision other)
    {
        if  (other.gameObject.CompareTag("Button") && !deadtimeActive) {
            OnButtonPressed?.Invoke();
        }
    }

    private void OnCollisionExit (Collision other) {
        if  (other.gameObject.CompareTag("Button") && !deadtimeActive) {
            StartCoroutine(WaitForDeadTime());
        }
    }

    IEnumerator WaitForDeadTime() {
        deadtimeActive = true;
        yield return new WaitForSeconds(deadtime);
        deadtimeActive = false;
    }
}
