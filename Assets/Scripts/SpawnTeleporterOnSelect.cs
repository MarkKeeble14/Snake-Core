using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTeleporterOnSelect : OnSelectCardAction
{
    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + detailsSuffix, () => GridGenerator._Instance.SpawnTeleporter());
    }
}
