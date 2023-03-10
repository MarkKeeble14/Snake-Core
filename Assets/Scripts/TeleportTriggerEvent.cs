public class TeleportTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        SnakeBehaviour._Instance.Teleport();
    }
}
