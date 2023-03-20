using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerEvent : MonoBehaviour
{
    [SerializeField] private TriggerEventData eventData;
    public TriggerEventData EventData => eventData;
    [SerializeField] private StoredTriggerEventDisplayInfo storedDisplayInfo;
    public StoredTriggerEventDisplayInfo StoredDisplayInfo => storedDisplayInfo;
    public abstract void Activate();
}

