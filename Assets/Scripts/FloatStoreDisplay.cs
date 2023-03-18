using System;
using UnityEngine;

public class FloatStoreDisplay : StoreDisplay
{
    [SerializeField] private FloatStore store;
    [SerializeField] private bool round;
    [SerializeField] private int numDigits;

    protected override bool Enabled => store.Value > 0;

    protected override string storeValue
    {
        get
        {
            if (round)
            {
                return Math.Round(store.Value, numDigits).ToString();
            }
            return store.Value.ToString();
        }
    }
}
