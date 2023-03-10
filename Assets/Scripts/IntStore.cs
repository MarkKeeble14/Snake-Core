using UnityEngine;

[CreateAssetMenu(fileName = "IntStore", menuName = "IntStore")]
public class IntStore : NumStore
{
    public int Value { get; set; }
    [SerializeField] private int defaultValue;
    public override void Reset()
    {
        Value = defaultValue;
    }

    public override float GetValue()
    {
        return Value;
    }
}
