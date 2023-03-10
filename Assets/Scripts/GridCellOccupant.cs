using System;
using UnityEngine;

public class GridCellOccupant : MonoBehaviour
{
    [SerializeField] protected GridCell currentCell;
    public GridCell CurrentCell => currentCell;
    [SerializeField] protected GridCell previousCell;
    public GridCell PreviousCell => previousCell;
    [SerializeField] private bool lockToCell = true;
    [SerializeField] private bool obstruction;
    public bool IsObstruction => obstruction;
    [SerializeField] private bool destroyable;
    public bool IsDestroyable => destroyable;
    public bool IsBorderWall { get; set; }
    [SerializeField] private bool triggersEvents;
    [SerializeField] private bool hasEvents;
    public bool HasEvents => hasEvents;
    [SerializeField] private bool isSnake;
    public bool IsSnake => isSnake;

    [SerializeField] private bool breakOnEventsTriggered = true;
    protected Action onDestroy;

    protected void Update()
    {
        if (lockToCell)
        {
            transform.position = currentCell.transform.position + (Vector3.up * transform.localScale.y / 2);
        }
    }

    public virtual void ChangeCell(GridCell nextCell)
    {
        // Debug.Log("Previous: " + previousCell + ", Current: " + currentCell);
        previousCell = currentCell;
        currentCell.RemoveOccupant(this);
        nextCell.AddOccupant(this);
        currentCell = nextCell;

        if (triggersEvents)
        {
            currentCell.TriggerEvents();
        }
    }

    public void SetToCell(GridCell cell)
    {
        currentCell = cell;
    }

    public void TriggerEvents()
    {
        foreach (TriggerEvent triggerEvent in GetComponents<TriggerEvent>())
        {
            for (int i = 0; i < GridGenerator._Instance.EventTriggerRepeats; i++)
            {
                triggerEvent.Activate();
                if (!triggerEvent.AllowDoubling) break;
            }
        }
        if (breakOnEventsTriggered)
            Break();
    }

    public void AddOnDestroyCallback(Action callback)
    {
        onDestroy += callback;
    }

    public virtual void Break()
    {
        currentCell.RemoveOccupant(this);
        onDestroy?.Invoke();
        Destroy(gameObject);
    }
}
