using UnityEngine;

public class AlterFloatStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private float changeBy;
    [SerializeField] private FloatStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + changeBy + detailsSuffix, delegate
        {
            Debug.Log("Alter Float Store: " + store.name + ": Pre-Alter: " + store.Value);
            store.Value += changeBy;
            Debug.Log("Alter Float Store: " + store.name + ": Post-Alter: " + store.Value);
        });
    }
}
