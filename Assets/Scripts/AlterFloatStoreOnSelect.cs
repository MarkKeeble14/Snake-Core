using UnityEngine;

public class AlterFloatStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private float changeBy;
    [SerializeField] private FloatStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + changeBy + detailsSuffix, delegate
        {
            store.Value += changeBy;
        });
    }
}
