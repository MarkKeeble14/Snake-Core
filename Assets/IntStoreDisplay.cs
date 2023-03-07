using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntStoreDisplay : MonoBehaviour
{
    [SerializeField] private IntStore intStore;
    [SerializeField] private string prefix;
    [SerializeField] private string suffix;
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.text = prefix + intStore.Value + suffix;
    }
}
