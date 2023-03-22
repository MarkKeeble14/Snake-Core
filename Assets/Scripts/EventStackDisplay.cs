using UnityEngine;
using System.Collections.Generic;

public class EventStackDisplay : MonoBehaviour
{
    [SerializeField] private TriggerEventDisplay displayPrefab;
    [SerializeField] private Transform list;

    private Queue<GameObject> spawnedDisplays = new Queue<GameObject>();

    public void Push(StoredTriggerEventDisplayInfo info)
    {
        TriggerEventDisplay spawned = Instantiate(displayPrefab, list);
        spawnedDisplays.Enqueue(spawned.gameObject);
        spawned.Set(info);
    }

    public void Dequeue()
    {
        GameObject spawned = spawnedDisplays.Dequeue();
        Destroy(spawned);
    }
}
