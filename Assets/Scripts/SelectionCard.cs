using System;
using UnityEngine;
using TMPro;

public class SelectionCard : MonoBehaviour
{
    private Action onSelect;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI details;
    [SerializeField] private TextMeshProUGUI cost;
    private CanvasGroup canvasGroup;
    [SerializeField] private float insufficientFundsAlpha = .45f;

    private void Awake()
    {
        // Get reference
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void AddOnSelectAction(Action action)
    {
        onSelect += action;
    }

    public void Select()
    {
        onSelect?.Invoke();
    }

    public void Set(bool buyable, string label, string details, int cost, Action p)
    {
        if (buyable)
        {
            this.label.text = label;
            this.details.text = details;
            this.cost.text = "-" + cost + " Coins";
            AddOnSelectAction(p);
        }
        else
        {
            this.label.text = "";
            this.details.text = "Not Enough Money";
            this.details.color = Color.red;
            this.cost.text = "-" + cost + " Coins";
            onSelect = null;
            canvasGroup.alpha = insufficientFundsAlpha;
        }
    }
}
