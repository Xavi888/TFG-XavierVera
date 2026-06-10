using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public delegate void PlateContactHandler();
    public event PlateContactHandler OnPlateContact;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plate"))
        {
            OnPlateContact?.Invoke();
        }
    }
}