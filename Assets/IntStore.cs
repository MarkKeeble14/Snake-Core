using UnityEngine;

[CreateAssetMenu(fileName = "IntStore", menuName = "IntStore")]
public class IntStore : ScriptableObject
{
    public int Value { get; set; }
    [SerializeField] private int defaultValue;
    public void Reset()
    {
        Value = defaultValue;
    }
}
