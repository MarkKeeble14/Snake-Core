using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private bool round;
    [SerializeField] private int numDigits;

    // Update is called once per frame
    void Update()
    {
        text.text = (round ? System.Math.Round(GridGenerator._Instance.GameDuration, numDigits) : GridGenerator._Instance.GameDuration).ToString();
    }
}