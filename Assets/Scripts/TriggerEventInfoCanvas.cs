using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TriggerEventInfoCanvas : MonoBehaviour
{
    private TextMeshProUGUI text;
    [SerializeField] private string eventType;
    [SerializeField] private bool quantitative;
    [SerializeField] private NumStore numStore;

    private void Awake()
    {
        // Get reference
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (quantitative)
        {
            int quantity = Mathf.RoundToInt(numStore.GetValue() * GridGenerator._Instance.EventTriggerRepeats);
            text.text = "+" + quantity.ToString() + eventType + (quantity > 1 ? "s" : "");
        }
        else
        {
            text.text = eventType;
        }
    }
}
