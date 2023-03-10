using UnityEngine;

public abstract class OnSelectCardAction : MonoBehaviour
{
    [SerializeField] protected string label;
    [SerializeField] protected string detailsPrefix;
    [SerializeField] protected string detailsSuffix;
    [SerializeField] protected int cost;
    [SerializeField] protected IntStore coins;

    private void Awake()
    {
        SetCard(GetComponent<SelectionCard>());
    }

    public abstract void SetCard(SelectionCard card);
}
