public class SpawnFoodTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        GridGenerator._Instance.SpawnFood();
    }
}

