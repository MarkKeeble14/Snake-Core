public class TeleportTriggerEvent : TriggerEvent
{
    private GridCell teleportTo;

    public void SetTeleportTo(GridCell cell)
    {
        teleportTo = cell;
    }

    public override void Activate()
    {
        SnakeBehaviour._Instance.Teleport(teleportTo);
    }
}
