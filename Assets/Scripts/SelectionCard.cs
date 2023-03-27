using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectionCard : MonoBehaviour
{
    private Action onSelect;

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI details;
    private Image cardBackground;

    [SerializeField] private CardType type;

    public void AddOnSelectAction(Action action)
    {
        onSelect += action;
    }

    public void Select()
    {
        if (UIManager._Instance.InSelectionPopGracePeriod)
        {
            UIManager._Instance.OnFailSelectCard.PlayOneShot();
            return;
        }
        UIManager._Instance.CardTypeDetailsDictionary[type].onSelect.PlayOneShot();
        onSelect?.Invoke();
    }

    public void Set(string label, string details, Action p)
    {
        // Get reference
        cardBackground = transform.GetChild(0).GetComponent<Image>();

        // Set UI
        this.label.text = label;
        this.details.text = details;

        // Set Action
        AddOnSelectAction(p);

        // Set Card Color
        Color c = UIManager._Instance.CardTypeDetailsDictionary[type].color;
        c.a = .5f;
        cardBackground.color = c;
    }
}
