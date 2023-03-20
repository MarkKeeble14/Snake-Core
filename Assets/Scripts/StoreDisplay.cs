using UnityEngine;
using TMPro;

public abstract class StoreDisplay : MonoBehaviour
{
    [SerializeField] private string prefix;
    [SerializeField] private string suffix;

    protected virtual bool Enabled
    {
        get
        {
            return true;
        }
    }

    protected abstract string storeValue { get; }
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.enabled = Enabled;
        text.text = prefix + storeValue + suffix;
    }
}
