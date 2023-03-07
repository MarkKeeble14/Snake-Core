using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddScoreTriggerEvent : TriggerEvent
{
    [SerializeField] private IntStore coinStore;
    [SerializeField] private int amount;

    public override void Activate()
    {
        coinStore.Value += amount;
    }
}
