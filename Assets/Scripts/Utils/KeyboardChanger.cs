using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Keyboard;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardChanger : MonoBehaviour
{
    private KeyboardManager keyboardManager;
    private TMP_InputField inputField;

    private void Start() {
        inputField = GetComponent<TMP_InputField>();
        keyboardManager = GameObject.FindWithTag("Keyboard").GetComponent<KeyboardManager>();
        inputField.onSelect.AddListener(OnSelectHandler);
    }
    public void OnSelectHandler(string arg0){
        keyboardManager.outputField = this.GetComponent<TMP_InputField>();
    }
}
