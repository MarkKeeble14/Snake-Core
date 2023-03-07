public class AddAllowedCollisionTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        SnakeBehaviour._Instance.AddAllowedCollision();
    }
}

