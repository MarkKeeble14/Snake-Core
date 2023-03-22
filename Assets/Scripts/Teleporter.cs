using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Teleporter : GridCellOccupant
{
    private TeleportTriggerEvent triggerEvent;

    public void Link(GridCell linkedCell, GridCellOccupant exit)
    {
        if (!triggerEvent) triggerEvent = GetComponent<TeleportTriggerEvent>();

        triggerEvent.SetTeleportTo(linkedCell);

        LineRenderer r = GetComponent<LineRenderer>();
        r.positionCount = 2;
        r.SetPosition(0, transform.position + Vector3.up);
        r.SetPosition(1, linkedCell.transform.position + Vector3.up);

        AddOnDestroyCallback(() => exit.Break());
    }
}