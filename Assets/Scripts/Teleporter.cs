public class Teleporter : GridCellOccupant
{
    private TeleportTriggerEvent triggerEvent;

    public void Link(GridCell linkedCell, GridCellOccupant exit)
    {
        if (!triggerEvent) triggerEvent = GetComponent<TeleportTriggerEvent>();

        triggerEvent.SetTeleportTo(linkedCell);
        AddOnDestroyCallback(() => exit.Break());
    }
}