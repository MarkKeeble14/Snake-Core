public class GrowSnakeTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        SnakeBehaviour._Instance.Grow();
    }
}
