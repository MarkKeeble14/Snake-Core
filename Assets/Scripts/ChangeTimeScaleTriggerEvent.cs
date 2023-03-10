using UnityEngine;

public class ChangeTimeScaleTriggerEvent : TriggerEvent
{
    [SerializeField] private FloatStore alterDuration;
    [SerializeField] private FloatStore alterTimeScale;

    public override void Activate()
    {
        GridGenerator._Instance.SlowTime(alterTimeScale.Value, alterDuration.Value);
    }
}
