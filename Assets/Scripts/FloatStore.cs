using UnityEngine;

[CreateAssetMenu(fileName = "FloatStore", menuName = "FloatStore")]
public class FloatStore : ScriptableObject
{
    public float Value { get; set; }
    [SerializeField] private float defaultValue;
    public void Reset()
    {
        Value = defaultValue;
    }
}
