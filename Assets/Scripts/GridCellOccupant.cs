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
            TriggerEventData data = triggerEvent.EventData;

            // Can be doubled
            if (data.CanBeDoubled)
            {
                if (data.CanBeStored) // Can be doubled and stored
                {
                    GridGenerator._Instance.AddEventToStack(delegate
                    {
                        for (int i = 0; i < GridGenerator._Instance.EventTriggerRepeats; i++)
                        {
                            triggerEvent.Activate();
                        }
                    }, triggerEvent.StoredDisplayInfo);
                }
                else // Can be doubled but not stored
                {
                    for (int i = 0; i < GridGenerator._Instance.EventTriggerRepeats; i++)
                    {
                        triggerEvent.Activate();
                    }
                }
            }
            else // Can't be Doubled
            {
                if (data.CanBeStored) // Can't be doubled but stored
                {
                    GridGenerator._Instance.AddEventToStack(() => triggerEvent.Activate(), triggerEvent.StoredDisplayInfo);
                }
                else // Can't be doubled and can't be stored
                {
                    triggerEvent.Activate();
                }
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
