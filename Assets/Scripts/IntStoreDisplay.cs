using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntStoreDisplay : StoreDisplay
{
    [SerializeField] private IntStore store;
    [SerializeField] private bool showAtZero;

    protected override bool Enabled
    {
        get
        {
            if (showAtZero)
            {
                return true;
            }
            else
            {
                return store.Value > 0;
            }
        }
    }

    protected override string storeValue => store.Value.ToString();
}
