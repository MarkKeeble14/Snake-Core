using UnityEngine;

public class AlterFloatStoreOnSelect : OnSelectCardAction
{
    [SerializeField] private float changeBy;
    [SerializeField] private FloatStore store;

    public override void SetCard(SelectionCard card)
    {
        card.Set(coins.Value > cost, label, detailsPrefix + changeBy + detailsSuffix, cost, () => store.Value += changeBy);
    }
}
