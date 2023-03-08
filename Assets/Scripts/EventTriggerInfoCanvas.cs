using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventTriggerInfoCanvas : MonoBehaviour
{
    private TextMeshProUGUI text;
    [SerializeField] private string eventType;
    [SerializeField] private bool quantitative;

    private void Awake()
    {
        // Get reference
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        int quantity = (GridGenerator._Instance.DoublingEventTriggers ? 2 : 1);
        // Set text
        text.text =
            (
                (quantitative ? "+" + quantity.ToString() : "")
                + eventType
                + (quantitative ? (quantity > 1 ? "s" : "") : "")
            );
    }
}
