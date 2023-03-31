using UnityEngine;

public class AlterFloatStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private float changeBy;
    [SerializeField] private FloatStore store;
    [SerializeField] private bool insertValue = true;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + (insertValue ? changeBy.ToString() : "") + detailsSuffix, delegate
        {
            store.Value += changeBy;
        });
    }
}
