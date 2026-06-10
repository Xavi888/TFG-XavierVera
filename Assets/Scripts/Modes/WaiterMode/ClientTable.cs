using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientTable : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text orderText;

    public delegate void TableActivatedHandler();
    public delegate void TableOrderDeliveredHandler();
    public event TableActivatedHandler OnTableActivated;
    public event TableOrderDeliveredHandler OnTableOrderDelivered;


    private bool Active { get; set; } = false;

    private void Awake() {
        DisplayPanel(false);
    }


    public void DisplayPanel(bool display) {
        panel.SetActive(display);
    }

    public void SetOrderText(string text) {
        orderText.text = text;
    }

    public void ClearOrderText() {
        orderText.text = "";
    }

    

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Collision detected");
        if (other.gameObject.CompareTag("Player") && !Active) {
            Debug.Log("El jugador ha hecho colisión con la mesa.");
            OnTableActivated?.Invoke();
            Active = true;
        }
        if (other.gameObject.CompareTag("Tray") && Active) {
            Debug.Log("La mesa ha hecho colisión con la caja.");
            OnTableOrderDelivered?.Invoke();
            Active = false;
        }
    }

    

    
}
