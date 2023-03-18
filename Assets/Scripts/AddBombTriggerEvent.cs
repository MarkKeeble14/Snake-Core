using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBombTriggerEvent : TriggerEvent
{
    [SerializeField] private IntStore bombs;
    [SerializeField] private IntStore valueChange;

    public override void Activate()
    {
        bombs.Value += valueChange.Value;
    }
}
