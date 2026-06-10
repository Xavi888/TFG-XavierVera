using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class WaiterRobotTV : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text orderCreatedText;
    [SerializeField] private TMP_Text orderDeliveredText;
    [SerializeField] private TMP_Text dayEndedText;
    [SerializeField] private GameObject orderCreatedPanel;
    [SerializeField] private GameObject orderDeliveredPanel;
    [SerializeField] private GameObject dayEndedPanel;
    void Awake()
    {
        DisplayOrderCreated(false);
        DisplayOrderDelivered(false);
        DisplayDayEnded(false);
    }

    public void Clear() {
        DisplayOrderCreated(false);
        DisplayOrderDelivered(false);
        DisplayDayEnded(false);
    }

    public void DisplayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);

        string timeString = string.Format("{0:D2}:{1:D2} H", hours, minutes);

        clockText.text = timeString;
    }

    public void SetOrderCreatedText(String text)
    {
        orderCreatedText.text = text;
    }

    public void ClearOrderCreatedText()
    {
        orderCreatedText.text = "";
    }

    public void SetOrderDeliveredText(String text)
    {
        orderDeliveredText.text = text;
    }
    public void ClearOrderDeliveredText()
    {
        orderDeliveredText.text = "";
    }

    public void SetDayEndedText(String text)
    {
        dayEndedText.text = text;
    }

    public void ClearDayEndedText()
    {
        dayEndedText.text = "";
    }

    public void DisplayOrderCreated(bool display)
    {
        orderCreatedPanel.SetActive(display);
    }
    public void DisplayOrderDelivered(bool display)
    {
        orderDeliveredPanel.SetActive(display);
    }
    public void DisplayDayEnded(bool display)
    {
        dayEndedPanel.SetActive(display);
    }
}
