using UnityEngine;

public class AddAllowedCollisionTriggerEvent : TriggerEvent
{
    [SerializeField] private IntStore allowedCollisionsChange;

    public override void Activate()
    {
        SnakeBehaviour._Instance.AddAllowedCollision(allowedCollisionsChange.Value);
    }
}