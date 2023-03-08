using UnityEngine;

public class DoubleEventTriggersTriggerEvent : TriggerEvent
{
    [SerializeField] private float duration;

    public override void Activate()
    {
        GridGenerator._Instance.DoubleEventTriggers(duration);
    }
}