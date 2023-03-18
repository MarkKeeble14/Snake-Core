using UnityEngine;

public class AlterIntStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private int changeBy;
    [SerializeField] private IntStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + changeBy + detailsSuffix, () => store.Value += changeBy);
    }
}
