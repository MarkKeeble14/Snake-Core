using UnityEngine;

[CreateAssetMenu(fileName = "FloatStore", menuName = "FloatStore")]
public class FloatStore : NumStore
{
    public float Value { get; set; }
    [SerializeField] private float defaultValue;
    public override void Reset()
    {
        Value = defaultValue;
    }

    public override float GetValue()
    {
        return Value;
    }
}
