using UnityEngine;

public class AlterIntStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private int changeBy;
    [SerializeField] private IntStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + changeBy + detailsSuffix, delegate
        {
            Debug.Log("Alter Int Store: " + store.name + ": Pre-Alter: " + store.Value);
            store.Value += changeBy;
            Debug.Log("Alter Int Store: " + store.name + ": Pre-Alter: " + store.Value);
        });
    }
}
