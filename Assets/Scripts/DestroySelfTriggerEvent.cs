using System;
using UnityEngine;

public class DestroySelfTriggerEvent : TriggerEvent
{
    private Action onDestroy;

    public void AddOnDestroyCallback(Action action)
    {
        onDestroy += action;
    }

    public override void Activate()
    {
        onDestroy?.Invoke();
        Destroy(gameObject);
    }
}

