public class TelportTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        SnakeBehaviour._Instance.Teleport();
    }
}
