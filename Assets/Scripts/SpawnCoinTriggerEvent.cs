using System;
using UnityEngine;

public class SpawnCoinTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        GridGenerator._Instance.SpawnCoin(1);
    }
}
