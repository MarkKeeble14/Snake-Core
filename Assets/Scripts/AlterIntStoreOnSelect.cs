using UnityEngine;

public class AlterIntStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private int changeBy;
    [SerializeField] private IntStore store;
    [SerializeField] private bool insertValue = true;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + (insertValue ? changeBy.ToString() : "") + detailsSuffix, delegate
        {
            store.Value += changeBy;
        });
    }
}
