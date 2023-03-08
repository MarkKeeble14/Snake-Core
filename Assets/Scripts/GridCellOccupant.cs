using UnityEngine;

public class GridCellOccupant : MonoBehaviour
{
    [SerializeField] protected GridCell currentCell;
    public GridCell CurrentCell => currentCell;
    [SerializeField] protected GridCell previousCell;
    public GridCell PreviousCell => previousCell;
    [SerializeField] private bool lockToCell = true;
    [SerializeField] private bool obstruction;
    public bool isObstruction => obstruction;
    public bool IsInstantLoss { get; set; }
    [SerializeField] private bool triggersEvents;
    [SerializeField] private bool hasEvents;
    public bool HasEvents => hasEvents;

    private void Awake()
    {
        if (TryGetComponent(out DestroySelfTriggerEvent destroySelf))
        {
            destroySelf.AddOnDestroyCallback(() => currentCell.RemoveOccupant(this));
        }
    }

    protected void Update()
    {
        if (lockToCell)
            transform.position = currentCell.transform.position + (Vector3.up * transform.localScale.y / 2);
    }

    public virtual void ChangeCell(GridCell nextCell)
    {
        if (triggersEvents
            && nextCell.isOccupied)
        {
            nextCell.TriggerEvents();
        }

        // Debug.Log("Previous: " + previousCell + ", Current: " + currentCell);
        previousCell = currentCell;
        currentCell.RemoveOccupant(this);
        nextCell.AddOccupant(this);
        currentCell = nextCell;
    }

    public void SetToCell(GridCell cell)
    {
        currentCell = cell;
    }

    public void TriggerEvents()
    {
        foreach (TriggerEvent triggerEvent in GetComponents<TriggerEvent>())
        {
            triggerEvent.Activate();
            if (GridGenerator._Instance.DoublingEventTriggers && triggerEvent.AllowDoubling)
            {
                triggerEvent.Activate();
            }
        }
    }
}
