using UnityEngine;

public class DoubleTriggerEventsTriggerEvent : TriggerEvent
{
    [SerializeField] private FloatStore durationChange;

    public override void Activate()
    {
        GridGenerator._Instance.DoubleEventTriggers(durationChange.Value);
    }
}