using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerEvent : MonoBehaviour
{
    [SerializeField] private bool allowDoubling = true;
    public bool AllowDoubling => allowDoubling;
    public abstract void Activate();
}

