using UnityEngine;

public class AlterIntStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private int changeBy;
    [SerializeField] private IntStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(coins.Value > cost, label, detailsPrefix + changeBy + detailsSuffix, cost, () => store.Value += changeBy);
    }
}
