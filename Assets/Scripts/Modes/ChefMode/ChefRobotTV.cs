using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class ChefRobotTV : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text centerText;
    [SerializeField] private TMP_Text dayEndedText;
    [SerializeField] private GameObject deliveredPanel;
    [SerializeField] private GameObject dayEndedPanel;
    void Start()
    {   
        DisplayDelivered(false);
        DisplayDayEnded(false);
    }

    public void Clear() {
        DisplayDelivered(false);
        DisplayDayEnded(false);
    }

    public void DisplayTime(float seconds){
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);

        string timeString = string.Format("{0:D2}:{1:D2} H", hours, minutes);

        clockText.text = timeString;
    }

    public void SetCenterText(String text) {
        centerText.text = text;
    }

    public void ClearCenterText() {
        centerText.text = "";
    }

    public void SetDayEndedText(String text) {
        dayEndedText.text = text;
    }

    public void ClearDayEndedText() {
        dayEndedText.text = "";
    }

    public void DisplayDelivered(bool display) {
        deliveredPanel.SetActive(display);
    }
    public void DisplayDayEnded(bool display) {
        dayEndedPanel.SetActive(display);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
