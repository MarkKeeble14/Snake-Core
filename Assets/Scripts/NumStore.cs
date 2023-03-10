using UnityEngine;

public abstract class NumStore : ScriptableObject
{
    public abstract void Reset();

    public abstract float GetValue();
}
