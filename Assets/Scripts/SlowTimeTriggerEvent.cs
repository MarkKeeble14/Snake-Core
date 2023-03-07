using UnityEngine;

public class SlowTimeTriggerEvent : TriggerEvent
{
    [SerializeField] private float duration;

    public override void Activate()
    {
        GridGenerator._Instance.SlowTime(duration);
    }
}

