using System;
using UnityEngine;

public class SpawnCoinEventTrigger : TriggerEvent
{
    public override void Activate()
    {
        GridGenerator._Instance.SpawnCoin();
    }
}
