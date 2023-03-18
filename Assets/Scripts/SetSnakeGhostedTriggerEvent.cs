using UnityEngine;

public class SetSnakeGhostedTriggerEvent : TriggerEvent
{
    [SerializeField] private FloatStore duration;

    public override void Activate()
    {
        SnakeBehaviour._Instance.SetGhost(duration.Value);
    }
}