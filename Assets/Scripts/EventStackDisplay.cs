using UnityEngine;
using System.Collections.Generic;

public class EventStackDisplay : MonoBehaviour
{
    [SerializeField] private TriggerEventDisplay displayPrefab;
    [SerializeField] private Transform list;

    private Stack<GameObject> spawnedDisplays = new Stack<GameObject>();

    public void Push(StoredTriggerEventDisplayInfo info)
    {
        TriggerEventDisplay spawned = Instantiate(displayPrefab, list);
        spawnedDisplays.Push(spawned.gameObject);
        spawned.Set(info);
    }

    public void Pop()
    {
        GameObject spawned = spawnedDisplays.Pop();
        Destroy(spawned);
    }
}
