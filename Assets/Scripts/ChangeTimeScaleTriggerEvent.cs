using UnityEngine;

public class ChangeTimeScaleTriggerEvent : TriggerEvent
{
    [SerializeField] private float duration;
    [SerializeField] private float changeTimeTo;

    public override void Activate()
    {
        GridGenerator._Instance.SlowTime(changeTimeTo, duration);
    }
}
