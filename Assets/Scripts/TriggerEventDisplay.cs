using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TriggerEventDisplay : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI labelText;

    public void Set(StoredTriggerEventDisplayInfo info)
    {
        labelText.text = info.Label;
        image.color = info.Color;
        image.sprite = info.Sprite;
    }
}